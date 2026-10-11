using System;
using System.Collections.Generic;
using System.IO;
using cfEngine.IO;
using cfEngine.Serialize;

namespace cfEngine.Info
{
    /// <summary>
    /// Debug/editor-only override cache for Info tables.
    ///
    /// One cache owns a single dedicated physical directory (already globalized by the caller,
    /// e.g. <c>user://Info</c>). <see cref="Recache"/> wipes that directory and copies every
    /// canonical <c>**/data.json</c> file into it; <see cref="CreateLoader{TInfo}"/> then builds
    /// loaders that read the override copy instead of the canonical source, so runtime tweaks
    /// survive a reload but never a restart.
    ///
    /// The whole capability is a compile-time boundary (<c>#if DEBUG || TOOLS</c>) combined with
    /// the runtime <c>enableOverrides</c> flag: a non-debug build always selects canonical storage
    /// and never evaluates the canonical reader, inspects the root or touches the filesystem.
    /// Engine-free: no Godot types, no URI parsing, no game info names.
    /// </summary>
    public sealed class InfoOverrideCache
    {
        private readonly bool _overridesEnabled;
        private readonly string? _overrideRoot;
        private readonly Func<IReadOnlyDictionary<string, byte[]>>? _readCanonical;

        /// <param name="enableOverrides">
        /// Caller gate. Only honoured by a debug/tools build; a release build ignores it entirely.
        /// </param>
        /// <param name="overrideRoot">
        /// Absolute physical path of the dedicated override directory (already globalized).
        /// Validated only when overrides are actually enabled; never inspected otherwise.
        /// </param>
        /// <param name="readCanonical">
        /// Returns the canonical inventory keyed by normalized relative path
        /// (for example <c>FeverInfo/data.json</c>), including nested canonical <c>data.json</c>
        /// files. Only invoked by an enabled debug/tools build.
        /// </param>
        public InfoOverrideCache(bool enableOverrides, string overrideRoot, Func<IReadOnlyDictionary<string, byte[]>> readCanonical)
        {
#if DEBUG || TOOLS
            _overridesEnabled = enableOverrides;
            _overrideRoot = enableOverrides ? NormalizeOverrideRoot(overrideRoot) : null;
            _readCanonical = readCanonical;
#else
            _overridesEnabled = false;
            _overrideRoot = null;
            _readCanonical = null;
#endif
        }

        /// <summary>
        /// Deletes the whole override directory and repopulates it from the canonical inventory.
        /// No stale table or file survives. No-op when overrides are disabled.
        /// </summary>
        public void Recache()
        {
            if (!_overridesEnabled)
            {
                return;
            }

            RecacheCore(tableName: null);
        }

        /// <summary>
        /// Creates the loader a manager should read from: an override-backed loader when
        /// overrides are enabled, otherwise one bound to <paramref name="canonicalStorage"/>.
        /// The override loader carries a per-table recache action, so <see cref="IInfoManager.Revert"/>
        /// refreshes only that table's subtree.
        /// </summary>
        public SerializationLoader<TInfo> CreateLoader<TInfo>(string tableName, IStorage canonicalStorage, ISerializer serializer)
        {
            if (canonicalStorage == null)
            {
                throw new ArgumentNullException(nameof(canonicalStorage));
            }

            if (!_overridesEnabled)
            {
                return new SerializationLoader<TInfo>(canonicalStorage, serializer);
            }

            var name = NormalizeTableName(tableName);
            var overrideStorage = new LocalFileStorage(Path.Combine(_overrideRoot!, name));
            return new SerializationLoader<TInfo>(overrideStorage, serializer, () => RecacheCore(name));
        }

        private void RecacheCore(string? tableName)
        {
            var root = _overrideRoot;
            if (root == null)
            {
                return;
            }

            // Inventory first: nothing is deleted until every canonical path and payload is readable.
            var entries = new List<(string RelativePath, byte[] Bytes)>();
            foreach (var entry in ReadCanonicalInventory())
            {
                var relativePath = NormalizeRelativePath(entry.Key);
                if (!IsDataFile(relativePath))
                {
                    continue;
                }

                if (tableName != null && !IsInTable(relativePath, tableName))
                {
                    continue;
                }

                entries.Add((relativePath, entry.Value ?? Array.Empty<byte>()));
            }

            // Only the target subtree is dropped: a per-table revert must not discard tweaks
            // recorded in any other table.
            var targetRoot = tableName == null ? root : EnsureInsideRoot(Path.Combine(root, tableName));
            if (Directory.Exists(targetRoot))
            {
                Directory.Delete(targetRoot, true);
            }

            Directory.CreateDirectory(targetRoot);

            foreach (var (relativePath, bytes) in entries)
            {
                var destination = EnsureInsideRoot(Path.Combine(root, relativePath));
                var destinationDirectory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.WriteAllBytes(destination, bytes);
            }
        }

        private IReadOnlyDictionary<string, byte[]> ReadCanonicalInventory()
        {
            var readCanonical = _readCanonical;
            var canonical = readCanonical?.Invoke();
            if (canonical == null)
            {
                throw new InvalidOperationException("canonical data reader is not configured");
            }

            return canonical;
        }

        private static bool IsDataFile(string relativePath)
        {
            var fileName = relativePath.Substring(relativePath.LastIndexOf('/') + 1);
            return string.Equals(fileName, "data.json", StringComparison.Ordinal);
        }

        private static bool IsInTable(string relativePath, string tableName)
            => relativePath.StartsWith(tableName + "/", StringComparison.Ordinal);

        /// <summary>Rejects anything that is not a single, plain directory name.</summary>
        private static string NormalizeTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("tableName must be a non-empty directory name", nameof(tableName));
            }

            if (tableName is "." or ".."
                || Path.IsPathRooted(tableName)
                || tableName.IndexOfAny(new[] { '/', '\\', ':' }) >= 0
                || tableName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException($"unsafe table name: {tableName}", nameof(tableName));
            }

            return tableName;
        }

        /// <summary>Normalizes a canonical relative path, rejecting rooted and traversal forms.</summary>
        private static string NormalizeRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("canonical path must be a non-empty relative path", nameof(relativePath));
            }

            if (Path.IsPathRooted(relativePath) || relativePath.IndexOf(':') >= 0)
            {
                throw new ArgumentException($"canonical path must be relative: {relativePath}", nameof(relativePath));
            }

            var segments = relativePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                throw new ArgumentException($"canonical path must name a file: {relativePath}", nameof(relativePath));
            }

            foreach (var segment in segments)
            {
                if (segment is "." or "..")
                {
                    throw new ArgumentException($"unsafe canonical path: {relativePath}", nameof(relativePath));
                }
            }

            return string.Join("/", segments);
        }

        /// <summary>Rejects a filesystem/volume root: the cache may never wipe a whole volume.</summary>
        private static string NormalizeOverrideRoot(string overrideRoot)
        {
            if (string.IsNullOrWhiteSpace(overrideRoot))
            {
                throw new ArgumentException("overrideRoot must be a non-empty absolute path", nameof(overrideRoot));
            }

            if (!Path.IsPathRooted(overrideRoot))
            {
                throw new ArgumentException($"overrideRoot must be absolute: {overrideRoot}", nameof(overrideRoot));
            }

            var full = Path.GetFullPath(overrideRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var volumeRoot = Path.GetPathRoot(full)?
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (full.Length == 0 || string.Equals(full, volumeRoot, StringComparison.Ordinal))
            {
                throw new ArgumentException($"overrideRoot must not be a filesystem root: {overrideRoot}", nameof(overrideRoot));
            }

            return full;
        }

        /// <summary>Refuses any destination outside the dedicated override root.</summary>
        private string EnsureInsideRoot(string path)
        {
            var full = Path.GetFullPath(path);
            var root = _overrideRoot!;
            var rootPrefix = root + Path.DirectorySeparatorChar;
            if (!full.StartsWith(rootPrefix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"refusing to write outside the override root: {full}");
            }

            return full;
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;

namespace cfEngine.IO
{
    public class LocalFileStorage: Storage
    {
        public LocalFileStorage(string storagePath): base(storagePath)
        {
        }
        
        public override string[] GetFiles(string searchPattern)
        {
            return Directory.GetFiles(storagePath, searchPattern, SearchOption.AllDirectories);
        }

        public override bool IsFileExist(string relativePath)
        {
            return File.Exists(Path.Combine(storagePath, relativePath));
        }

        public override void CopyFile(string relativeFrom, string relativeTo, bool overwrite = false)
        {
            File.Copy(Path.Combine(storagePath, relativeFrom), Path.Combine(storagePath, relativeTo), overwrite);
        }

        public override void DeleteFile(string relativePath)
        {
            File.Delete(Path.Combine(storagePath, relativePath));
        }

        public override byte[] LoadBytes(string relativePath)
        {
            var absPath = Path.Combine(storagePath, relativePath);

            using var fileStream = new FileStream(absPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite, 1024, false);

            var fileBytes = new byte[fileStream.Length];
            var byteLoaded = fileStream.Read(fileBytes, 0, (int)fileStream.Length);
            if (byteLoaded > fileStream.Length)
            {
                throw new InvalidOperationException($"Detect fileStream read size ({fileStream.Length} differ from byte load ({byteLoaded}))");
            }

            return fileBytes;       
        }

        public override async Task<byte[]> LoadBytesAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var absPath = Path.Combine(storagePath, relativePath);

            var fileStream = new FileStream(absPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite,
                1024, true);

            var fileBytes = new byte[fileStream.Length];
            
            var byteLoaded = await fileStream.ReadAsync(fileBytes, cancellationToken).ConfigureAwait(false);
            
            if (byteLoaded > fileStream.Length)
            {
                throw new InvalidOperationException(
                    $"Detect fileStream read size ({fileStream.Length} differ from byte load ({byteLoaded}))");
            }
            
            fileStream.Close();

            return fileBytes;       
        }

        public override Stream CreateStream(string relativePath, bool useAsync)
        {
            var absPath = Path.Combine(storagePath, relativePath);
            return new FileStream(absPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite, 1024, useAsync);
        }

        public override void Save(string fileName, byte[] data)
        {
            var filePath = Path.Combine(storagePath, fileName); 
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Log.LogInfo($"Directory created for storage: {directoryPath}");
            }
            File.WriteAllBytes(filePath, data);    
        }

        public override Task SaveAsync(string fileName, byte[] data, CancellationToken token = default)
        {
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
                Log.LogInfo($"Directory created for storage: {storagePath}");
            }
            
            var filePath = Path.Combine(storagePath, fileName);
            return File.WriteAllBytesAsync(filePath, data, token);
        }

        public override bool IsStorageExist()
        {
            return Directory.Exists(storagePath);
        }
    }
}
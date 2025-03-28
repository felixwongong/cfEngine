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
        
        public override string[] GetFiles(string subDirectory, string searchPattern)
        {
            return Directory.GetFiles(Path.Combine(StoragePath, subDirectory), searchPattern);
        }

        public override bool IsFileExist(string relativePath)
        {
            return File.Exists(Path.Combine(StoragePath, relativePath));
        }

        public override void CopyFile(string from, string to, bool overwrite = false)
        {
            File.Copy(Path.Combine(StoragePath, from), Path.Combine(StoragePath, to), overwrite);
        }

        public override void DeleteFile(string relativePath)
        {
            File.Delete(Path.Combine(StoragePath, relativePath));
        }

        public override byte[] LoadBytes(string subDirectory, string fileName)
        {
            var absPath = GetFilePath(subDirectory, fileName);

            using var fileStream = new FileStream(absPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite,
                1024, false);

            var fileBytes = new byte[fileStream.Length];
            var byteLoaded = fileStream.Read(fileBytes, 0, (int)fileStream.Length);
            if (byteLoaded > fileStream.Length)
            {
                throw new InvalidOperationException(
                    $"Detect fileStream read size ({fileStream.Length} differ from byte load ({byteLoaded}))");
            }

            return fileBytes;       
        }

        public override async Task<byte[]> LoadBytesAsync(string subDirectory, string fileName, CancellationToken cancellationToken = default)
        {
            var absPath = GetFilePath(subDirectory, fileName);

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

        public override Stream CreateStream(string subDirectory, string fileName, bool useAsync)
        {
            var absPath = GetFilePath(subDirectory, fileName);

            return new FileStream(absPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite, 1024, useAsync);
        }

        public override void Save(string fileName, byte[] data)
        {
            if (!Directory.Exists(StoragePath))
            {
                Directory.CreateDirectory(StoragePath);
                Log.LogInfo($"Directory created for storage: {StoragePath}");
            }

            var filePath = Path.Combine(StoragePath, fileName); 
            File.WriteAllBytes(filePath, data);    
        }

        public override Task SaveAsync(string fileName, byte[] data, CancellationToken token = default)
        {
            if (!Directory.Exists(StoragePath))
            {
                Directory.CreateDirectory(StoragePath);
                Log.LogInfo($"Directory created for storage: {StoragePath}");
            }
            
            var filePath = Path.Combine(StoragePath, fileName);
            return File.WriteAllBytesAsync(filePath, data, token);
        }

        public override bool IsStorageExist()
        {
            return Directory.Exists(StoragePath);
        }

        private string GetFilePath(string subDirectory, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "filename is empty");
            }

            var absPath = Path.Combine(StoragePath, subDirectory, fileName);

            if (!File.Exists(absPath))
            {
                throw new ArgumentNullException(nameof(fileName), "file does not exist");
            }

            return absPath;
        }
    }
}
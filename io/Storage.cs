using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.IO
{
    public interface IStorage : IDisposable 
    {
        public string[] GetFiles(string subDirectory, string regex);
        public bool IsFileExist(string relativePath);
        public void CopyFile(string from, string to, bool overwrite = false);
        public void DeleteFile(string relativePath);
        public byte[] LoadBytes(string subDirectory, string fileName);
        public Task<byte[]> LoadBytesAsync(string subDirectory, string fileName, CancellationToken token = default);
        public Stream CreateStream(string subDirectory, string fileName, bool useAsync);
        public void Save(string relativePath, byte[] data);
        public Task SaveAsync(string relativePath, byte[] data, CancellationToken token = default);
        public bool IsStorageExist();
    }
    
    public abstract class Storage: IStorage
    {
        public readonly string StoragePath;
        
        protected Storage(string storagePath)
        {
            this.StoragePath = storagePath;
        }

        public void Validate()
        {
            if (!IsStorageExist())
            {
                throw new ArgumentException($"Storage ({StoragePath}) not valid.");
            }
        }

        public abstract string[] GetFiles(string subDirectory, string regex);
        public abstract bool IsFileExist(string relativePath);
        public abstract void CopyFile(string from, string to, bool overwrite = false);
        public abstract void DeleteFile(string relativePath);
        public abstract byte[] LoadBytes(string subDirectory, string fileName);
        public abstract Task<byte[]> LoadBytesAsync(string subDirectory, string fileName, CancellationToken token = default);
        public abstract Stream CreateStream(string subDirectory, string fileName, bool useAsync);
        public abstract void Save(string relativePath, byte[] data);
        public abstract Task SaveAsync(string relativePath, byte[] data, CancellationToken token = default);
        public abstract bool IsStorageExist();

        public virtual void Dispose()
        {
        }
    }
}
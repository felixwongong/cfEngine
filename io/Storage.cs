using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.IO
{
    public interface IStorage : IDisposable 
    {
        public string[] GetFiles(string regex);
        public bool IsFileExist(string relativePath);
        public void CopyFile(string relativeFrom, string relativeTo, bool overwrite = false);
        public void DeleteFile(string relativePath);
        public byte[] LoadBytes(string relativePath);
        public Task<byte[]> LoadBytesAsync(string relativePath, CancellationToken token = default);
        public Stream CreateStream(string relativePath, bool useAsync);
        public void Save(string relativePath, byte[] data);
        public Task SaveAsync(string relativePath, byte[] data, CancellationToken token = default);
        public bool IsStorageExist();
    }
    
    public abstract class Storage: IStorage
    {
        public readonly string storagePath;
        
        protected Storage(string storagePath)
        {
            this.storagePath = storagePath;
        }

        public void Validate()
        {
            if (!IsStorageExist())
            {
                throw new ArgumentException($"Storage ({storagePath}) not valid.");
            }
        }

        public abstract string[] GetFiles(string regex);
        public abstract bool IsFileExist(string relativePath);
        public abstract void CopyFile(string relativeFrom, string relativeTo, bool overwrite = false);
        public abstract void DeleteFile(string relativePath);
        public abstract byte[] LoadBytes(string relativePath);
        public abstract Task<byte[]> LoadBytesAsync(string relativePath, CancellationToken token = default);
        public abstract Stream CreateStream(string relativePath, bool useAsync);
        public abstract void Save(string relativePath, byte[] data);
        public abstract Task SaveAsync(string relativePath, byte[] data, CancellationToken token = default);
        public abstract bool IsStorageExist();

        public virtual void Dispose()
        {
        }
    }
}
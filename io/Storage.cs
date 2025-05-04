using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.IO
{
    public interface IStorage : IDisposable 
    {
        public string[] GetFiles(string searchPattern);
        public string[] GetFiles(string directory, string searchPattern);
        public bool IsFileExist(string relativePath);
        public void CopyFile(string relativeFrom, string relativeTo, bool overwrite = false);
        public void DeleteFile(string relativePath);
        public byte[] LoadBytes(string relativePath);
        public Task<byte[]> LoadBytesAsync(string relativePath, CancellationToken token = default);
        public Stream CreateStream(string relativePath, bool useAsync);
        public void Save(string relativeFilePath, byte[] data);
        public Task SaveAsync(string relativeFilePath, byte[] data, CancellationToken token = default);
        public bool IsStorageExist();
    }
}
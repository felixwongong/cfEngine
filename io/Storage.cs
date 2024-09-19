using System;
using System.IO;
using UnityEngine;

namespace cfEngine.IO
{
    public abstract class Storage
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

        public abstract string[] GetFiles(string regex);
        public abstract byte[] LoadBytes(string fileName);
        public abstract void Save(string fileName, Stream streamIn);
        public abstract bool IsStorageExist();
    }

    public class FileStorage: Storage
    {
        public FileStorage(string storagePath): base(storagePath)
        {
        }
        
        public override string[] GetFiles(string searchPattern)
        {
            return Directory.GetFiles(StoragePath, searchPattern);
        }

        public override byte[] LoadBytes(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "filename is empty");
            }

            var absPath = Path.Combine(StoragePath, fileName);

            if (!File.Exists(absPath))
            {
                throw new ArgumentNullException(nameof(fileName), "file does not exist");
            }

            byte[] fileBytes;
            using var filestream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileBytes = new byte[filestream.Length];
            var byteLoaded = filestream.Read(fileBytes, 0, (int)filestream.Length);
            if (byteLoaded > filestream.Length)
            {
                throw new InvalidOperationException(
                    $"Detect filestream read size ({filestream.Length} differ from byte load ({byteLoaded}))");
            }

            return fileBytes;       
        }

        public override void Save(string fileName, Stream streamIn)
        {
            if (!Directory.Exists(StoragePath))
            {
                Directory.CreateDirectory(StoragePath);
                Debug.Log($"Directory created for storage: {StoragePath}");
            }

            var filePath = Path.Combine(StoragePath, fileName); 
            
            using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            streamIn.CopyTo(fileStream);
        }

        public override bool IsStorageExist()
        {
            return Directory.Exists(StoragePath);
        }
    }
}
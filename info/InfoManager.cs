using System;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.IO;
using cfEngine.Serialize;
using CofyDev.Xml.Doc;

namespace cfEngine.Info
{
    public interface IInfoManager: IDisposable
    {
        public string infoKey {get;}
        string infoDirectory { get; }
        public ISerializer Serializer { set; }
        public DataObjectEncoder Encoder { set; }
        public IStorage Storage { set; }

        void DirectlyLoadFromExcel();
        void LoadSerialized();
        Task LoadSerializedAsync(CancellationToken cancellationToken);
        void SerializeIntoStorage();
    }
    
    public abstract class InfoManager: IInfoManager
    {
        public abstract string infoKey { get; }
        
        private ISerializer _serializer;

        public ISerializer Serializer
        {
            protected get => _serializer;
            set => _serializer = value;
        }

        private DataObjectEncoder _encoder;

        public DataObjectEncoder Encoder
        {
            protected get => _encoder;
            set => _encoder = value;
        }

        private IStorage _storage;

        public IStorage Storage
        {
            protected get => _storage;
            set => _storage = value;
        }

        public abstract string infoDirectory { get; }
        public abstract void DirectlyLoadFromExcel();
        public abstract void LoadSerialized();
        public abstract Task LoadSerializedAsync(CancellationToken cancellationToken);
        public abstract void SerializeIntoStorage();
        protected virtual void OnLoadCompleted() {}

        public virtual void Dispose()
        {
            _serializer = null;
            _encoder?.Dispose();
            _encoder = null;
            _storage?.Dispose();
            _storage = null;
        }
    }
}
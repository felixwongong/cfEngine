using System;
using System.Collections.Generic;
using cfEngine.IO;
using cfEngine.Serialize;
using CofyDev.Xml.Doc;
using Unity.VisualScripting;

namespace cfEngine.Info
{
    public abstract class InfoManager: IDisposable
    {
        private StreamSerializer _serializer;

        public StreamSerializer Serializer
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

        private Storage _storage;

        public Storage Storage
        {
            protected get => _storage;
            set => _storage = value;
        }

        public abstract string InfoDirectory { get; }

        public abstract void DirectlyLoadFromExcel();
        public abstract void LoadSerialized();
        public abstract void SerializeIntoStorage();

        public virtual void Dispose()
        {
            _serializer = null;
            _encoder?.Dispose();
            _encoder = null;
            _storage?.Dispose();
            _storage = null;
        }
    }

    public abstract class ExcelInfoManager<TKey, TInfo> : InfoManager where TKey : notnull
    {
        protected readonly Dictionary<TKey, TInfo> _valueMap = new();
        public IReadOnlyDictionary<TKey, TInfo> ValueMap => _valueMap;

        protected abstract Func<TInfo, TKey> KeyFn { get; }

        protected ExcelInfoManager() : base()
        {
        }

        public override void DirectlyLoadFromExcel()
        {
            if (string.IsNullOrEmpty(InfoDirectory))
            {
                throw new ArgumentNullException(nameof(InfoDirectory), "info key is unset");
            }

            var files = Storage.GetFiles(InfoDirectory, "*.xlsx");

            var excelData = new CofyXmlDocParser.DataContainer();
            foreach (var file in files)
            {
                var fileExcelData = CofyXmlDocParser.ParseExcel(Storage.LoadBytes(InfoDirectory, file));
                excelData.AddRange(fileExcelData);
            }

            if (Encoder == null)
            {
                throw new ArgumentNullException(nameof(Encoder), "encoder unset");
            }
            
            _valueMap.EnsureCapacity(excelData.Count);

            foreach (var dataObject in excelData)
            {
                var decoded = Encoder.DecodeAs<TInfo>(dataObject, DataObjectExtension.SetDecodePropertyValue);
                _valueMap.Add(KeyFn(decoded), decoded);
            }
        }

        public override void LoadSerialized()
        {
            if (string.IsNullOrEmpty(InfoDirectory))
            {
                throw new ArgumentNullException(nameof(InfoDirectory), "info key is unset");
            }

            var files = Storage.GetFiles(string.Empty, InfoDirectory);
            if (files.Length <= 0)
            {
                throw new ArgumentException($"serialized file ({InfoDirectory}) not found in Info Directory",
                    nameof(InfoDirectory));
            }

            using var fileStream = Storage.StreamLoad(string.Empty, InfoDirectory);
            var deserialized = Serializer.DeserializeAs<Dictionary<TKey, TInfo>>(fileStream);
            _valueMap.AddRange(deserialized);
        }

        public override void SerializeIntoStorage()
        {
            using var memoryStream = Serializer.Serialize(ValueMap);
            Storage.Save(InfoDirectory, memoryStream);
        }

        public override void Dispose()
        {
            base.Dispose();
            _valueMap.Clear();
        }
    }
}
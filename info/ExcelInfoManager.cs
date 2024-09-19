using System;
using System.Collections.Generic;
using cfEngine.IO;
using cfEngine.Serialize;
using CofyDev.Xml.Doc;

namespace cfEngine.Info
{
    public abstract class InfoManager
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
    }

    public abstract class ExcelInfoManager<TKey, TInfo> : InfoManager where TKey : notnull
    {
        private readonly Dictionary<TKey, TInfo> _infoDict = new();
        public IReadOnlyDictionary<TKey, TInfo> infoDict => _infoDict;

        protected abstract Func<TInfo, TKey> keyFn { get; }

        protected ExcelInfoManager() : base()
        {
        }

        public void LoadFromExcel()
        {
            if (string.IsNullOrEmpty(InfoDirectory))
            {
                throw new ArgumentNullException(nameof(InfoDirectory), "info key is unset");
            }

            var files = Storage.GetFiles("*.xlsx");

            var excelData = new CofyXmlDocParser.DataContainer();
            foreach (var file in files)
            {
                var fileExcelData = CofyXmlDocParser.ParseExcel(Storage.LoadBytes(file));
                excelData.AddRange(fileExcelData);
            }

            _infoDict.EnsureCapacity(excelData.Count);

            if (Encoder == null)
            {
                throw new ArgumentNullException(nameof(Encoder), "encoder unset");
            }

            foreach (var dataObject in excelData)
            {
                var decoded = Encoder.DecodeAs<TInfo>(dataObject, DataObjectExtension.SetDecodePropertyValue);
                _infoDict.Add(keyFn(decoded), decoded);
            }
        }
    }
}
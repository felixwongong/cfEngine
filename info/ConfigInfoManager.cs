using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;
using CofyDev.Xml.Doc;

namespace cfEngine.Info
{
    public abstract class ConfigInfoManager<TKey, TInfo> : InfoManager where TKey : notnull
    {
        protected readonly Dictionary<TKey, TInfo> _valueMap = new();
        public IReadOnlyDictionary<TKey, TInfo> ValueMap => _valueMap;

        private List<TInfo> _allValues;
        public IReadOnlyList<TInfo> allValues => _allValues ??= _valueMap.Values.ToList();

        protected abstract Func<TInfo, TKey> keyFn { get; }

        protected ConfigInfoManager() : base()
        {
        }

        public override void DirectlyLoadFromExcel()
        {
            if (string.IsNullOrEmpty(infoDirectory))
            {
                throw new ArgumentNullException(nameof(infoDirectory), "info key is unset");
            }

            var files = Storage.GetFiles(infoDirectory, "*.xlsx");

            var excelData = new CofyXmlDocParser.DataContainer();
            foreach (var file in files)
            {
                var fileExcelData = CofyXmlDocParser.ParseExcel(Storage.LoadBytes(infoDirectory, file));
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
                _valueMap.Add(keyFn(decoded), decoded);
            }
            
            OnLoadCompleted();
        }

        public override void LoadSerialized()
        {
            if (string.IsNullOrEmpty(infoDirectory))
            {
                throw new ArgumentNullException(nameof(infoDirectory), "info key is unset");
            }

            var files = Storage.GetFiles(string.Empty, infoDirectory);
            if (files.Length <= 0)
            {
                throw new ArgumentException($"serialized file ({infoDirectory}) not found in Info Directory",
                    nameof(infoDirectory));
            }

            var byteLoaded = Storage.LoadBytes(string.Empty, infoDirectory);
            var deserialized = Serializer.DeserializeAs<Dictionary<TKey, TInfo>>(byteLoaded);
            _valueMap.EnsureCapacity(deserialized.Count);
            foreach (var kvp in deserialized)
            {
                _valueMap.Add(kvp.Key, kvp.Value);
            }
            
            OnLoadCompleted();
            
            Log.LogDebug($"{infoDirectory} loaded from serialized, value count: {_valueMap.Count}");
        }

        public override async Task LoadSerializedAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(infoDirectory))
            {
                throw new ArgumentNullException(nameof(infoDirectory), "info key is unset");
            }

            var files = Storage.GetFiles(string.Empty, infoDirectory);
            if (files.Length <= 0)
            {
                throw new ArgumentException($"serialized file ({infoDirectory}) not found in Info Directory",
                    nameof(infoDirectory));
            }

            var byteLoaded = await Storage.LoadBytesAsync(string.Empty, infoDirectory, cancellationToken).ConfigureAwait(false);
            var deserialized = await Serializer.DeserializeAsAsync<Dictionary<TKey, TInfo>>(byteLoaded, token:cancellationToken)
                .ConfigureAwait(false);
            
            _valueMap.EnsureCapacity(deserialized.Count);
            foreach (var kvp in deserialized)
            {
                _valueMap.Add(kvp.Key, kvp.Value);
            }
            
            OnLoadCompleted();
        }

        public override void SerializeIntoStorage()
        {
            var serialized = Serializer.Serialize(ValueMap);
            Storage.Save(infoDirectory, serialized);
        }

        public override void Dispose()
        {
            base.Dispose();
            _valueMap.Clear();
        }
    }
}
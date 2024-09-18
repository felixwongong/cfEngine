using cfEngine.Serialize;
using CofyDev.Xml.Doc;

namespace cfEngine.Info;

public abstract class InfoManager
{
    private StreamSerializer? _serializer;

    public StreamSerializer? Serializer
    {
         protected get => _serializer;
         set => _serializer = value;
    }
    
    private DataObjectEncoder? _encoder;

    public DataObjectEncoder? Encoder
    {
        protected get => _encoder;
        set => _encoder = value;
    }

    private string _infoRoot = string.Empty;
    public string InfoRoot
    {
        protected get => _infoRoot;
        set => _infoRoot = value;
    }
    
    public abstract string InfoDirectory { get; }
}

public abstract class ExcelInfoManager<TKey, TInfo>: InfoManager where TKey : notnull
{
    private readonly Dictionary<TKey, TInfo> _infoDict = new();
    public IReadOnlyDictionary<TKey, TInfo> infoDict => _infoDict;
    
    protected abstract Func<TInfo, TKey> keyFn { get; }

    protected ExcelInfoManager(): base() { }

    public void LoadFromExcel()
    {
        if (string.IsNullOrEmpty(InfoRoot))
        {
            throw new ArgumentNullException(nameof(InfoRoot), "info root path is unset");
        }

        if (string.IsNullOrEmpty(InfoDirectory))
        {
            throw new ArgumentNullException(nameof(InfoDirectory), "info key is unset");
        }

        var infoDirectoryPath = Path.Combine(InfoRoot, InfoDirectory);
        var files = Directory.GetFiles(infoDirectoryPath, "*.xlsx");

        var excelData = new CofyXmlDocParser.DataContainer();
        foreach (var file in files)
        {
            var fileExcelData = CofyXmlDocParser.ParseExcel(file);
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
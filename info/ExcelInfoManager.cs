using cfEngine.Serialize;
using CofyDev.Xml.Doc;

namespace cfEngine.Info;

public abstract class InfoManager
{
    private StreamSerializer? _serializer;
    protected StreamSerializer? serializer => _serializer;
    
    private string _infoRoot = string.Empty;
    protected string infoRoot => _infoRoot;
    
    public abstract string infoDirectory { get; }
    
    public void RegisterSerializer(StreamSerializer? s)
    {
        _serializer = s;
    }

    public void setInfoRoot(string infoRootPath)
    {
        _infoRoot = infoRootPath;
    }
}

public abstract class ExcelInfoManager<TKey, TInfo>: InfoManager where TKey : notnull
{
    private readonly Dictionary<TKey, TInfo> _infoDict = new();
    public IReadOnlyDictionary<TKey, TInfo> infoDict => _infoDict;
    
    protected abstract Func<TInfo, TKey> keyFn { get; }

    protected ExcelInfoManager(): base() { }

    public void LoadFromExcel()
    {
        if (string.IsNullOrEmpty(infoRoot))
        {
            throw new ArgumentNullException(nameof(infoRoot), "info root path is unset");
        }

        if (string.IsNullOrEmpty(infoDirectory))
        {
            throw new ArgumentNullException(nameof(infoDirectory), "info key is unset");
        }

        var infoDirectoryPath = Path.Combine(infoRoot, infoDirectory);
        var files = Directory.GetFiles(infoDirectoryPath, "*.xlsx");

        var excelData = new CofyXmlDocParser.DataContainer();
        foreach (var file in files)
        {
            var fileExcelData = CofyXmlDocParser.ParseExcel(file);
           excelData.AddRange(fileExcelData); 
        }

        _infoDict.EnsureCapacity(excelData.Count);
        foreach (var dataObject in excelData)
        {
            var decoded = dataObject.DecodeAs<TInfo>(DataObjectExtension.SetDecodePropertyValue);
            _infoDict.Add(keyFn(decoded), decoded);
        }
    }
}
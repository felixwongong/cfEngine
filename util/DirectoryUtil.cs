using System.IO;

namespace cfEngine.Util
{
    public static class DirectoryUtil
    {
        public static DirectoryInfo CreateDirectoryIfNotExists(string directoryPath, string assetFolderPath)
        {
            var absolutePath = Path.Combine(directoryPath, assetFolderPath);
            var directoryInfo = new DirectoryInfo(absolutePath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return directoryInfo;
        }
    }
}
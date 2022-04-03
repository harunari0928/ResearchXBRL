using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ResearchXBRL.Infrastructure.Services;

public interface IFileStorage
{
    public Stream Get(string filePath);
    public void Set(in Stream inputStream, string filePath);
    IReadOnlyList<string> GetFiles(string directoryPath, string searchPattern = "*");
    IReadOnlyList<string> GetDirectoryNames(string directoryPath, string searchPattern = "*");
    public void Unzip(string zipFilePath, string unzippedDirectoryPath);
    public void Delete(string path);

    protected static bool IsDirectory(string path)
    {
        path = path?.Trim() ?? throw new ArgumentNullException(nameof(path));

        if (Directory.Exists(path))
        {
            return true;
        }

        if (File.Exists(path))
        {
            return false;
        }

        if (new[] { "\\", "/" }.Any(path.EndsWith))
        {
            return true;
        }

        return string.IsNullOrWhiteSpace(Path.GetExtension(path));
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ResearchXBRL.Infrastructure.Services;

public interface IFileStorage
{
    public Stream? Get(in string filePath);
    public void Set(in Stream inputStream, in string filePath);
    IReadOnlyList<string> GetFiles(string directoryPath, string searchPattern = "*");
    IReadOnlyList<string> GetDirectoryNames(string directoryPath, string searchPattern = "*");
    public void Unzip(string zipFilePath, string unzippedDirectoryPath);
    public void Delete(string path);
    protected static bool IsDirectory(in string path)
    {
        var trimedPath = path.Trim();

        if (new[] { "\\", "/" }.Any(trimedPath.EndsWith))
        {
            return true;
        }

        return string.IsNullOrWhiteSpace(Path.GetExtension(trimedPath));
    }
}

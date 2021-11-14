﻿using System.Collections.Generic;
using System.IO;

namespace ResearchXBRL.Application.Services
{
    public interface IFileStorage
    {
        public Stream Get(string filePath);
        public void Set(Stream inputStream, string filePath);
        IReadOnlyList<string> GetFiles(string directoryPath, string searchPattern = "*");
        public void Unzip(string zipFilePath, string unzippedDirectoryPath, bool isDeleteOriginalZipFile);
    }
}
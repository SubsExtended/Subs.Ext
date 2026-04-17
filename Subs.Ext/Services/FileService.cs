using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Subs.Ext.Models;

namespace Subs.Ext.Services
{
    internal class FileService : IFileService
    {
        public FileService() { }

        public FileService(string path) { }

        public Task<ObservableCollection<FileModel>> ReadFileAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task WriteFileAsync(FileModel fileModel, string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
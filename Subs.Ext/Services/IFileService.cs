using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Subs.Ext.Models;

namespace Subs.Ext.Services
{
    internal interface IFileService
    {
        internal Task<ObservableCollection<FileModel>> ReadFileAsync(string directoryPath);
        internal Task WriteFileAsync(FileModel fileModel, string directoryPath);
    }
}

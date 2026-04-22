using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rating.WPF.Models;

namespace Rating.WPF.Services
{
    public interface IFileService
    {
        public Task<ObservableCollection<FileModel>> ReadFileAsync(string directoryPath);
        public Task WriteFileAsync(FileModel fileModel, string directoryPath);
    }
}

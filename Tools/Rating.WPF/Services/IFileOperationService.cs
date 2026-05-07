// Subs.Ext\Tools\Rating.WPF\Services\IFileOperationService.cs

using Prism.Services.Dialogs;
using Rating.WPF.Enums;
using Rating.WPF.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Rating.WPF.Services
{
    public interface IFileOperationService
    {
        Task ExecuteAsync(
            FileOperationEnum operation,
            ObservableCollection<FileModel> files,
            FileModel primaryFile,
            FileModel secondarySelected,
            IFileService fileService,
            IDialogService dialogService);
    }
}
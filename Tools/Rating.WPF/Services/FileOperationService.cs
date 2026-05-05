using Microsoft.Win32;
using Prism.Services.Dialogs;
using Rating.WPF.Dialogs;
using Rating.WPF.Enums;
using Rating.WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rating.WPF.Services
{
    public class FileOperationService : IFileOperationService
    {
        public async Task ExecuteAsync(
            FileOperationEnum operation,
            ObservableCollection<FileModel> files,
            FileModel primaryFile,
            FileModel secondarySelected,
            IFileService fileService,
            IDialogService dialogService)
        {
            switch (operation)
            {
                case FileOperationEnum.PrimarySave:
                    await SavePrimary(primaryFile, fileService);
                    break;

                case FileOperationEnum.PrimarySaveAs:
                    await SaveAs(primaryFile, fileService);
                    break;

                case FileOperationEnum.PrimaryClose:
                    await ClosePrimary(primaryFile, files, fileService, dialogService);
                    break;

                case FileOperationEnum.SecondaryAllSave:
                    await SaveAllSecondaries(files, fileService);
                    break;

                case FileOperationEnum.SecondaryAllClose:
                    await CloseAllSecondaries(files, fileService, dialogService);
                    break;

                case FileOperationEnum.SecondarySingleSave:
                    await SaveSecondary(secondarySelected, fileService);
                    break;

                case FileOperationEnum.SecondarySingleSaveAs:
                    await SaveAs(secondarySelected, fileService);
                    break;

                case FileOperationEnum.SecondarySingleClose:
                    await CloseSecondary(secondarySelected, files, fileService, dialogService);
                    break;
            }
        }

        private async Task SavePrimary(FileModel primary, IFileService fileService)
        {
            if (primary == null || !primary.IsDirty)
                return;

            await fileService.WriteFileAsync(primary, primary.FilePath, CancellationToken.None);
            FinalizeSave(primary);
        }

        private async Task SaveSecondary(FileModel file, IFileService fileService)
        {
            if (file == null || !file.IsDirty)
                return;

            await fileService.WriteFileAsync(file, file.FilePath, CancellationToken.None);
            FinalizeSave(file);
        }

        private async Task SaveAllSecondaries(ObservableCollection<FileModel> files, IFileService fileService)
        {
            foreach (var file in files.Where(f => f.FileRank == FileRankEnum.Secondary))
            {
                if (!file.IsDirty)
                    continue;

                await fileService.WriteFileAsync(file, file.FilePath, CancellationToken.None);
                FinalizeSave(file);
            }
        }

        private async Task ClosePrimary(
            FileModel primary,
            ObservableCollection<FileModel> files,
            IFileService fileService,
            IDialogService dialogService)
        {
            if (primary == null)
                return;

            if (primary.IsDirty)
            {
                if (Ask(dialogService, primary.FilePath))
                {
                    await fileService.WriteFileAsync(primary, primary.FilePath, CancellationToken.None);
                }
            }

            files.Remove(primary);
        }

        private async Task CloseSecondary(
            FileModel file,
            ObservableCollection<FileModel> files,
            IFileService fileService,
            IDialogService dialogService)
        {
            if (file == null)
                return;

            if (file.IsDirty)
            {
                if (Ask(dialogService, file.FilePath))
                {
                    await fileService.WriteFileAsync(file, file.FilePath, CancellationToken.None);
                }
            }

            files.Remove(file);
        }

        private async Task CloseAllSecondaries(
            ObservableCollection<FileModel> files,
            IFileService fileService,
            IDialogService dialogService)
        {
            var toRemove = new List<FileModel>();

            foreach (var file in files.Where(f => f.FileRank == FileRankEnum.Secondary))
            {
                if (file.IsDirty)
                {
                    if (Ask(dialogService, file.FilePath))
                    {
                        await fileService.WriteFileAsync(file, file.FilePath, CancellationToken.None);
                        toRemove.Add(file);
                    }
                }
                else
                {
                    toRemove.Add(file);
                }
            }

            foreach (var f in toRemove)
                files.Remove(f);
        }

        private async Task SaveAs(FileModel file, IFileService fileService)
        {
            if (file == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "SRT files (*.srt)|*.srt",
                FileName = Path.GetFileName(file.FilePath)
            };

            if (dialog.ShowDialog() == true)
            {
                await fileService.WriteFileAsync(file, dialog.FileName, CancellationToken.None);

                file.FilePath = dialog.FileName;
                file.FileName = Path.GetFileName(dialog.FileName);

                FinalizeSave(file);
            }
        }

        private void FinalizeSave(FileModel file)
        {
            foreach (var sub in file.SubtitleCollection)
                sub.RatingOriginal = sub.RatingCurrent;

            file.SetIsDirty();
        }

        private bool Ask(IDialogService dialogService, string filePath)
        {
            bool result = false;

            dialogService.ShowDialog(
                nameof(YesNoDialog),
                new DialogParameters($"message=You have unsaved changes in\r\n{filePath}. Save before closing?"),
                r => result = r.Result == ButtonResult.Yes);

            return result;
        }
    }
}
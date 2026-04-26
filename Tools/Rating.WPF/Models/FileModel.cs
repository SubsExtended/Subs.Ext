// Subs.Ext\Tools\Rating.WPF\Models\FileModel.cs

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;

using Rating.WPF.Enums;

namespace Rating.WPF.Models
{
    public class FileModel : BindableBase
    {
        public Guid PK { get; set; } = Guid.NewGuid();

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { SetProperty(ref fileName, value); }
        }

        /// <summary>
        /// This property is not stored/read in/from file, but used for display purposes only.
        /// </summary>
        private LanguageEnum? language;
        public  LanguageEnum? Language
        {
            get { return language; }
            set { SetProperty(ref language, value); }
        }

        private FileTypeEnum fileType;
        public FileTypeEnum FileType
        {
            get { return fileType; }
            set { SetProperty(ref fileType, value); }
        }

        /// <summary>
        /// Primary or Secondary
        /// </summary>
        private FileRankEnum fileRank;
        public FileRankEnum FileRank
        {
            get { return fileRank; }
            set { SetProperty(ref fileRank, value); }
        }

        private SubtitleModel subtitleSelectedItem;
        public SubtitleModel SubtitleSelectedItem
        {
            get { return subtitleSelectedItem; }
            set { SetProperty(ref subtitleSelectedItem, value); }
        }

        private ObservableCollection<SubtitleModel> subtitleCollection;
        public ObservableCollection<SubtitleModel> SubtitleCollection
        {
            get { return subtitleCollection; }
            set { SetProperty(ref subtitleCollection, value); }
        }
    }
}
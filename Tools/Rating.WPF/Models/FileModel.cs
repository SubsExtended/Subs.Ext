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
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { SetProperty(ref fileName, value); }
        }

        /// <summary>
        /// This property is not stored/read in/from file, but used for display purposes only.
        /// </summary>
        private Language language;
        public  Language Language
        {
            get { return language; }
            set { SetProperty(ref language, value); }
        }

        private FileType fileType;
        public FileType FileType
        {
            get { return fileType; }
            set { SetProperty(ref fileType, value); }
        }

        private FileRank fileRank;
        public FileRank FileRank
        {
            get { return fileRank; }
            set { SetProperty(ref fileRank, value); }
        }

        private ObservableCollection<SubtitleModel> subs;
        public ObservableCollection<SubtitleModel> Subs
        {
            get { return subs; }
            set { SetProperty(ref subs, value); }
        }
    }
}
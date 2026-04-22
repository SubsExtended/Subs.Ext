using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;

using Rating.WPF.Enums;

namespace Rating.WPF.Models
{
    public class SubtitleModel : BindableBase
    {
        private string originalBlock;
        public string OriginalBlock
        {
            get { return originalBlock; }
            set { SetProperty(ref originalBlock, value); }
        }

        private int position;
        public int Position
        {
            get { return position; }
            set { SetProperty(ref position, value); }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        private TimeSpan timeFrom;
        public TimeSpan TimeFrom
        {
            get { return timeFrom; }
            set { SetProperty(ref timeFrom, value); }
        }

        private TimeSpan timeTo;
        public TimeSpan TimeTo
        {
            get { return timeTo; }
            set { SetProperty(ref timeTo, value); }
        }

        private SubRating subtitleRating;
        public SubRating SubtitleRating
        {
            get { return subtitleRating; }
            set { SetProperty(ref subtitleRating, value); }
        }
    }
}
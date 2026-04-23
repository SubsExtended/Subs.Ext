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
        /// <summary>
        /// Original block of text from the subtitle file, including index, timecode, and text.
        /// If the subtitle file is modified and saved, this original block will be used to reconstruct the file with updated ratings
        /// </summary>
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

        /// <summary>
        /// Difference between Original and Current Ratings will be considered as "file dirty"
        /// and will be used for visual cues in the UI (e.g. red highlight)
        /// and for saving logic (e.g. prompt to save changes).
        /// If null, it means no rating was originally assigned
        /// and any current rating will be treated as a new assignment rather than a change.
        /// This allows us to distinguish between subs that were never rated
        /// and those that have been modified from an original rating.
        /// </summary>
        private SubRating? ratingOriginal;
        public SubRating? RatingOriginal
        {
            get { return ratingOriginal; }
            set { SetProperty(ref ratingOriginal, value); }
        }

        /// <summary>
        /// Difference between Original and Current Ratings will be considered as "file dirty"
        /// and will be used for visual cues in the UI (e.g. red highlight)
        /// and for saving logic (e.g. prompt to save changes).
        /// If null, it means no rating was originally assigned
        /// and any current rating will be treated as a new assignment rather than a change.
        /// This allows us to distinguish between subs that were never rated
        /// and those that have been modified from an original rating.
        /// </summary>
        private SubRating? ratingCurrent;
        public SubRating? RatingCurrent
        {
            get { return ratingCurrent; }
            set { SetProperty(ref ratingCurrent, value); }
        }
    }
}
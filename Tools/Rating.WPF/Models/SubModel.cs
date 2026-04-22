using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;

using Rating.WPF.Enums;

namespace Rating.WPF.Models
{
    internal class SubModel : BindableBase
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

        private SubRating subRating;
        public SubRating SubRating
        {
            get { return subRating; }
            set { SetProperty(ref subRating, value); }
        }
    }
}
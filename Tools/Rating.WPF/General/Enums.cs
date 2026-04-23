using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rating.WPF.Enums
{
    public enum FileRank
    {
        None,
        Primary,
        Secondary,
    }

    public enum FileType
    {
        None,
        Srt,
    }

    public enum SubRating
    {
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
    }

    public enum Language
    {
        English,
        Spanish,
        French,
        Russian,
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rating.WPF.Enums
{
    public enum FileRankEnum
    {
        Primary,
        Secondary,
    }

    public enum FileTypeEnum
    {
        None,
        Srt,
    }

    public enum SubtitleRatingEnum
    {
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
    }

    public enum LanguageEnum
    {
        English,
        Spanish,
        French,
        Russian,
    }
}
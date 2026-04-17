using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subs.Ext.Enums
{
    internal enum FileRank
    {
        None,
        Primary,
        Secondary,
    }

    internal enum FileType
    {
        None,
        Srt,
    }

    internal enum SubRating
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
    }

    internal enum Language
    {
        English,
        Spanish,
        French,
        Russian,
    }
}
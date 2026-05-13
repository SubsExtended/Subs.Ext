// Subs.Ext\Tools\Rating.WPF\General\Enums.cs

namespace Rating.WPF.Enums
{
    public enum FileRankEnum
    {
        Primary,
        Secondary,
        Media,
    }

    public enum FileTypeEnum
    {
        None,
        Srt,
    }

    // The rating is based on how hard is a phrase for understanding, with A being the hardest and E being the easiest.
    public enum SubtitleRatingEnum
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
    }

    public enum LanguageEnum
    {
        English,
        Russian,
        Spanish,
        French,
        Italian,
        German,
        Chinese,
        Japanese,
    }

    public enum MyLanguageLevelEnum
    {
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
    }

    public enum FileOperationEnum
    {
        PrimarySave,
        PrimarySaveAs,
        PrimaryClose,
        SecondarySingleSave,
        SecondarySingleSaveAs,
        SecondarySingleClose,
        SecondaryAllSave,
        SecondaryAllClose,
    }
}
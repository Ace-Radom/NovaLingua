namespace NovaLingua.Lib;

public enum LetterPlacementRule
{
    Unknown = 0,
    NoRule,
    InitialOnly,
    FinalOnly,
    InitialFinalOnly,
    NotInitial,
    NotFinal,
    NotInitialFinal
}

public enum LetterType
{
    Unknown = 0,
    Vowel,
    Consonant
}

public enum LocalLanguage
{
    Unknown = 0,
    English,
    ChineseSimplified,
    ChineseTraditional,
    German,
    Japanese,
    Spanish,
    French,
    Others = 999
}

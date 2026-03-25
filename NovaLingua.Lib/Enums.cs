namespace NovaLingua.Lib;

public enum LetterPlacementRule
{
    NoRule = 0,
    InitialOnly,
    FinalOnly,
    InitialFinalOnly,
    NotInitial,
    NotFinal,
    NotInitialFinal,
    Unknown = 999
}

public enum LetterType
{
    Vowel = 0,
    Consonant,
    Unknown = 999
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

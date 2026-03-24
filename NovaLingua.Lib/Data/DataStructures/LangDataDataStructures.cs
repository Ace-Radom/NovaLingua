using System.Collections.Generic;

namespace NovaLingua.Lib.Data.DataStructures;

internal class MetaData : IDataStructure<MetaData>
{
    public required int Version { get; set; }
    public required string LangName { get; set; }
    public string LangVersion { get; set; } = "";
    public required int DefinitionLang { get; set; }
    // local lang, which all conlang words should be translated into
    public string LangNameTranslation { get; set; } = "";
    public string LangDescription { get; set; } = "";
    public required long CreateTimeTs { get; set; }
    public required long LastModifyTimeTs { get; set; }

    public required bool AutoGenerationUseVariants { get; set; }
    public required bool ForceLetterUnique { get; set; }
    public required bool ForceLetterVariantGlobalUnique { get; set; }
    // one variant of a letter cannot be same as another letter (or its upper & variants)
    public required bool ForceWordUnique { get; set; }
    public required bool ForceWordInflectionGlobalUnique { get; set; }
    // one inflection of a word cannot be same as another word (or its inflections)
    public required bool ForceWordDefinitionUnique { get; set; }
    public required bool WordCaseInsensitive { get; set; }


    public bool IsEmpty => (Version < 0);
    public static MetaData Empty => new()
    {
        Version = -1,
        LangName = "",
        DefinitionLang = -1,
        CreateTimeTs = -1,
        LastModifyTimeTs = -1,
        AutoGenerationUseVariants = false,
        ForceLetterUnique = false,
        ForceLetterVariantGlobalUnique = false,
        ForceWordUnique = false,
        ForceWordInflectionGlobalUnique = false,
        ForceWordDefinitionUnique = false,
        WordCaseInsensitive = false
    };
    public static string Type => nameof(MetaData);
}

internal class AlphabetData : IDataStructure<AlphabetData>
{
    public required int Version { get; set; }
    public required List<LetterData> Vowels { get; set; }
    public required List<LetterData> Consonants { get; set; }

    public bool IsEmpty => (Version < 0);
    public static AlphabetData Empty => new()
    {
        Version = -1,
        Vowels = [],
        Consonants = []
    };
    public static string Type => nameof(AlphabetData);
}

internal class LetterData
{
    public required string Id { get; set; }
    public required string Letter { get; set; }
    public required string LetterUppercase { get; set; }
    public required int AlphabeticOrder { get; set; }
    public string Comment { get; set; } = "";
    public required List<LetterVariantData> Variants { get; set; }
    // variants are special forms of a letter, they share max count & have the same placement rule
    public required int MaxCountInWord { get; set; }
    public required int PlacementRule { get; set; }
    public required bool AllowInAutoGeneration { get; set; }
    public required int AutoGenerationRate { get; set; }
    public required long AddTimeTs { get; set; }
}

internal class LetterVariantData
{
    public required string Id { get; set; }
    // just a normal uuid here
    // when referencing variants in words, use `root-id`-`variant-id`
    public required string VariantLetter { get; set; }
    public required string VariantLetterUppercase { get; set; }
    public string Comment { get; set; } = "";
    public required long AddTimeTs { get; set; }
}

internal class WordListData : IDataStructure<WordListData>
{
    public required int Version { get; set; }
    public required List<WordData> Words { get; set; }

    public bool IsEmpty => (Version < 0);
    public static WordListData Empty => new()
    {
        Version = -1,
        Words = [],
    };
    public static string Type => nameof(WordListData);
}

internal class WordData
{
    public required string Id { get; set; }
    public required List<WordLetterData> Letters { get; set; }
    public required int Type { get; set; }
    public required List<string> Definitions { get; set; }
    // there can be multiple defs
    public string Comment { get; set; } = "";
    public required List<WordInflectionData> Inflections { get; set; }
    public required long AddTimeTs { get; set; }
}

internal class WordInflectionData
{
    public required string Id { get; set; }
    public required List<WordLetterData> Letters { get; set; }
    public required int Type { get; set; }
    // normally it should be same as the root word
    public required List<string> Definitions { get; set; }
    public string Comment { get; set; } = "";
    public required long AddTimeTs { get; set; }
}

internal class WordLetterData
{
    public required string LetterId { get; set; }
    public required bool UseUpper { get; set; }
}

internal class TodoListData : IDataStructure<TodoListData>
{
    public required int Version { get; set; }
    public required List<TodoLabelData> Labels { get; set; }
    public required List<TodoData> Todos { get; set; }

    public bool IsEmpty => (Version < 0);
    public static TodoListData Empty => new()
    {
        Version = -1,
        Labels = [],
        Todos = []
    };
    public static string Type => nameof(TodoListData);
}

internal class TodoLabelData
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required long AddTimeTs { get; set; }
}

internal class TodoData
{
    public required string Msg { get; set; }
    public string LabelId { get; set; } = "";
    public int Color { get; set; } = -1;
    public required long AddTimeTs { get; set; }
}

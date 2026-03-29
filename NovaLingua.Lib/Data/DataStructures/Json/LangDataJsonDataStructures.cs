using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NovaLingua.Lib.Data.DataStructures.Json;

internal class MetaData : IJsonDataStructure<MetaData>
{
    public required int Version { get; set; }
    public required string LangName { get; set; }
    public string LangVersion { get; set; } = "";
    public required int LocalLang { get; set; }
    // def lang, which all conlang words should be translated into
    public string LangDescription { get; set; } = "";
    public required long CreateTimeTs { get; set; }
    public required long LastModifyTimeTs { get; set; }

    public required bool AutoGenerationUseVariants { get; set; }
    public required bool ForceLetterVariantGlobalUnique { get; set; }
    // one variant of a letter cannot be same as another letter (or its upper & variants)
    public required bool ForceWordInflectionGlobalUnique { get; set; }
    // one inflection of a word cannot be same as another word (or its inflections)
    public required int MaxConsecutiveVowelsCount { get; set; }
    public required int MaxConsecutiveConsonantCount { get; set; }
    public required bool WordCaseInsensitive { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Version < 0;
    public static MetaData Empty => new()
    {
        Version = -1,
        LangName = "",
        LocalLang = -1,
        CreateTimeTs = -1,
        LastModifyTimeTs = -1,
        AutoGenerationUseVariants = false,
        ForceLetterVariantGlobalUnique = false,
        ForceWordInflectionGlobalUnique = false,
        MaxConsecutiveVowelsCount = -1,
        MaxConsecutiveConsonantCount = -1,
        WordCaseInsensitive = false
    };
    public static string TypeName => nameof(MetaData);
}

internal class AlphabetData : IJsonDataStructure<AlphabetData>
{
    public required int Version { get; set; }
    public required List<LetterData> Letters { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Version < 0;
    public static AlphabetData Empty => new()
    {
        Version = -1,
        Letters = []
    };
    public static string TypeName => nameof(AlphabetData);
}

internal class LetterData
{
    public required string Id { get; set; }
    public required int Type { get; set; }
    public required string Letter { get; set; }
    public required string LetterUppercase { get; set; }
    public string Comment { get; set; } = "";
    public required List<LetterVariantData> Variants { get; set; }
    // variants are special forms of a letter, they share max count & have the same placement rule

    public required int MaxInWordCount { get; set; }
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
    public required string Letter { get; set; }
    public required string LetterUppercase { get; set; }
    public string Comment { get; set; } = "";
    public required long AddTimeTs { get; set; }
}

internal class WordListData : IJsonDataStructure<WordListData>
{
    public required int Version { get; set; }
    public required List<WordData> Words { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Version < 0;
    public static WordListData Empty => new()
    {
        Version = -1,
        Words = [],
    };
    public static string TypeName => nameof(WordListData);
}

internal class WordData
{
    public required string Id { get; set; }
    public required List<WordLetterData> Letters { get; set; }
    public required List<WordDefinitionData> Definitions { get; set; }
    // there can be multiple defs
    public string Comment { get; set; } = "";
    public required List<WordInflectionData> Inflections { get; set; }
    public required long AddTimeTs { get; set; }
}

internal class WordInflectionData
{
    public required string Id { get; set; }
    public required List<WordLetterData> Letters { get; set; }
    public string Comment { get; set; } = "";
    public required long AddTimeTs { get; set; }
}

internal class WordLetterData
{
    public required string LetterId { get; set; }
    public required string VariantId { get; set; }
    public required bool UseUppercase { get; set; }
}

internal class WordDefinitionData
{
    public required int Class { get; set; }
    public required string Definition { get; set; }
}

internal class TodoListData : IJsonDataStructure<TodoListData>
{
    public required int Version { get; set; }
    public required List<TodoData> Todos { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Version < 0;
    public static TodoListData Empty => new()
    {
        Version = -1,
        Todos = []
    };
    public static string TypeName => nameof(TodoListData);
}

internal class TodoData
{
    public required string Msg { get; set; }
    public required long AddTimeTs { get; set; }
}

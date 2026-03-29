using System;
using System.Collections.Generic;
using System.Linq;
using NovaLingua.Lib.Data.DataStructures;
using NovaLingua.Lib.Data.DataStructures.Json;
using NovaLingua.Lib.Exceptions;
using NovaLingua.Lib.Extensions;
using static NovaLingua.Lib.Data.DataStructures.AbstractLangDataWord;

namespace NovaLingua.Lib.Data;

public class LangData
{
    public string LangName { get; set; } = "";
    public string LangVersion { get; set; } = "";
    public LocalLanguage LocalLang { get; set; }
    public string LangDescription { get; set; } = "";
    public long CreateTimeTs
    {
        get => _createTimeTs;
        set => _createTimeTs = Math.Max(0, value);
    }
    public long LastModifyTimeTs
    {
        get => _lastModifyTimeTs;
        set => _lastModifyTimeTs = Math.Max(0, value);
    }

    public LangDataConfig Config { get; set; } = new()
    {
        AutoGenerationUseVariants = false,
        ForceLetterVariantGlobalUnique = true,
        ForceWordUnique = true,
        ForceWordInflectionGlobalUnique = true,
        WordCaseInsensitive = true
    };
    public DoubleLinkedHashMap<string, LangDataLetter> Alphabet { get; set; } = new();
    public DoubleLinkedHashMap<string, LangDataWord> WordList { get; set; } = new();
    public List<LangDataTodo> Todos { get; set; } = [];

    public bool CheckWordList()
    {
        _wordListChecked = false;

        var wordSet = new HashSet<string>();
        foreach (var word in WordList.Nodes)
        {
            int letterIndex = 0;
            foreach (var letter in word.Value.Letters)
            {
                var letterId = letter.LetterId;
                var variantId = letter.VariantId;
                if (!CheckLetterId(letterId))
                {
                    return false;
                    //throw new LangDataException(LangDataErrorCode.InvalidValue,
                    //    WordListData.TypeName, $"Words/{word.Key}/Letters/{letterIndex}/LetterId", letterId
                    //);
                } // invalid letter id
                if (!string.IsNullOrEmpty(variantId) && !CheckLetterVariantId(letterId, variantId))
                {
                    return false;
                    //throw new LangDataException(LangDataErrorCode.InvalidValue,
                    //    WordListData.TypeName, $"Words/{word.Key}/Letters/{letterIndex}/VariantId", variantId
                    //);
                } // has variant id but invalid
                letterIndex++;
            } // walk letter list
            // check letters

            var thisWordLetterListStr = ConvertLetterListToString(word.Value.Letters, Config.WordCaseInsensitive);
            if (!wordSet.Add(thisWordLetterListStr))
            {
                return false;
                //throw new LangDataException(LangDataErrorCode.DataCollision,
                //    WordListData.TypeName,
                //    $"Words/{word.Key}/Letters", thisWordLetterListStr
                //);
            } // word collision
            // check word unique

            if (word.Value.Inflections.Count > 0)
            {
                var inflectionSet = new HashSet<string>();
                foreach (var inflection in word.Value.Inflections.Nodes)
                {
                    int inflectionLetterIndex = 0;
                    foreach (var letter in inflection.Value.Letters)
                    {
                        var letterId = letter.LetterId;
                        var variantId = letter.VariantId;
                        if (!CheckLetterId(letterId))
                        {
                            return false;
                            //throw new LangDataException(LangDataErrorCode.InvalidValue,
                            //    WordListData.TypeName, $"Words/{word.Key}/Inflections/{inflection.Key}/Letters/{inflectionLetterIndex}/LetterId", letterId
                            //);
                        } // invalid letter id
                        if (!string.IsNullOrEmpty(variantId) && !CheckLetterVariantId(letterId, variantId))
                        {
                            return false;
                            //throw new LangDataException(LangDataErrorCode.InvalidValue,
                            //    WordListData.TypeName, $"Words/{word.Key} /Inflections/{inflection.Key}/Letters/{inflectionLetterIndex}/VariantId", variantId
                            //);
                        } // has variant id but invalid
                        inflectionLetterIndex++;
                    } // walk letter list

                    var thisInflectionLetterListStr = ConvertLetterListToString(inflection.Value.Letters, Config.WordCaseInsensitive);
                    if (!inflectionSet.Add(thisInflectionLetterListStr))
                    {
                        return false;
                        //throw new LangDataException(LangDataErrorCode.DataCollision,
                        //    WordListData.TypeName,
                        //    $"Words/{word.Key}/Inflections/{inflection.Key}/Letters", thisInflectionLetterListStr
                        //);
                    } // inflection collision
                    if (Config.ForceWordInflectionGlobalUnique)
                    {
                        if (!wordSet.Add(thisInflectionLetterListStr))
                        {
                            return false;
                            //throw new LangDataException(LangDataErrorCode.DataCollision,
                            //    WordListData.TypeName,
                            //    $"Words/{word.Key}/Inflections/{inflection.Key}/Letters", thisInflectionLetterListStr
                            //);
                        } // inflection global collision
                    } // force inflection global unique
                    // check inflection unique
                } // walk inflection list
            } // word has inflections
        } // walk word list

        _wordListChecked = true;
        return true;
    }

    public bool CheckWordListIfNeeded() => _wordListChecked || CheckWordList();

    public bool SortWordList()
    {
        if (!CheckWordListIfNeeded())
        {
            return false;
        } // broken word list

        var wordList = WordList.Nodes
            .Select(kvp => (Id: kvp.Key, Word: kvp.Value))
            .ToList();
        SortWordListNoCheckThrow(wordList);
        WordList.RemoveAll();
        foreach (var (Id, Word) in wordList)
        {
            if (Word.Inflections.Count > 0)
            {
                var inflectionList = Word.Inflections.Nodes
                    .Select(kvp => (Id: kvp.Key, Inflection: kvp.Value))
                    .ToList();
                SortWordListNoCheckThrow(inflectionList);
                Word.Inflections.RemoveAll();
                foreach (var (inflectionId, Inflection) in inflectionList)
                {
                    if (!Word.Inflections.TryAddTail(inflectionId, Inflection))
                    {
                        throw new LangDataException(LangDataErrorCode.Unexpected,
                            WordListData.TypeName, $"Failed to add inflection [id={Id}, iid={inflectionId}]"
                        );
                    }
                }
            } // word has inflections
            if (!WordList.TryAddTail(Id, Word))
            {
                throw new LangDataException(LangDataErrorCode.Unexpected,
                    WordListData.TypeName, $"Failed to add word [id={Id}]"
                );
            }
        } // walk word list
        return true;
    }

    public bool UpdateAllWordsStringPreview()
    {
        if (!CheckWordListIfNeeded())
        {
            return false;
        } // broken word list
        
        _needUpdateWordsStringPreview = true;
        foreach (var word in WordList.Nodes)
        {
            UpdateWordStringPreviewNoCheckThrow(word.Key, "", word.Value);
            if (word.Value.Inflections.Count > 0)
            {
                foreach (var inflection in word.Value.Inflections.Nodes)
                {
                    UpdateWordStringPreviewNoCheckThrow(word.Key, inflection.Key, inflection.Value);
                } // walk inflection list
            } // word has inflections
        } // walk word list
        _needUpdateWordsStringPreview = false;
        return true;
    }

    public bool UpdateAllWordsStringPreviewIfNeeded() => _needUpdateWordsStringPreview || UpdateAllWordsStringPreview();

    public bool UpdateWordStringPreview(string wordId)
    {
        if (!CheckWordListIfNeeded())
        {
            return false;
        } // broken word list

        if (WordList.TryGetValue(wordId, out var wordData))
        {
            UpdateWordStringPreviewNoCheckThrow(wordId, "", wordData);
            if (wordData.Inflections.Count > 0)
            {
                foreach (var inflection in wordData.Inflections.Nodes)
                {
                    UpdateWordStringPreviewNoCheckThrow(wordId, inflection.Key, inflection.Value);
                } // walk inflection list
            }
            return true;
        }
        else
        {
            return false;
        } // word id doesn't exist
    }

    public bool UpdateWordStringPreviewIfNeeded(string wordId) => _needUpdateWordsStringPreview || UpdateWordStringPreview(wordId);

    private bool CheckLetterId(string id) => id.IsValidLetterId() && Alphabet.ContainsKey(id);

    private bool CheckLetterVariantId(string letterId, string variantId)
    {
        if (!letterId.IsValidLetterId() || !variantId.IsValidLetterId())
        {
            return false;
        } // invalid id(s)
        if (Alphabet.TryGetValue(letterId, out var letterData))
        {
            if (!letterData.Variants.ContainsKey(variantId))
            {
                return false;
            } // variant id doesn't exist
        }
        else
        {
            return false;
        } // letter id doesn't exist
        return true;
    }

    private static string ConvertLetterListToString(List<Letter> letterList, bool caseInsensitive)
    {
        var outStr = "";
        foreach (var letter in letterList)
        {
            outStr += letter.LetterId;
            if (!string.IsNullOrEmpty(letter.VariantId))
            {
                outStr += "-" + letter.VariantId;
            }
            if (!caseInsensitive && letter.UseUppercase)
            {
                outStr += "U";
            }
            outStr += "|";
        }
        return outStr;
    }

    // Order in double linked hash map should be set before calling this
    void SortWordListNoCheckThrow<T>(List<(string Id, T Word)> wordList) where T : AbstractLangDataWord => wordList.Sort((a, b) =>
    {
        var letterListA = a.Word.Letters;
        var letterListB = b.Word.Letters;

        int minLength = Math.Min(letterListA.Count, letterListB.Count);
        for (int i = 0; i < minLength; i++)
        {
            var idA = letterListA[i].LetterId;
            var idB = letterListB[i].LetterId;
            if (idA == idB)
            {
                var vidA = letterListA[i].VariantId;
                var vidB = letterListB[i].VariantId;
                if (vidA == vidB)
                {
                    continue;
                } // same letter, same variant

                if (Alphabet.TryGetValue(idA, out var letterData))
                {

                    #region LocalFunction

                    int GetVariantRank(string vid)
                    {
                        if (string.IsNullOrEmpty(vid))
                        {
                            return -1;
                        }
                        return letterData.Variants.TryGetValue(vid, out var variantData) ? variantData.Order
                            : throw new LangDataException(LangDataErrorCode.Unexpected,
                                WordListData.TypeName, $"Failed to get letter variant data when sorting word list [id={idA}, vid={vid}]"
                        );
                    }

                    #endregion LocalFunction

                    int rankA = GetVariantRank(vidA);
                    int rankB = GetVariantRank(vidB);
                    return rankA.CompareTo(rankB);
                }
                else
                {
                    throw new LangDataException(LangDataErrorCode.Unexpected,
                        WordListData.TypeName, $"Failed to get letter data when sorting word list [id={idA}]"
                    );
                } // failed to get letter data
            } // same letter
            else
            {

                #region LocalFunction

                int GetRank(string id)
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        return -1;
                    } // for later enhancement, no use for now
                    return Alphabet.TryGetValue(id, out var letterData) ? letterData.Order
                        : throw new LangDataException(LangDataErrorCode.Unexpected,
                            WordListData.TypeName, $"Failed to get letter data when sorting word list [id={id}]"
                    );
                }

                #endregion LocalFunction

                int rankA = GetRank(idA);
                int rankB = GetRank(idB);
                return rankA.CompareTo(rankB);
            } // different letter
        }

        return letterListA.Count.CompareTo(letterListB.Count);
        // all letters before MinLength pos are same, shorter one first
    });

    private void UpdateWordStringPreviewNoCheckThrow<T>(string wordId, string inflectionId, T wordData) where T : AbstractLangDataWord
    {
        string preview = "";
        foreach (var letter in wordData.Letters)
        {
            var letterId = letter.LetterId;
            var variantId = letter.VariantId;
            var letterToUseStr = "";
            var letterToUseUppercaseStr = "";
            if (Alphabet.TryGetValue(letterId, out var letterData))
            {
                if (!string.IsNullOrEmpty(variantId))
                {
                    if (letterData.Variants.TryGetValue(variantId, out var variantData))
                    {
                        letterToUseStr = variantData.Letter;
                        letterToUseUppercaseStr = variantData.LetterUppercase;
                    }
                    else
                    {
                        throw new LangDataException(LangDataErrorCode.Unexpected,
                            WordListData.TypeName,
                            $"Failed to get variant data after successful check [wordId={wordId}, inflectionId={inflectionId}, letterId={letterId}, variantId={variantId}]"
                        );
                        // this should never happen
                    } // failed to get variant data
                } // use variant
                else
                {
                    letterToUseStr = letterData.Letter;
                    letterToUseUppercaseStr = letterData.LetterUppercase;
                } // use letter
            }
            else
            {
                throw new LangDataException(LangDataErrorCode.Unexpected,
                    WordListData.TypeName,
                    $"Failed to get letter data after successful check [wordId={wordId}, inflectionId={inflectionId}, letterId={letterId}]"
                );
                // this should never happen
            } // failed to get letter data
            preview += letter.UseUppercase ? letterToUseUppercaseStr : letterToUseStr;
        } // walk letters
        wordData.WordStringPreview = preview;
        return;
    }

    private long _createTimeTs;
    private long _lastModifyTimeTs;
    private bool _needUpdateWordsStringPreview = true;
    private bool _wordListChecked = false;
}

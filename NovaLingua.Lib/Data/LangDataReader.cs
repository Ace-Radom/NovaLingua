using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using NovaLingua.Lib.Data.DataStructures;
using NovaLingua.Lib.Data.DataStructures.Json;
using NovaLingua.Lib.Exceptions;
using NovaLingua.Lib.Extensions;
using static NovaLingua.Lib.Data.DataStructures.AbstractLangDataWord;

namespace NovaLingua.Lib.Data;

public static class LangDataReader
{
    public static LangData ReadNld(string zipFilePath)
    {
        var data = new LangData();

        if (Path.GetExtension(zipFilePath) != ".nld")
        {
            throw new FileException(FileErrorCode.UnexpectedExt, zipFilePath, ".nld");
        }

        using var zipFileStream = SafeOpenReadFileStream(zipFilePath);
        using var zipArchive = SafeOpenReadZipArchive(zipFileStream, zipFilePath);

        var metaData = MetaData.Empty;
        var alphabetData = AlphabetData.Empty;
        var wordListData = WordListData.Empty;
        var todoListData = TodoListData.Empty;

        #region ZipArchiveRead

        foreach (var entry in zipArchive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            } // skip directories for now

            string fileName = entry.Name.ToLower();
            string fileExt = Path.GetExtension(fileName);

            using var entryStream = SafeOpenZipArchiveEntryStream(entry, zipFilePath);
            if (fileExt == ".json")
            {
                using var reader = new StreamReader(entryStream);
                string jsonData = SafeReadStreamReaderToEnd(reader, $"{zipFilePath}/{fileName}", "Json inside Zip Archive");
                if (fileName == "meta.json")
                {
                    metaData = SafeDeserializeJsonData<MetaData>(jsonData);
                }
                else if (fileName == "alphabet.json")
                {
                    alphabetData = SafeDeserializeJsonData<AlphabetData>(jsonData);
                }
                else if (fileName == "wordlist.json")
                {
                    wordListData = SafeDeserializeJsonData<WordListData>(jsonData);
                }
                else if (fileName == "todolist.json")
                {
                    todoListData = SafeDeserializeJsonData<TodoListData>(jsonData);
                }
                else
                {
                    // TODO: log unknown file found
                }
            }
            else
            {
                // TODO: log unknown file format found
            }
        }

        var notFoundPartList = GetNotFoundPartNames().ToList();
        if (notFoundPartList.Count != 0)
        {
            string notFoundPartListString = string.Join(", ", notFoundPartList);
            throw new LangDataException(LangDataErrorCode.RequiredPartNotFound, notFoundPartListString);
        } // one / some required part(s) not found
        // check all required parts exist

        #endregion ZipArchiveRead

        #region MetadataRead

        data.LangName = metaData.LangName;
        data.LangVersion = metaData.LangVersion;
        data.LocalLang = metaData.LocalLang.ToLocalLanguage() switch
        {
            LocalLanguage.Unknown => throw new LangDataException(
                LangDataErrorCode.InvalidValue,
                MetaData.TypeName, "LocalLang", metaData.LocalLang.ToString()
            ),
            var lang => lang
        };
        data.LangDescription = metaData.LangDescription;
        data.CreateTimeTs = metaData.CreateTimeTs; // value checked by setter
        data.LastModifyTimeTs = metaData.LastModifyTimeTs; // value checked by setter
        data.Config = new()
        {
            AutoGenerationUseVariants = metaData.AutoGenerationUseVariants,
            ForceLetterVariantGlobalUnique = metaData.ForceLetterVariantGlobalUnique,
            ForceWordUnique = metaData.ForceWordUnique,
            ForceWordInflectionGlobalUnique = metaData.ForceWordInflectionGlobalUnique,
            MaxConsecutiveVowelsCount = metaData.MaxConsecutiveVowelsCount, // value checked by setter
            MaxConsecutiveConsonantCount = metaData.MaxConsecutiveConsonantCount, // value checked by setter
            WordCaseInsensitive = metaData.WordCaseInsensitive
        };

        #endregion MetadataRead

        #region AlphabetRead

        #region LettersRead

        var letterSet = new HashSet<string>(); // set for checking letter collision
        foreach (var letter in alphabetData.Letters)
        {
            var id = letter.Id;
            if (data.Alphabet.ContainsKey(id))
            {
                throw new LangDataException(LangDataErrorCode.IdCollision,
                    AlphabetData.TypeName, "Letters", id
                );
            } // id collision
            // check if id already exists

            #region LetterCheck

            var thisLetter = letter.Letter;
            var thisLetterUppercase = letter.LetterUppercase;
            if (letterSet.Contains(thisLetter))
            {
                throw new LangDataException(LangDataErrorCode.DataCollision,
                    AlphabetData.TypeName, $"Letters/{id}/Letter", thisLetter
                );
            } // letter collision
            letterSet.Add(thisLetter);
            if (letterSet.Contains(thisLetterUppercase))
            {
                throw new LangDataException(LangDataErrorCode.DataCollision,
                    AlphabetData.TypeName, $"Letters/{id}/LetterUppercase", thisLetterUppercase
                );
            } // letter uppercase collision
            letterSet.Add(thisLetterUppercase);
            // check if letter (and its uppercase) already exists

            // TODO: check letter legality (with regex)

            #endregion LetterCheck

            var thisLetterData = new LangDataLetter()
            {
                Type = letter.Type.ToLetterType() switch
                {
                    LetterType.Unknown => throw new LangDataException(
                        LangDataErrorCode.InvalidValue,
                        AlphabetData.TypeName, $"Letters/{id}/Type", letter.Type.ToString()
                    ),
                    var type => type
                },
                Letter = thisLetter,
                LetterUppercase = thisLetterUppercase,
                Comment = letter.Comment,
                MaxInWordCount = letter.MaxInWordCount, // value checked by setter
                PlacementRule = letter.PlacementRule.ToLetterPlacementRule() switch
                {
                    LetterPlacementRule.Unknown => throw new LangDataException(
                        LangDataErrorCode.InvalidValue,
                        AlphabetData.TypeName, $"Letters/{id}/PlacementRule", letter.PlacementRule.ToString()
                    ),
                    var rule => rule
                },
                AllowInAutoGeneration = letter.AllowInAutoGeneration,
                AutoGenerationRate = letter.AutoGenerationRate, // value checked by setter
                AddTimeTs = letter.AddTimeTs, // value checked by setter
            }; // build basic letter data

            #region VariantsRead

            var variantSet = new HashSet<string>();
            foreach (var variant in letter.Variants)
            {
                var vid = variant.Id;
                if (thisLetterData.Variants.ContainsKey(vid))
                {
                    throw new LangDataException(LangDataErrorCode.IdCollision,
                        AlphabetData.TypeName, $"Letters/{id}/Variants", vid
                    );
                } // variant id collision

                #region VariantLetterCheck

                var thisVariant = variant.Letter;
                var thisVariantUppercase = variant.LetterUppercase;
                if (variantSet.Contains(thisVariant))
                {
                    throw new LangDataException(LangDataErrorCode.DataCollision,
                        AlphabetData.TypeName, $"Letters/{id}/Variants/{vid}/Letter", thisVariant
                    );
                } // variant letter collision
                variantSet.Add(thisVariant);
                if (variantSet.Contains(thisVariantUppercase))
                {
                    throw new LangDataException(LangDataErrorCode.DataCollision,
                        AlphabetData.TypeName,
                        $"Letters/{id}/Variants/{vid}/LetterUppercase", thisVariantUppercase
                    );
                } // variant letter uppercase collision
                variantSet.Add(thisVariantUppercase);
                if (data.Config.ForceLetterVariantGlobalUnique)
                {
                    if (letterSet.Contains(thisVariant))
                    {
                        throw new LangDataException(LangDataErrorCode.DataCollision,
                            AlphabetData.TypeName,
                            $"Letters/{id}/Variants/{vid}/Letter", thisVariant
                        );
                    } // variant letter global collision
                    letterSet.Add(thisVariant);
                    if (letterSet.Contains(thisVariantUppercase))
                    {
                        throw new LangDataException(LangDataErrorCode.DataCollision,
                            AlphabetData.TypeName,
                            $"Letters/{id}/Variants/{vid}/LetterUppercase", thisVariantUppercase
                        );
                    } // variant letter uppercase global collision
                    letterSet.Add(thisVariantUppercase);
                } // variants global unique check enabled
                // check if variant letter (and its uppercase) already exists

                #endregion VariantLetterCheck

                var thisVowelVariantData = new LangDataLetterVariant()
                {
                    Letter = thisVariant,
                    LetterUppercase = thisVariantUppercase,
                    Comment = variant.Comment,
                    AddTimeTs = variant.AddTimeTs // value checked by setter
                };

                if (!thisLetterData.Variants.TryAddTail(vid, thisVowelVariantData))
                {
                    throw new LangDataException(LangDataErrorCode.Unexpected,
                        AlphabetData.TypeName, $"Failed to add variant [id={id}, vid={vid}]"
                    );
                    // this should never happen
                } // insert node failed
            } // read variants

            #endregion VariantsRead

            if (!thisLetterData.Variants.Check(setOrder: true))
            {
                throw new LangDataException(LangDataErrorCode.InvalidValue,
                    AlphabetData.TypeName, $"Letters/{id}/Variants", "Illegal Linked List"
                );
            } // illegal variant linked list
            // check variants double linked hash map

            if (!data.Alphabet.TryAddTail(id, thisLetterData))
            {
                throw new LangDataException(LangDataErrorCode.Unexpected,
                    AlphabetData.TypeName, $"Failed to add letter [id={id}]"
                );
                // this should never happen
            } // insert node failed
        } // read letters

        #endregion LettersRead

        if (!data.Alphabet.Check(setOrder: true))
        {
            throw new LangDataException(LangDataErrorCode.InvalidValue,
                AlphabetData.TypeName, $"Letters", "Illegal Linked List"
            );
        } // illegal letter linked list
        // check letters double linked hash map

        #endregion AlphabetRead

        #region WordListRead

        HashSet<string> wordIdSet = [];
        HashSet<string> wordSet = [];
        List<(string Id, LangDataWord Node)> wordListTemp = [];
        foreach (var word in wordListData.Words)
        {
            var id = word.Id;
            if (!wordIdSet.Add(id))
            {
                throw new LangDataException(LangDataErrorCode.IdCollision,
                    WordListData.TypeName, "Words", id
                );
            } // id collision

            var thisWordData = new LangDataWord()
            {
                Letters = word.Letters.Select((l, index) =>
                {
                    var letterId = l.LetterId;
                    var variantId = l.VariantId;
                    if (!CheckLetterId(letterId))
                    {
                        throw new LangDataException(LangDataErrorCode.InvalidValue,
                            WordListData.TypeName, $"Words/{id}/Letters/{index}/LetterId", letterId
                        );
                    } // invalid letter id
                    if (!string.IsNullOrEmpty(variantId) && !CheckLetterVariantId(letterId, variantId))
                    {
                        throw new LangDataException(LangDataErrorCode.InvalidValue,
                            WordListData.TypeName, $"Words/{id}/Letters/{index}/VariantId", variantId
                        );
                    } // has variant id but invalid
                    return new Letter()
                    {
                        LetterId = letterId,
                        VariantId = variantId,
                        UseUppercase = l.UseUppercase
                    };
                }).ToList(),
                Definitions = word.Definitions.Select((d, index) => new LangDataWord.WordDefinition()
                {
                    Class = d.Class.ToWordClass() switch
                    {
                        WordClass.Unknown => throw new LangDataException(
                            LangDataErrorCode.InvalidValue,
                            WordListData.TypeName, $"Words/{id}/Definitions/{index}/Class", d.Class.ToString()
                        ),
                        var wordClass => wordClass
                    },
                    Definition = d.Definition
                }).ToList(),
                Comment = word.Comment,
                AddTimeTs = word.AddTimeTs,
            }; // build basic word data

            var thisWordLetterListStr = ConvertLetterListToString(thisWordData.Letters, data.Config.WordCaseInsensitive);
            if (!wordSet.Add(thisWordLetterListStr))
            {
                throw new LangDataException(LangDataErrorCode.DataCollision,
                    WordListData.TypeName,
                    $"Words/{id}/Letters", thisWordLetterListStr
                );
            } // word collision
            // check word unique

            // TODO: build word string cache

            #region InflectionsRead

            if (word.Inflections.Count > 0)
            {
                var inflectionIdSet = new HashSet<string>();
                var inflectionSet = new HashSet<string>();
                var inflectionsListTemp = new List<(string Id, LangDataWordInflection Node)>();
                foreach (var inflection in word.Inflections)
                {
                    var iid = inflection.Id;
                    if (!inflectionIdSet.Add(iid))
                    {
                        throw new LangDataException(LangDataErrorCode.IdCollision,
                            WordListData.TypeName, $"Words/{id}/Inflections", iid
                        );
                    } // inflection id collision

                    var thisInflectionData = new LangDataWordInflection()
                    {
                        Letters = inflection.Letters.Select((l, index) =>
                        {
                            var letterId = l.LetterId;
                            var variantId = l.VariantId;
                            if (!CheckLetterId(letterId))
                            {
                                throw new LangDataException(LangDataErrorCode.InvalidValue,
                                    WordListData.TypeName, $"Words/{id}/Inflections/Letters/{index}/LetterId", letterId
                                );
                            } // invalid letter id
                            if (!string.IsNullOrEmpty(variantId) && !CheckLetterVariantId(letterId, variantId))
                            {
                                throw new LangDataException(LangDataErrorCode.InvalidValue,
                                    WordListData.TypeName, $"Words/{id}/Inflections/Letters/{index}/VariantId", variantId
                                );
                            } // has variant id but invalid
                            return new Letter()
                            {
                                LetterId = letterId,
                                VariantId = variantId,
                                UseUppercase = l.UseUppercase
                            };
                        }).ToList(),
                        Comment = inflection.Comment,
                        AddTimeTs = inflection.AddTimeTs,
                    }; // build inflection data

                    var thisInflectionLetterListStr = ConvertLetterListToString(thisInflectionData.Letters, data.Config.WordCaseInsensitive);
                    if (!inflectionSet.Add(thisInflectionLetterListStr))
                    {
                        throw new LangDataException(LangDataErrorCode.DataCollision,
                            WordListData.TypeName,
                            $"Words/{id}/Inflections/{iid}/Letters", thisInflectionLetterListStr
                        );
                    } // inflection collision
                    if (data.Config.ForceWordInflectionGlobalUnique)
                    {
                        if (!wordSet.Add(thisInflectionLetterListStr))
                        {
                            throw new LangDataException(LangDataErrorCode.DataCollision,
                                WordListData.TypeName,
                                $"Words/{id}/Inflections/{iid}/Letters", thisInflectionLetterListStr
                            );
                        } // inflection global collision
                    } // force inflection global unique
                    // check inflection unique

                    // TODO: build inflection str cache

                    inflectionsListTemp.Add((Id: iid, Node: thisInflectionData));
                } // walk inflection list

                SortWordListNoCheckThrow(inflectionsListTemp);
                foreach (var (Id, Node) in inflectionsListTemp)
                {
                    if (!thisWordData.Inflections.TryAddTail(Id, Node))
                    {
                        throw new LangDataException(LangDataErrorCode.Unexpected,
                            WordListData.TypeName, $"Failed to add inflection [id={id}, iid={Id}]"
                        );
                    }
                } // insert inflections into word data after sort
            } // word has inflections

            #endregion InflectionsRead

            wordListTemp.Add((Id: id, Node: thisWordData));
        } // walk word list

        SortWordListNoCheckThrow(wordListTemp);
        foreach (var (Id, Node) in wordListTemp)
        {
            if (!data.WordList.TryAddTail(Id, Node))
            {
                throw new LangDataException(LangDataErrorCode.Unexpected,
                    WordListData.TypeName, $"Failed to add inflection [id={Id}]"
                );
            }
        } // insert words into data after sort

        #endregion WordListRead

        #region TodoListRead

        data.Todos = todoListData.Todos.Select(v => new LangDataTodo()
        {
            Msg = v.Msg,
            AddTimeTs = v.AddTimeTs // value checked by setter
        }).OrderBy(v => v.AddTimeTs).ThenBy(v => v.Msg).ToList();

        #endregion TodoListRead

        // TODO: check parts format

        return data;

        #region LocalFunction

        static FileStream SafeOpenReadFileStream(string path)
        {
            try
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                throw ex switch
                {
                    FileNotFoundException or DirectoryNotFoundException => new FileException(FileErrorCode.NotFound, path),
                    UnauthorizedAccessException => new FileException(FileErrorCode.AccessDenied, path),
                    ArgumentException => new FileException(FileErrorCode.InvalidPath, path),
                    IOException => new FileException(FileErrorCode.IOError, path, ex.Message),
                    _ => new FileException(FileErrorCode.Others, path, ex.Message)
                };
            }
        }

        static ZipArchive SafeOpenReadZipArchive(FileStream zipFileStream, string zipFilePath)
        {
            try
            {
                return new ZipArchive(zipFileStream, ZipArchiveMode.Read);
            }
            catch (Exception ex)
            {
                throw ex switch
                {
                    InvalidDataException => new FileException(FileErrorCode.InvalidFileFormat, zipFilePath, "Zip Archive"),
                    _ => new FileException(FileErrorCode.Others, zipFilePath, ex.Message)
                };
            }
        }

        static Stream SafeOpenZipArchiveEntryStream(ZipArchiveEntry entry, string zipFilePath)
        {
            try
            {
                return entry.Open();
            }
            catch (Exception ex)
            {
                throw ex switch
                {
                    InvalidDataException => new FileException(FileErrorCode.InvalidFileFormat, zipFilePath, "Zip Archive"),
                    IOException => new FileException(FileErrorCode.IOError, zipFilePath, ex.Message),
                    _ => new FileException(FileErrorCode.Others, zipFilePath, ex.Message)
                };
            }
        }

        static string SafeReadStreamReaderToEnd(StreamReader reader, string filePath, string expectedFileFormat)
        {
            try
            {
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex switch
                {
                    InvalidDataException => new FileException(FileErrorCode.InvalidFileFormat, filePath, expectedFileFormat),
                    IOException => new FileException(FileErrorCode.IOError, filePath, ex.Message),
                    _ => new FileException(FileErrorCode.Others, filePath, ex.Message)
                };
            }
        }

        static T SafeDeserializeJsonData<T>(string jsonData) where T : IJsonDataStructure<T>
        {
            try
            {
                var option = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var deserializedData = JsonSerializer.Deserialize<T>(jsonData, option);
                return deserializedData ?? T.Empty;
            }
            catch (Exception ex)
            {
                throw ex switch
                {
                    JsonException => new LangDataException(LangDataErrorCode.InvalidPartFormat, T.TypeName, ex.Message),
                    _ => new LangDataException(LangDataErrorCode.Others, T.TypeName, ex.Message)
                };
            }
        }

        IEnumerable<string> GetNotFoundPartNames()
        {
            if (metaData.IsEmpty) yield return MetaData.TypeName;
            if (alphabetData.IsEmpty) yield return AlphabetData.TypeName;
            if (wordListData.IsEmpty) yield return WordListData.TypeName;
            if (todoListData.IsEmpty) yield return TodoListData.TypeName;
        }

        bool CheckLetterId(string id) => (!id.IsValidLetterId() || !data.Alphabet.ContainsKey(id));

        bool CheckLetterVariantId(string letterId, string variantId)
        {
            if (!letterId.IsValidLetterId() || !variantId.IsValidLetterId())
            {
                return false;
            } // invalid id(s)
            if (data.Alphabet.TryGetValue(letterId, out var letterData))
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

        static string ConvertLetterListToString(List<Letter> letterList, bool caseInsensitive)
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

                        if (data.Alphabet.TryGetValue(idA, out var letterData))
                        {
                            uint rankA = letterData.Variants.TryGetValue(vidA, out var variantAData) ? variantAData.Order
                                : throw new LangDataException(LangDataErrorCode.Unexpected,
                                    WordListData.TypeName, $"Failed to get letter variant data when sorting word list [id={idA}, vid={vidA}]"
                            );
                            uint rankB = letterData.Variants.TryGetValue(vidB, out var variantBData) ? variantBData.Order
                                : throw new LangDataException(LangDataErrorCode.Unexpected,
                                    WordListData.TypeName, $"Failed to get letter variant data when sorting word list [id={idA}, vid={vidB}]"
                            );
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
                        uint rankA = data.Alphabet.TryGetValue(idA, out var letterAData) ? letterAData.Order
                            : throw new LangDataException(LangDataErrorCode.Unexpected,
                                WordListData.TypeName, $"Failed to get letter data when sorting word list [id={idA}]"
                        );
                        uint rankB = data.Alphabet.TryGetValue(idB, out var letterBData) ? letterBData.Order
                            : throw new LangDataException(LangDataErrorCode.Unexpected,
                                WordListData.TypeName, $"Failed to get letter data when sorting word list [id={idB}]"
                        );
                        return rankA.CompareTo(rankB);
                    } // different letter
                }

                return letterListA.Count.CompareTo(letterListB.Count);
                // all letters before MinLength pos are same, shorter one first
            });

        #endregion LocalFunction
    }
}

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
            ForceWordDefinitionUnique = metaData.ForceWordDefinitionUnique,
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
            if (data.Alphabet.Letters.ContainsKey(id))
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
                PrevLetterId = letter.PrevLetterId,
                NextLetterId = letter.NextLetterId,
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
                    PrevLetterId = variant.PrevLetterId,
                    NextLetterId = variant.NextLetterId,
                    Comment = variant.Comment,
                    AddTimeTs = variant.AddTimeTs // value checked by setter
                };

                thisLetterData.Variants.Add(vid, thisVowelVariantData);
            } // read variants

            #endregion VariantsRead

            thisLetterData.HeadVariantId = letter.HeadVariantId;
            thisLetterData.TailVariantId = letter.TailVariantId;
            if (!CheckLetterLinkedList(thisLetterData.HeadVariantId, thisLetterData.TailVariantId, thisLetterData.Variants))
            {
                throw new LangDataException(LangDataErrorCode.InvalidValue,
                    AlphabetData.TypeName, $"Letters/{id}/Variants", "Illegal Linked List"
                );
            } // illegal variant linked list

            data.Alphabet.Letters.Add(id, thisLetterData);
        } // read letters

        #endregion LettersRead

        data.Alphabet.HeadLetterId = alphabetData.HeadLetterId;
        data.Alphabet.TailLetterId = alphabetData.TailLetterId;
        if (!CheckLetterLinkedList(data.Alphabet.HeadLetterId, data.Alphabet.TailLetterId, data.Alphabet.Letters))
        {
            throw new LangDataException(LangDataErrorCode.InvalidValue,
                AlphabetData.TypeName, $"Letters", "Illegal Linked List"
            );
        } // illegal letter linked list

        #endregion AlphabetRead

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

        static bool CheckLetterLinkedList<T>(string headId, string tailId, Dictionary<string, T> list) where T : AbstractLangDataLetter
        {
            if (list.Count == 0)
            {
                return true;
            } // empty list
            if (headId == "" || tailId == "")
            {
                return false;
            } // list not empty but head / tail not specified
            if (list.Count == 1 && headId != tailId)
            {
                return false;
            } // one element but different head & tail
            if (!list.ContainsKey(headId) || !list.ContainsKey(tailId))
            {
                return false;
            } // head / tail not in list

            int count = 0;
            string ptr = headId;
            string prevPtr = "";
            while (true)
            {
                count++;
                if (count > list.Count)
                {
                    return false;
                } // loop

                if (list.TryGetValue(ptr, out var node))
                {
                    if (node.PrevLetterId != prevPtr)
                    {
                        return false;
                    } // wrong prev letter id
                    if (ptr == tailId)
                    {
                        if (node.NextLetterId != "")
                        {
                            return false;
                        } // tail shouldn't have next letter id
                        break;
                    }
                    if (node.NextLetterId == "")
                    {
                        return false;
                    } // we havn't reached tail, but no next letter id
                    prevPtr = ptr;
                    ptr = node.NextLetterId;
                }
                else
                {
                    return false;
                } // id doesn't exist
            } // walk linked list

            if (count != list.Count)
            {
                return false;
            }
            return true;
        }

        #endregion LocalFunction
    }
}

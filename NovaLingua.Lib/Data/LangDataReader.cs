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

        var notFoundPartList = GetNotFoundPartList().ToArray();
        if (notFoundPartList.Length != 0)
        {
            string notFoundPartListString = string.Join(", ", notFoundPartList);
            throw new LangDataException(LangDataErrorCode.RequiredPartNotFound, notFoundPartListString);
        } // one / some required part(s) not found
        // check all required parts exist

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
            WordCaseInsensitive = metaData.WordCaseInsensitive
        };
        // read metadata

        var letterSet = new HashSet<string>(); // set for checking letter collision
        var alphabeticOrderList = new List<(int Order, string Id)>(); // list for checking alphabetic order collision
        // the tuple stores (order, id), for later quick reordering alphabetic orders (e.g. {0,3,4} -> {0,1,2})
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

            var thisAlphabeticOrder = letter.AlphabeticOrder;
            if (alphabeticOrderList.Exists(m => m.Order == thisAlphabeticOrder))
            {
                throw new LangDataException(LangDataErrorCode.DataCollision,
                    AlphabetData.TypeName, $"Letters/{id}/AlphabeticOrder", thisAlphabeticOrder.ToString()
                );
            } // alphabetic order collision
            alphabeticOrderList.Add((thisAlphabeticOrder, id));
            // check if alphabetic order already exists

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
                AlphabeticOrder = thisAlphabeticOrder,
                Comment = letter.Comment,
                MaxCountInWord = letter.MaxCountInWord,
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

            var variantLetterSet = new HashSet<string>();
            var variantAlphabeticOrderList = new List<(int Order, string Id)>();
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

                var thisVariantLetter = variant.Letter;
                var thisVariantLetterUppercase = variant.LetterUppercase;
                if (variantLetterSet.Contains(thisVariantLetter))
                {
                    throw new LangDataException(LangDataErrorCode.DataCollision,
                        AlphabetData.TypeName, $"Letters/{id}/Variants/{vid}/Letter", thisVariantLetter
                    );
                } // variant letter collision
                variantLetterSet.Add(thisVariantLetter);
                if (variantLetterSet.Contains(thisVariantLetterUppercase))
                {
                    throw new LangDataException(LangDataErrorCode.DataCollision,
                        AlphabetData.TypeName,
                        $"Letters/{id}/Variants/{vid}/LetterUppercase", thisVariantLetterUppercase
                    );
                } // variant letter uppercase collision
                variantLetterSet.Add(thisVariantLetterUppercase);
                if (data.Config.ForceLetterVariantGlobalUnique)
                {
                    if (letterSet.Contains(thisVariantLetter))
                    {
                        throw new LangDataException(LangDataErrorCode.DataCollision,
                            AlphabetData.TypeName,
                            $"Letters/{id}/Variants/{vid}/Letter", thisVariantLetter
                        );
                    } // variant letter global collision
                    letterSet.Add(thisVariantLetter);
                    if (letterSet.Contains(thisVariantLetterUppercase))
                    {
                        throw new LangDataException(LangDataErrorCode.DataCollision,
                            AlphabetData.TypeName,
                            $"Letters/{id}/Variants/{vid}/LetterUppercase", thisVariantLetterUppercase
                        );
                    } // variant letter uppercase global collision
                    letterSet.Add(thisVariantLetterUppercase);
                } // variants global unique check enabled
                // check if variant letter (and its uppercase) already exists

                var thisVariantAlphabeticOrder = variant.AlphabeticOrder;
                if (variantAlphabeticOrderList.Exists(m => m.Order == thisVariantAlphabeticOrder))
                {
                    throw new LangDataException(LangDataErrorCode.DataCollision,
                        AlphabetData.TypeName,
                        $"Letters/{id}/Variants/{vid}/AlphabeticOrder", thisVariantAlphabeticOrder.ToString()
                    );
                } // variant alphabetic order collision
                variantAlphabeticOrderList.Add((thisVariantAlphabeticOrder, vid));
                // check if variant alphabetic order already exists

                #endregion VariantLetterCheck

                var thisVowelVariantData = new LangDataLetterVariant()
                {
                    Letter = thisVariantLetter,
                    LetterUppercase = thisVariantLetterUppercase,
                    AlphabeticOrder = thisVariantAlphabeticOrder,
                    Comment = variant.Comment,
                    AddTimeTs = variant.AddTimeTs // value checked by setter
                };

                thisLetterData.Variants.Add(vid, thisVowelVariantData);
            } // read variants

            variantAlphabeticOrderList = variantAlphabeticOrderList
                .OrderBy(x => x.Order)
                .Select((item, index) => (index, item.Id))
                .ToList();
            foreach (var (order, vid) in variantAlphabeticOrderList)
            {
                if (!thisLetterData.Variants.TryUpdateValue(vid, v => v.AlphabeticOrder = order))
                {
                    throw new LangDataException(LangDataErrorCode.Unexpected,
                        AlphabetData.TypeName, "Variant disappeared, wtf?"
                    );
                }
            } // reorder variants

            data.Alphabet.Letters.Add(id, thisLetterData);
        } // read letters
        alphabeticOrderList = alphabeticOrderList
            .OrderBy(x => x.Order)
            .Select((item, index) => (index, item.Id))
            .ToList();
        foreach (var (order, id) in alphabeticOrderList)
        {
            if (!data.Alphabet.Letters.TryUpdateValue(id, v => v.AlphabeticOrder = order))
            {
                throw new LangDataException(LangDataErrorCode.Unexpected,
                    AlphabetData.TypeName, "Letter disappeared, wtf?"
                );
            }
        } // reorder letters
        // read alphabet

        // TODO: check function (read alphabet, TryUpdateValue)

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

        IEnumerable<string> GetNotFoundPartList()
        {
            if (metaData.IsEmpty) yield return MetaData.TypeName;
            if (alphabetData.IsEmpty) yield return AlphabetData.TypeName;
            if (wordListData.IsEmpty) yield return WordListData.TypeName;
            if (todoListData.IsEmpty) yield return TodoListData.TypeName;
        }

        #endregion LocalFunction
    }
}

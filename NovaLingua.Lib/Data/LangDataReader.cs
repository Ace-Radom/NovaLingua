using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using NovaLingua.Lib.Data.DataStructures;
using NovaLingua.Lib.Exceptions;

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

        static T SafeDeserializeJsonData<T>(string jsonData) where T : IDataStructure<T>
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
                    JsonException => new LangDataException(LangDataErrorCode.InvalidPartFormat, T.Type),
                    _ => new LangDataException(LangDataErrorCode.Others, T.Type, ex.Message)
                };
            }
        }

        IEnumerable<string> GetNotFoundPartList()
        {
            if (metaData.IsEmpty) yield return MetaData.Type;
            if (alphabetData.IsEmpty) yield return AlphabetData.Type;
            if (wordListData.IsEmpty) yield return WordListData.Type;
            if (todoListData.IsEmpty) yield return TodoListData.Type;
        }

        #endregion LocalFunction
    }
}

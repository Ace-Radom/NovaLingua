using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NovaLingua.Lib.Exceptions;

namespace NovaLingua.Lib.Data;

public static class LangDataReader
{
    public static LangData ReadFromDisk(string zipFilePath)
    {
        var data = new LangData();

        if (Path.GetExtension(zipFilePath) != ".nld")
        {
            throw new FileException(FileErrorCode.UnexpectedExt, zipFilePath, ".nld");
        }

        using var zipFileStream = SafeOpenReadFileStream(zipFilePath);
        using var zipArchive = SafeUnarchiveZipFileStream(zipFileStream, zipFilePath);

        foreach (var entry in zipArchive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            string fileName = entry.Name.ToLower();
            string fileExt = Path.GetExtension(fileName);

            using var entryStream = SafeOpenZipArchiveEntryStream(entry, zipFilePath);
            if (fileExt == ".json")
            {
                using var reader = new StreamReader(entryStream);
                string jsonData = SafeReadStreamReaderToEnd(reader, $"{zipFilePath}/{fileName}", "Json inside Zip Archive");
                if (fileName == "alphabet.json")
                {
                    var alphabetData = SafeDeserializeJsonAlphabetData(jsonData);
                    // TODO: check alphabet data
                }
            }
        }

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

        static ZipArchive SafeUnarchiveZipFileStream(FileStream zipFileStream, string zipFilePath)
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

        static DataStructures.AlphabetData SafeDeserializeJsonAlphabetData(string jsonData)
        {
            try
            {
                var option = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var alphabetData = JsonSerializer.Deserialize<DataStructures.AlphabetData>(jsonData, option);
                return alphabetData ?? DataStructures.AlphabetData.Empty;
            }
            catch (Exception ex)
            {
                throw ex switch
                {
                    JsonException => new LangDataException(LangDataErrorCode.InvalidFileFormat, "Alphabet"),
                    _ => new LangDataException(LangDataErrorCode.Others, ex.Message)
                };
            }
        }

        #endregion LocalFunction
    }
}

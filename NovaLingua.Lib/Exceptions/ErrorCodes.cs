namespace NovaLingua.Lib.Exceptions;

public enum FileErrorCode
{
    NoError = 0,
    NotFound = 1001,
    AccessDenied = 1002,
    IOError = 1003,              // extraArgs: `ex.Message`
    InvalidPath = 1004,
    UnexpectedExt = 1005,        // extraArgs: `expectedExt`
    InvalidFileFormat = 1006,    // extraArgs: `validFileFormatName`
    Others = 1999                // extraArgs: `ex.Message`
}

public enum LangDataErrorCode
{
    NoError = 0,
    InvalidFileFormat = 2001,    // extraArgs: `invalidPartName`
    Others = 2999                // extraArgs: `ex.Message`
}

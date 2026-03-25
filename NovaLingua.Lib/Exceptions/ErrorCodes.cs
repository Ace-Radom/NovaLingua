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
    Others = 1998,               // extraArgs: `ex.Message`
    Unexpected = 1999            // extraArgs: `msg`
}

public enum LangDataErrorCode
{
    NoError = 0,
    InvalidPartFormat = 2001,    // extraArgs: `ex.Message`
    RequiredPartNotFound = 2002,
    InvalidValue = 2003,         // extraArgs: `fieldName`, `value`
    IdCollision = 2004,          // extraArgs: `fieldName`, `idValue`
    DataCollision = 2005,        // extraArgs: `fieldName`, `value`
    Others = 2998,               // extraArgs: `ex.Message`
    Unexpected = 2999            // extraArgs: `msg`
}

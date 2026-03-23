namespace NovaLingua.Lib.Exceptions;

public class FileException : AppException
{
    public FileException(FileErrorCode errorCode, string filePath, params string[] extraArgs)
        : base(
            (int)errorCode,
            errorCode.ToString(),
            [filePath, ..extraArgs ?? []]
        )
    { }
}

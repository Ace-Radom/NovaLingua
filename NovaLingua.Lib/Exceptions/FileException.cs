namespace NovaLingua.Lib.Exceptions;

public class FileException(FileErrorCode errorCode, string filePath, params string[] extraArgs) : AppException(
        (int)errorCode,
        errorCode.ToString(),
        [filePath, ..extraArgs ?? []]
        )
{
}

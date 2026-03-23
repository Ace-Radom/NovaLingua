namespace NovaLingua.Lib.Exceptions;

public class LangDataException : AppException
{
    public LangDataException(LangDataErrorCode errorCode, params string[] extraArgs)
        : base(
            (int)errorCode,
            errorCode.ToString(),
            extraArgs
        )
    { }
}

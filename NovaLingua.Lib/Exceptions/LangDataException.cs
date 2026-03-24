namespace NovaLingua.Lib.Exceptions;

public class LangDataException(LangDataErrorCode errorCode, string part, params string[] extraArgs) : AppException(
        (int)errorCode,
        errorCode.ToString(),
        [part, ..extraArgs ?? []]
        )
{
}

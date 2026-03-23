using System;

namespace NovaLingua.Lib.Exceptions;

public abstract class AppException(int errorCode, string errorCodeString, string[] errorArgs) : Exception
{
    public int ErrorCode { get; } = errorCode;

    public string ErrorCodeString { get; } = errorCodeString;

    public string[] ErrorArgs { get; } = errorArgs;
    
    public override string Message => $"{this.GetType().Name}: {ErrorCodeString}";

    // public string TranslatedMessage => ;
}

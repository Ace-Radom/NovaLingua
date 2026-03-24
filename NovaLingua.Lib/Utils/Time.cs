using System;

namespace NovaLingua.Lib.Utils;

public static class Time
{
    public static long GetUtcNowTs() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

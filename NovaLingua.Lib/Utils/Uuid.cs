using System;

namespace NovaLingua.Lib.Utils;

public static class Uuid
{
    public static string Generate() => Guid.NewGuid().ToString();
    public static string GenerateN() => Guid.NewGuid().ToString("N");
}

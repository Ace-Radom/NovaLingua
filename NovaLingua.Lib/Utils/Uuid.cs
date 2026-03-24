using System;

namespace NovaLingua.Lib.Utils;

public static class Uuid
{
    public static string Generate()
    {
        return Guid.NewGuid().ToString();
    }
}

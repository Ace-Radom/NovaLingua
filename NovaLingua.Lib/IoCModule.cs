using Autofac;
using NovaLingua.Lib.Data;
using NovaLingua.Lib.Extensions;

namespace NovaLingua.Lib;

public class IoCModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<LangDataReader>();

        return;
    }
}

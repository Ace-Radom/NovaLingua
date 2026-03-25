namespace NovaLingua.Lib.Data.DataStructures;

public class LangDataConfig
{
    public bool AutoGenerationUseVariants { get; set; }
    public bool ForceLetterVariantGlobalUnique { get; set; }
    public bool ForceWordUnique { get; set; }
    public bool ForceWordInflectionGlobalUnique { get; set; }
    public bool ForceWordDefinitionUnique { get; set; }
    public bool WordCaseInsensitive { get; set; }
}

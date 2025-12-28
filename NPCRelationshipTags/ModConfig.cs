using NPCRelationshipTags.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace NPCRelationshipTags;

public sealed class ModConfig
{
    public KeybindList EditTagKey { get; set; } = new(SButton.RightShift);
    public bool EnableDefaultTags { get; set; } = true;

    internal void Register(IModHelper helper, IManifest modManifest, IGenericModConfigMenuApi gmcm)
    {
        gmcm.Register(modManifest, Reset, () => helper.WriteConfig(this));
        gmcm.AddKeybindList(
            modManifest,
            () => EditTagKey,
            (value) => EditTagKey = value,
            I18n.Config_EditTagKey_Name,
            I18n.Config_EditTagKey_Desc
        );
        gmcm.AddBoolOption(
            modManifest,
            () => EnableDefaultTags,
            (value) => EnableDefaultTags = value,
            I18n.Config_EnableDefaultTags_Name,
            I18n.Config_EnableDefaultTags_Desc
        );
    }

    private void Reset()
    {
        EditTagKey = new(SButton.RightShift);
        EnableDefaultTags = true;
    }
}

using NPCRelationshipTags.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace NPCRelationshipTags;

public sealed class ModConfig
{
    public KeybindList EditTagKey { get; set; } = new(SButton.RightShift);

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
        gmcm.AddKeybindList(
            modManifest,
            () => EditTagKey,
            (value) => EditTagKey = value,
            I18n.Config_EditTagKey_Name,
            I18n.Config_EditTagKey_Desc
        );
    }

    private void Reset()
    {
        EditTagKey = new(SButton.N);
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace NPCRelationshipTags;

using TagStore = Dictionary<string, string>;

internal static class TagManager
{
    internal const string TAG_DATA = "tag_data";
    internal const string DEFAULT_TAG_ASSET = $"{ModEntry.ModId}\\DefaultTags";
    internal static TagStore tagDataStore = null!;
    internal static IDataHelper dataHelper = null!;
    internal static FieldInfo? profileMenuStatusField = null!;

    internal static void Register(IModHelper helper)
    {
        dataHelper = helper.Data;
        tagDataStore = dataHelper.ReadGlobalData<TagStore>(TAG_DATA) ?? [];
        helper.Events.Content.LocaleChanged += RemeasureW;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        helper.Events.Content.AssetRequested += OnAssetRequested;

        helper.ConsoleCommands.Add("npc-tag-set", "Set a tag for an NPC", ConsolSetNPCTag);
        helper.ConsoleCommands.Add("npc-tag-clear", "Remove all NPC tags", ConsleRemoveAllNPCTag);

        profileMenuStatusField = AccessTools.DeclaredField(typeof(ProfileMenu), "_status");

        TokenParser.RegisterParser("Mod_NPCTag", TS_NPCTag);
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(DEFAULT_TAG_ASSET))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }

    private static bool TS_NPCTag(string[] query, out string replacement, Random random, Farmer player)
    {
        replacement = "NPCTag";
        if (
            !ArgUtility.TryGet(query, 1, out string npcId, out string error)
            || !ArgUtility.TryGetOptional(query, 2, out string fallback, out error, defaultValue: replacement)
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }
        replacement = TryGetTag(npcId, out string? tagStr) ? tagStr : fallback;
        return true;
    }

    private static void ConsolSetNPCTag(string cmd, string[] args)
    {
        if (
            !ArgUtility.TryGet(args, 0, out string npcId, out string error)
            || !ArgUtility.TryGet(args, 1, out string tag, out error)
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return;
        }
        tagDataStore[npcId] = tag;
        dataHelper.WriteGlobalData(TAG_DATA, tagDataStore);
        ModEntry.Log($"Set tag to '{tag}' for NPC '{npcId}'", LogLevel.Info);
    }

    private static void ConsleRemoveAllNPCTag(string cmd, string[] args)
    {
        tagDataStore.Clear();
        dataHelper.WriteGlobalData(TAG_DATA, tagDataStore);
        ModEntry.Log("Cleared all tag data", LogLevel.Info);
    }

    internal static void RemeasureW(object? sender, EventArgs e)
    {
        smallFontY = Game1.smallFont.MeasureString("W").Y;
    }

    private static void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ProfileMenu profileMenu || profileMenu.GetChildMenu() is NamingMenu)
            return;
        if (!ModEntry.config.EditTagKey.JustPressed())
            return;
        if (
            profileMenu.Current == null
            || profileMenu.Current.InternalName == null
            || !profileMenu.Current.IsMet
            || profileMenu.Current.IsPlayer
        )
            return;
        EditRelationshipTag(
            profileMenu,
            profileMenu.Current.InternalName,
            (s) =>
            {
                if (string.IsNullOrEmpty(s))
                {
                    profileMenuStatusField?.SetValue(profileMenu, "");
                }
                else
                {
                    profileMenuStatusField?.SetValue(profileMenu, Utility.capitalizeFirstLetter(s));
                }
                profileMenu.SetupLayout();
            }
        );
    }

    internal static void EditRelationshipTag(
        IClickableMenu priorMenu,
        string internalName,
        NamingMenu.doneNamingBehavior? postDoneNaming = null
    )
    {
        if (!TryGetTag(internalName, out string? tagStr))
            tagStr = "";
        NamingMenu tagSetMenu = new(null, I18n.Label_EditTag(), tagStr);
        NamingMenu.doneNamingBehavior doneNaming = (s) => UpdateTag(priorMenu, tagSetMenu, internalName, s);
        if (postDoneNaming != null)
        {
            doneNaming = (NamingMenu.doneNamingBehavior)Delegate.Combine(doneNaming, postDoneNaming);
        }
        tagSetMenu.doneNaming = doneNaming;
        tagSetMenu.minLength = 0;
        priorMenu.SetChildMenu(tagSetMenu);
    }

    private static void UpdateTag(IClickableMenu profileMenu, NamingMenu tagSetMenu, string npcId, string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            tagDataStore.Remove(npcId);
        }
        else
        {
            tagDataStore[npcId] = s;
        }
        dataHelper.WriteGlobalData(TAG_DATA, tagDataStore);
        tagSetMenu.exitThisMenu();
    }

    internal static bool TryGetTag(
        string internalName,
        [NotNullWhen(true)] out string? tagStr,
        bool tryDefaultTag = true
    )
    {
        if (!tagDataStore.TryGetValue(internalName, out tagStr))
        {
            if (
                tryDefaultTag
                && ModEntry.config.EnableDefaultTags
                && Game1.content.LoadStringReturnNullIfNotFound(string.Concat(DEFAULT_TAG_ASSET, ':', internalName))
                    is string defaultTag
            )
            {
                tagStr = defaultTag;
            }
            else
            {
                return false;
            }
        }
        return !string.IsNullOrEmpty(tagStr);
    }

    private static float smallFontY;

    internal static bool SocialPage_drawNPCTag(
        SocialPage socialPage,
        SpriteBatch b,
        int i,
        SocialPage.SocialEntry entry
    )
    {
        if (!entry.IsMet)
            return true;
        if (!TryGetTag(entry.InternalName, out string? tagStr))
            return true;

        int maxWidth = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
        string tag = Game1.parseText(I18n.Parentheses(value: tagStr), Game1.smallFont, maxWidth);
        Vector2 tagSize = Game1.smallFont.MeasureString(tag);
        Vector2 tagPos = new(
            socialPage.xPositionOnScreen + 192 + 8 - tagSize.X / 2f,
            socialPage.sprites[i].bounds.Bottom - (tagSize.Y - smallFontY)
        );
        b.DrawString(Game1.smallFont, tag, tagPos, Game1.textColor);
        return false;
    }

    internal static void AnimalPage_drawNPCTag(
        AnimalPage animalPage,
        SpriteBatch b,
        int i,
        AnimalPage.AnimalEntry entry
    )
    {
        if (!TryGetTag(entry.InternalName, out string? tagStr, false))
            return;

        int maxWidth = IClickableMenu.borderWidth * 3 / 2 + 192 - 20 + 96;
        string tag = Game1.parseText(I18n.Parentheses(value: tagStr), Game1.smallFont, maxWidth);
        Vector2 tagSize = Game1.smallFont.MeasureString(tag);
        float num =
            (
                LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru
                || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko
            )
                ? ((0f - smallFontY) / 2f)
                : 0f;
        int num2 = (entry.TextureSourceRect.Height <= 16) ? (-40) : 8;
        Vector2 tagPos = new(
            animalPage.xPositionOnScreen + maxWidth - tagSize.X / 2f,
            animalPage.sprites[i].bounds.Y + 48 + num2 + num - 20f - smallFontY + 8
        );
        b.DrawString(Game1.smallFont, tag, tagPos, Game1.textColor);
    }
}

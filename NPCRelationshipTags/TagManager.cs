using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace NPCRelationshipTags;

using TagStore = Dictionary<string, string>;

internal static class TagManager
{
    internal const string TAG_DATA = "tag_data";
    internal static TagStore tagDataStore = null!;
    internal static IDataHelper dataHelper = null!;
    internal static FieldInfo? profileMenuStatusField = null!;

    internal static void Register(IModHelper helper)
    {
        dataHelper = helper.Data;
        tagDataStore = dataHelper.ReadGlobalData<TagStore>(TAG_DATA) ?? [];
        helper.Events.GameLoop.GameLaunched += RemeasureW;
        helper.Events.Content.LocaleChanged += RemeasureW;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;

        helper.ConsoleCommands.Add("npc-tag-add", "Add npc tag", ConsoleAddNPCTag);
        helper.ConsoleCommands.Add("npc-tag-clear", "Remove all npc tag", ConsleRemoveAllNPCTag);

        profileMenuStatusField = AccessTools.DeclaredField(typeof(ProfileMenu), "_status");
    }

    private static void ConsoleAddNPCTag(string cmd, string[] args)
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
    }

    private static void ConsleRemoveAllNPCTag(string cmd, string[] args)
    {
        tagDataStore.Clear();
        dataHelper.WriteGlobalData(TAG_DATA, tagDataStore);
        ModEntry.Log("Cleared all tag data", LogLevel.Info);
    }

    private static void RemeasureW(object? sender, EventArgs e)
    {
        smallFontY = Game1.smallFont.MeasureString("W").Y;
    }

    private static void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ProfileMenu profileMenu || profileMenu.GetChildMenu() is NamingMenu)
            return;
        if (!ModEntry.config.EditTagKey.JustPressed())
            return;
        if (!TryGetTag(profileMenu.Current.InternalName, out string? tagStr))
            tagStr = "";
        NamingMenu tagSetMenu = new(null, I18n.Label_SetTag(), tagStr);
        tagSetMenu.doneNaming = (s) => UpdateTag(profileMenu, tagSetMenu, profileMenu.Current.InternalName, s);
        tagSetMenu.minLength = 0;
        profileMenu.SetChildMenu(tagSetMenu);
    }

    private static void UpdateTag(ProfileMenu profileMenu, NamingMenu tagSetMenu, string npcId, string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            tagDataStore.Remove(npcId);
            profileMenuStatusField?.SetValue(profileMenu, "");
            profileMenu.SetupLayout();
        }
        else
        {
            tagDataStore[npcId] = s;
            profileMenuStatusField?.SetValue(profileMenu, Utility.capitalizeFirstLetter(s));
            profileMenu.SetupLayout();
        }
        dataHelper.WriteGlobalData(TAG_DATA, tagDataStore);
        tagSetMenu.exitThisMenu();
    }

    internal static bool TryGetTag(string internalName, out string? tagStr)
    {
        return tagDataStore.TryGetValue(internalName, out tagStr);
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
}

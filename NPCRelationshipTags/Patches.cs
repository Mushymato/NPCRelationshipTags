using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace NPCRelationshipTags;

internal static class Patches
{
    internal static void Register()
    {
        Harmony harmony = new(ModEntry.ModId);
        try
        {
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(SocialPage), nameof(SocialPage.drawNPCSlot)),
                transpiler: new HarmonyMethod(typeof(Patches), nameof(SocialPage_draw_drawNPCSlot))
            );
            harmony.Patch(
                original: AccessTools.DeclaredMethod(typeof(ProfileMenu), "_SetCharacter"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(ProfileMenu_SetCharacter_Postfix))
            );
        }
        catch (Exception err)
        {
            ModEntry.Log($"Failed to patch:\n{err}", LogLevel.Error);
        }
    }

    private static void ProfileMenu_SetCharacter_Postfix(
        ProfileMenu __instance,
        SocialPage.SocialEntry entry,
        ref string ____status
    )
    {
        if (TagManager.TryGetTag(entry.InternalName, out string? tagStr))
        {
            ____status = Utility.capitalizeFirstLetter(tagStr);
            __instance.SetupLayout();
        }
    }

    private static CodeInstruction StlocToLdloc(CodeInstruction stloc)
    {
        if (stloc.opcode == OpCodes.Stloc_S)
        {
            return new(OpCodes.Ldloc_S, stloc.operand);
        }
        else if (stloc.opcode == OpCodes.Stloc)
        {
            return new(OpCodes.Ldloc, stloc.operand);
        }
        if (stloc.opcode == OpCodes.Stloc_0)
        {
            return new(OpCodes.Ldloc_0);
        }
        else if (stloc.opcode == OpCodes.Stloc_1)
        {
            return new(OpCodes.Ldloc_1);
        }
        else if (stloc.opcode == OpCodes.Stloc_2)
        {
            return new(OpCodes.Ldloc_2);
        }
        else if (stloc.opcode == OpCodes.Stloc_3)
        {
            return new(OpCodes.Ldloc_3);
        }
        return new(OpCodes.Nop);
    }

    private static IEnumerable<CodeInstruction> SocialPage_draw_drawNPCSlot(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        try
        {
            CodeMatcher matcher = new(instructions, generator);
            // IL_0000: ldarg.0
            // IL_0001: ldarg.2
            // IL_0002: call instance class StardewValley.Menus.SocialPage/SocialEntry StardewValley.Menus.SocialPage::GetSocialEntry(int32)
            // IL_0007: stloc.0
            // IL_0008: ldloc.0
            matcher
                .MatchEndForward([
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_2),
                    new(
                        OpCodes.Call,
                        AccessTools.DeclaredMethod(typeof(SocialPage), nameof(SocialPage.GetSocialEntry))
                    ),
                    new(inst => inst.IsStloc()),
                    new(inst => inst.IsLdloc()),
                ])
                .ThrowIfNotMatch("Failed to match 'SocialEntry socialEntry = GetSocialEntry(i);'");
            CodeInstruction socialEntryLdloc = matcher.Instruction.Clone();
            CodeMatch socialEntryLdlocMatch = new(socialEntryLdloc.opcode, socialEntryLdloc.operand);

            // IL_00ce: ldloc.0
            // IL_00cf: ldfld bool StardewValley.Menus.SocialPage/SocialEntry::IsDatable
            // IL_00d4: stloc.3
            matcher
                .MatchEndForward([
                    socialEntryLdlocMatch,
                    new(
                        OpCodes.Ldfld,
                        AccessTools.DeclaredField(
                            typeof(SocialPage.SocialEntry),
                            nameof(SocialPage.SocialEntry.IsDatable)
                        )
                    ),
                    new(inst => inst.IsStloc()),
                ])
                .ThrowIfNotMatch("Failed to match 'bool isDatable = socialEntry.IsDatable;'");
            CodeInstruction isDatableLdloc = StlocToLdloc(matcher.Instruction);

            // IL_00e5: ldloc.0
            // IL_00e6: callvirt instance bool StardewValley.Menus.SocialPage/SocialEntry::IsRoommateForCurrentPlayer()
            // IL_00eb: stloc.s 6
            matcher
                .MatchEndForward([
                    socialEntryLdlocMatch,
                    new(
                        OpCodes.Callvirt,
                        AccessTools.DeclaredMethod(
                            typeof(SocialPage.SocialEntry),
                            nameof(SocialPage.SocialEntry.IsRoommateForCurrentPlayer)
                        )
                    ),
                    new(inst => inst.IsStloc()),
                ])
                .ThrowIfNotMatch("Failed to match 'bool flag3 = socialEntry.IsRoommateForCurrentPlayer();'");
            CodeInstruction isRoomateLdloc = StlocToLdloc(matcher.Instruction);

            // IL_01d1: ldloc.3
            // IL_01d2: ldloc.s 6
            // IL_01d4: or
            // IL_01d5: brfalse IL_0389
            matcher
                .MatchStartForward([
                    new(isDatableLdloc.opcode, isDatableLdloc.operand),
                    new(isRoomateLdloc.opcode, isRoomateLdloc.operand),
                    new(OpCodes.Or),
                    new(OpCodes.Brfalse),
                ])
                .ThrowIfNotMatch("Failed to match 'if (isDatable || flag3)'");
            Label postDrawJump = (Label)matcher.InstructionAt(3).operand;

            matcher.InsertAndAdvance([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldarg_2),
                socialEntryLdloc,
                new(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(TagManager), nameof(TagManager.SocialPage_drawNPCTag))
                ),
                new(OpCodes.Brfalse, postDrawJump),
            ]);

            return matcher.Instructions();
        }
        catch (Exception err)
        {
            ModEntry.Log($"Error in ConnectedTextures::SocialPage_draw_drawNPCSlot:\n{err}", LogLevel.Error);
            return instructions;
        }
    }
}

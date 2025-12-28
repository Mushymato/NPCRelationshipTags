# NPCRelationshipTags

Allows players to add relationship tags to NPCs, this replaces the normal relationship tags like `(single)`, `(dating)` etc. At the moment, you cannot tag the player (i.e. yourself).

There are 2 ways to do this:
1. Open the social page, then hold `RightShift` and click on the desired NPC.
2. In the profile page of a NPC, press `RightShift`.

The key used is configured via **EditTagKey**.

The tags are stored in global app data instead of config, if you need to reset the tags, use console command `npc-tag-clear`.

## For Mod Authors

You can reference the tag set by the player in various places such as dialogue using the tokenizable string `[Mod_NPCTag <npcId> <fallbackValue>]`.

For example:
- Original string: `Hey we are [Mod_NPCTag Lewis buddies] right?`
- Resolved, no tag set: `Hey we are buddies right?`
- Resolved, Lewis has tag "pals": `Hey we are pals right?`

## Installation

1. Download and install SMAPI.
3. Download this mod and extract to the Mods folder.

## Translations 

* English
* 简体中文


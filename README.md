# NPCRelationshipTags

Allows players to add relationship tags to NPCs, this replaces the normal relationship tags like `(single)`, `(dating)` etc.
There are 2 ways to do this:
1. Open the social page on the game pause menu, then hold `RightShift` and click on the NPC to add or edit their tag.
2. In the profile page of a NPC, press `RightShift` to add or edit their tag.

You can also add tags to farm animals by going to the animals page on the game pause menu, then hold `RightShift` and click on the desired animal.

The key used in all cases can be configured via **EditTagKey**.

At the moment, you cannot tag the player (i.e. yourself).

The tags are stored in global app data instead of config, if you need to reset the tags, use console command `npc-tag-clear`.

All in all these tags are cosmetic only and have zero gameplay impact.

## For Mod Authors

You can reference the tag set by the player in various places such as dialogue.
To do this, use this tokenizable string:
`[Mod_NPCTag <npcId> <fallbackValue>]`

For example:
- Original string: `Hey we are [Mod_NPCTag Lewis buddies] right?`
- Resolved, no tag set: `Hey we are buddies right?`
- Resolved, Lewis has tag "pals": `Hey we are pals right?`

You can provide default tags to show when the player hasn't set a tag yet, see [example here](./DefaultTagExample/content.json). These tags are also valid for the tokenizable string.

## Installation

1. Download and install SMAPI.
3. Download this mod and extract to the Mods folder.

## Translations 

* English
* 简体中文


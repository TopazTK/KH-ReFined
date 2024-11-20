<div align="center">

# Kingdom Hearts - Re:Fined
![License](https://img.shields.io/badge/License-MIT-blue)
![Version](https://img.shields.io/badge/Current_Version-v1.15-dark_lime)
[![Discord](https://img.shields.io/discord/978975211481354251?logo=Discord&logoColor=%23f0f0f0&label=Discord%20Server&color=6A7EC2)](https://discord.gg/kh-refined)
[![Patreon](https://img.shields.io/endpoint?url=https%3A%2F%2Fshieldsio-Patreon.vercel.app%2Fapi%2F%3Fusername%3Dtopaztk%26type%3DPatrons)](https://www.patreon.com/topaztk)  

![KHI](https://img.shields.io/badge/Kingdom_Hearts_I-Not_Live!-red)
![KHII](https://img.shields.io/badge/Kingdom_Hearts_II-Live!-dark_lime)
![KHBBS](https://img.shields.io/badge/Birth_by_Sleep-Not_Live!-red)
![KHDDD](https://img.shields.io/badge/Dream_Drop_Distance-Not_Live!-red)

**Kingdom Hearts - Re:Fined** is a mod that aims to improve upon the Kingdom Hearts experience by introducing Quality of Life improvements, as well as fixing many of the glaring problems of this port.</div>
</div>

> [!WARNING]  
> Every game within the mod library is currently being rewritten. Releases may take time as a result. Thank you for understanding!

## The Premise

The project has two modes of operation within every game, which alters their featureset.  
- **Re:Freshed Mode** [known internally as Lite Mode] will disable most QoL improvements, and will mainly only leave the bug fixes.
- **Re:Fined Mode** [known internally as Full Mode] will enable all features. This makes the mod illegal in speedrunning.

The project is currently compatible with the worldwide releases on **Epic Games Store** and **Steam**.  
Compatibility for the Japanese releases is underway.

> [!IMPORTANT]  
> The games utilize OpenKH in order to add in new features and fix some bugs. Please refer to the Wiki for installation guides and general how-to stuff.
> If you need extensive help, <ins>please do not make an issue</ins>, but rather, join the Discord and make a ticket!

## The Featureset

Re:Fined is aimed at improving upon every game in the 1.5 + 2.5 collection that it can, so the featureset for all of them will differ.  
However, there are also features that are common across every game, which you can see below.

### All Games

| Error/Bug Fix | Enhancement | Description |
|---------------|-------------|-------------|
| :heavy_check_mark: | - | Disable cutscene framelimiter. [[1]](#reference) |
| :heavy_check_mark: | - | The Mouse Cursor will now follow a VirtualBox-style capture mechanism. |
| :heavy_check_mark: | - | The game will no longer infinitely freeze while shutting down. |
| :heavy_check_mark: | - | Removed Save File Restrictions. [[2]](#reference) |
| :heavy_check_mark: | - | Fixed broken and/or missing sound effects and quotes. [[3]](#reference) |
| - | :heavy_check_mark: | Voice channels will now be muted when set to "1" in the PC settings menu. |
| - | :heavy_check_mark: | Added in the option to force controller prompts and camera at will. |
| - | :heavy_check_mark: | Autosaving functionality, utilizing Slot #99. |
| - | :heavy_check_mark: | An RPC Engine is now present for use with Discord. |
| - | :heavy_check_mark: | Soft Reset implementation [R2 + L2, Customizable]. |
| - | :heavy_check_mark: | Ultrawide Support, aiming to improve the experience on 32:9, 21:9, and 16:10 aspects. |
| - | :heavy_check_mark: | Create a game-specific configuration file named `config_xxx.dat` so that changes only have effects on a per-game basis. |

### Kingdom Hearts II

| Error/Bug Fix | Enhancement | Description |
|---------|-------------|-------------|
| :heavy_check_mark: | - | Correct Limit Form's Limit names so that they are more accurately translated. [[4]](#reference) |
| :heavy_check_mark: | - | Restore the ability to exit the game from the title screen. |
| :heavy_check_mark: | - | The Credits FMV can now be skipped. |
| :heavy_check_mark: | - | The "Right Stick" configuration option has been restored. |
| - | :heavy_check_mark: | Removed the FMV on the Title Screen. |
| - | :heavy_check_mark: | The player can now retry all forced encounters. [[5]](#reference) |
| - | :heavy_check_mark: | Added the ability to "Prepare and Retry". [[6]](#reference) |
| - | :heavy_check_mark: | Added the Encounter Plus ability to the game. [[7]](#reference) |
| - | :heavy_check_mark: | Added a warp point to Garden of Assemblage. |
| - | :heavy_check_mark: | Sora's Crown will now also appear in the HUD Image according to the crown he has. |
| - | :heavy_check_mark: | Multi-Audio capabilities, allowing one to switch from and to any of the voice over packs installed. [[8]](#reference) |
| - | :heavy_check_mark: | The ability to switch between Classic and Final Mix Heartless, if the pack is installed. |
| - | :heavy_check_mark: | The ability to switch between Classic and Remastered OST, if the pack is installed. |
| - | :heavy_check_mark: | Atlantica Musical segments can now be unpaused. |
| - | :heavy_check_mark: | Include an option upon selecting `New Game` to skip Roxas' introduction sequence. |
| - | :heavy_check_mark: | The player is now able to re-organize the Magic menu to their liking. This is done on a per-save basis. [[9]](#reference) |
| - | :heavy_check_mark: | Drive Forms will now appear in the inventory. |
| - | :heavy_check_mark: | Drive Forms can now be placed in the Shortcut Menu. |
| - | :heavy_check_mark: | Added the ability to customize and switch between 3 shortcut configurations on-the-fly. This is done on a per-save basis. [[10]](#reference)|
| - | :heavy_check_mark: | Limit Form's Shortcuts can now be reorganized through the config file. |
| - | :heavy_check_mark: | Added the `Quadratum` Command Menu Setting for those who may want to use it. |

#### Reference

1. Cutscenes will by default lock the game to 30fps. This alleviates that.
2. The Save Data is normally locked to a specific account. Re:Fined removes this restriction.
3. Most sound effects are broken (Like Sephiroth's Deflect and Xigbar's Shots) and some boss quotes are missing (Genie Jafar's Beginning Quote, Saix's Desperation Move). Re:Fined fixes this.
4. In the HD remaster of Kingdom Hearts II, the localization team changed the translation of certain attacks to contrast with their usual translations from Japanese. E.g. Ragnarok -> Infinity. This fix reverts that decision.
5. Starting in Birth by Sleep, the player is given an option to retry a forced encounter instead of being sent to the room before. Re:Fined implements this feature to Kingdom Hearts II.
6. In Kingdom Hearts III, the player is able to press "Prepare and Retry" after dying to enter the loadout menu. Re:Fined implements this feature into Kingdom Hearts II.
7. In Kingdom Hearts, there was a convenient "Encounter Plus" ability to make the grinding experience more bearable. Re:Fined implements this ability into Kingdom Hearts II.
8. CrazyCatz00's English Patch for Kingdom Hearts II - Final Mix on the PS2 had an option in the menu to switch from and to English or Japanese voice acting. This mod reimplements that to the games. There's also extra patches to have up to 5 languages at once. Please see the "Official Addons" section.
9. In Kingdom Hearts III, the player can reorganize their magic menu. This feature is neat, so now it's in Kingdom Hearts II. To do this, highlight your magic command and press L2+DPAD to shift around.
10. In Kingdom Hearts III, the player can have up to 3 shortcut sets and can cycle between them by using the DPAD. This feature is neat, so now it's in Kingdom Hearts II.

## Official Addons

Re:Fined has support for addons that add extra functionality to the game. This is done so that people don't have to download gigabytes of content that they may not use.  
The current official addons for each game can be seen below, and what they do being listed. More addons are planned for the future.

### Kingdom Hearts II

- **KH-ReFined/KH2-VanillaOST** - Adds an option to use the MIDI versions of the soundtrack from the PS2 in Kingdom Hearts II. Can be toggled from the in-game config menu or at the New Game screen.
- **KH-ReFined/KH2-VanillaEnemy** - Adds an option to use the classic Heartless Colors in Kingdom Hearts II. Can be toggled from the in-game config menu or at the New Game screen.
- **KH-ReFined/KH2-JapaneseVO** - Adds an option to use Japanese Voice Over in Kingdom Hearts II. Can be toggled from the in-game config menu or at the New Game screen.
- **KH-ReFined/KH2-SpanishVO** - Adds an option to use Spanish Voice Over in Kingdom Hearts II. Can be toggled from the in-game config menu or at the New Game screen.

> [!NOTE]  
> These addons require the use of OpenKH Mod Manager.

## Community Addons

The addon system allows the end user to make their own addons for Re:Fined to add their own custom sound packs, music packs, etc. A guide on how to create these packs will be made and posted to the Wiki soon!

> [!IMPORTANT]  
> The end user is responsible for all that can go wrong when using community addons.  
> The Re:Fined Team will not accept any responsibility for anything that can happen as a result of community addons.

## Compiling and Contributions

A guide on how to compile and test Re:Fined is currently being made, and the Code of Conduct is currently being written.

## Support

All of Topaz' Kingdom Hearts projects are now his full time gig, and thus a Patreon exists to support him and any of his projects you may like.  
You can use the badge that is present at the start of this ReadME to check the Patreon out. There are benefits like private conversation, early BETA testing, and mentions in releases!  
Thank you for your consideration, all support is appreciated!

## Credits

### All Games
- **Televo** and **deltaemeraldin** for almost every asset Re:Fined uses.
- **Denhonator** for his work on Soft Reset, and Volume Mute in the past.
- **Xeeynamo** for the functions used to calculate the Save Checksum.
- **UtenaSW** and **Ruby-Chan** for the German translation.
- **Svenchu**, **Mikote11**, and **Abelatox** for the Spanish translation.
- **blobdash** for the French translation.
- **iTeoh**, **kuxir**, and **atlas_0x** for the Italian translation.
- **SapphireSaphic** for the upcoming Japanese translation.

### Kingdom Hearts 2
- **CrazyCatz00** for his contributions to Ultrawide Compatibility and Boss Quote Fixes.
- **TieuLink** for providing the Japanese voices for the Multi Audio Add-on.
- **DA** and **AntonioDePau** for their help with injecting ".a.jp" files for the Multi Audio Add-on.
- **Y2K** for his past work on the Prologue Skip.
- **VladABDF** for his work on the Crown Engine as well as the Quadratum Menu.
- **Some1fromthedark** for his contributions to the new Configuration System.
- **gg3502** for his help in the SharpHook Engine.
- **MainMemory** for her help in fixing Panacea related issues.

## Third Party Libraries

Re:Fined makes use of the following third-party libraries for it's functionality:

[Discord-RPC-CSharp by Lachee](https://github.com/Lachee/discord-rpc-csharp)
[MemorySharp by JamesMenetrey](https://github.com/JamesMenetrey/MemorySharp)

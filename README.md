# SkidMenu

A host-side toolkit for running private Among Us lobbies. SkidMenu puts per-player host controls, lobby management, and game-state monitoring in one panel, so a host can run clean, organized custom games without fighting the client. It also bundles a large set of convenience features for private play: notifications, kill aura, votekick, lag compensation, instant vote, save/load player info, spoofing, a full randomizer, chat tooling, and more.

**Created by SNOWHAXX**

**Current build:** `1.2.2` (Stable)

![SkidMenu in lobby](image%20in%20lobby.png)

> This build is labeled Stable. That tho doesnt mean that nothing will break. It just means that no errors occured while playing for long time. Some functions may break mid-game, cause disconnects, or behave like shit. This is normal given how many features are packed in.

## For Hosts and Private Lobbies

SkidMenu is a host tool. It is meant to be run by the host of a private lobby, with friends who know what you're running and are fine with it. It is not a tool for public lobbies against strangers. Keep it in lobbies you host or run, and everyone keeps having a good time.

SkidMenu is not affiliated with, endorsed by, or sponsored by Innersloth in any way. Whatever you choose to do with this menu, including any bans, account issues, or other consequences, is entirely your own responsibility, not the menu's.

## Why Closed Source

Honestly, there's no big reason behind it. Part of it is that I'm using this project to learn a bit of reverse engineering, and keeping it closed source is just part of that. I'll make it open source sooner or later . Spoiler: it's obfuscated with ConfuserEx.

## Compatible Among Us versions

| Status | Versions |
|---|---|
| Supported | `2026.3.31`, `2026.6.5` |
| Tolerated (some features may not work) | `2026.2.24`, `2026.3.17` |

Running a different version than the ones above isn't recommended, you'll get a warning popup in the main menu.

## Installation

1. Install **BepInEx (IL2CPP)** for Among Us, matching your game's architecture: x64 if you own the game on Epic Games or Microsoft Store, x86 if you own it on Steam or Itch.io.
2. Download the latest `SkidMenu.dll` from the releases page.
3. Copy `SkidMenu.dll` into your Among Us `BepInEx/plugins` folder.
4. Launch the game, and press **Delete** in the main menu to open the menu.

## Features

The menu is split into the following tabs and features (this isn't a full list of everything in the menu).

- Movement: NoClip, Invert controls, Lag Compensation, Speed modifier, Current Speed Changer, Teleport (to cursor, to player, to map locations)
- Self: Set color, Snipe Color, Rainbow, Body Type, Immortality, Invisibility, Load Info, Task animations, Ladder cooldown, Unlimited meetings, Chat theme/Font customization
- ESP: See Roles, Ghosts, Phantoms, Player info, Notifications, Votes, Lobby Info, No Shadows, Zoom, Freecam
- Roles: Fake Roles, Fake Alive, Kill Aura/Reach, Tracking, Engineer, Sabotage, Instant Pet, Spam Pet, Break Pet Infinite Protect Range, Shapeshifter
- Players: Player targeted actions and Info, Teleport, Murder, Watch, Shapeshift, Copy Info, Protect, Save Info, Copy Player, Ban, Kick, Teleport to Vent, Frame
- Ship: Doors Menu, Meetings, Body Reports, Fake Tasks, Unlock Vents, Sabotages
- Chat: Chat unlocks, Extended Chat, Chat History, Chat Windows, Copy Message, Chat sender (Manual Spam and Trigger based Messages on Join, Death, Meeting, Kill, and Ejection)
- Animations: Task animations, Skip Shhh Animation, Skip Roles Reveal
- Console: In game event log (kills, vents, phantom vanishes, shapeshifts, sabotages, sabotage fixes, cameras, vitals, binoculars, meetings, ejections, votes, votekicks, chat, disconnects, joins, messages, and more)
- Host: Host only tools (Disable Sabotages, Meetings, Cameras, pre-game role forcing, live role forcing, Disco party, Kill all, Force Start, END Game, Vote Immune, Protect)
- Host Settings: Allows u to see, edit, save and load Host settings from ur lobby and different lobbies
- AutoHost: Automatic Match Starting
- Fun Ban Exploit: A single ban related exploit that can be used to ban SOME players while in-game.
- Vent Kick Exploit: Better version of Fun Ban Exploit
- Schizo/FakeSab: Fake Sabotage Spam (Doors and Reactor)
- Passive: Anti-overload, Unlock features, Free Cosmetics, Penalty Avoidance, Auto Return After Match, Copy code on Disconnect
- Troll: Troll stuff such as Disabling Vents, Door troller, Block Sabotages, Vent Teleporter, Auto Report Bodies and a Delay Setting for it
- Votekick: Votekick Players, Votekick all, Auto Votekick All, Crews, Imps, Host, Finish the Kick
- Protections: client side hardening against malicious hosts and players (DTLS, overload protection, ladder validation, anti exploits)
- Anticheat: RPC validation, Blacklist, known mod detection, punishments, max level
- Spoofing: Spoof Level, Platform, Name, Randomizers
- Lobby Finding: extended lobby list with filters for finding the right kind of lobby, community lobbies.
- Modes: Streamer Mode
- Settings/Config: save and load your profile, menu size, max fps, Disable/Spoof Telemetry, Disable/Spoof Device Id
- Info: About, Disclaimer, Keybinds

## Keybinds

| Key | Action |
|---|---|
| Hold F1 | Close doors in your current room |
| Hold F2 | Close every door on the map |
| Hold F3 | Open every door on the map |
| Hold F6 | Trigger all sabotages at once |
| Hold F7 | Fix all active sabotages |
| Hold 7 | Spam electrical sabotage |
| F4 | Complete your tasks one by one with a small delay |
| F5 | Report a random dead body (in game only) |
| F8 | Votekick everyone in the lobby |
| F9 | Ban everyone in the lobby |
| F10 | Ban all impostors |
| F11 | Ban a random player |
| 0 | Call an emergency meeting (in game only) |
| 9 | Kill a random player (in game only, host or impostor) |
| 8 | Teleport kill a random player (in game only, host or impostor) |

All keybinds can be disabled from the Info tab.

## Saving Your Setup

Use Save to Profile / Load from Profile in the Config tab to preserve your settings across sessions.

## I dont know

We have an official SkidMenu Discord server where you can report bugs, suggest features, or just hang out:

**[Join the Discord](https://discord.gg/zgwTD4FFFx)**

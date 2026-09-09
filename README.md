<div align="center">

# SkidMenu

**Host-side toolkit for running private Among Us lobbies.**

Per-player host controls, lobby management, and game-state monitoring in one panel, so a host can run clean, organized custom games. Bundles a large set of convenience features for private play: radar, ESP, kill aura, votekick, lag compensation, instant vote, skin save/load, spoofing, a full randomizer, chat tooling, customizable keybinds, player history, telekill, rainbow, custom body type, custom chat bubbles, anticheat, HUD size and transparency, and much more.

**Created by SNOWHAXX**

<a href="https://github.com/SNOWHAXX/SkidMenu/"><img src="https://img.shields.io/badge/SkidMenu-GitHub-181717?style=for-the-badge&logo=github" alt="GitHub"></a>
<a href="https://discord.gg/zgwTD4FFFx"><img src="https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge&logo=discord&logoColor=white" alt="Discord"></a>
<a href="https://github.com/SNOWHAXX/SkidMenu/stargazers"><img src="https://img.shields.io/github/stars/SNOWHAXX/SkidMenu?style=for-the-badge&color=yellow" alt="Stars"></a>
<a href="https://github.com/SNOWHAXX/SkidMenu/releases"><img src="https://img.shields.io/badge/Version-1.4.3_Stable-success?style=for-the-badge" alt="Version"></a>

![SkidMenu in lobby](image%20in%20lobby.png)

> This build is labeled Stable. That tho doesnt mean that nothing will break. It just means that no errors occured while playing for long time. Some functions may break mid-game or behave like shit. This is normal given how many features are packed in.

</div>

## Contents

- [For Hosts and Private Lobbies](#for-hosts-and-private-lobbies)
- [Credits](#credits)
- [Compatible Among Us versions](#compatible-among-us-versions)
- [Installation](#installation)
- [Features](#features)
- [Keybinds](#keybinds)
- [Saving Your Setup](#saving-your-setup)
- [Community](#community)

## For Hosts and Private Lobbies

SkidMenu is a host utility tool. It is meant to be run by the host of a private lobby, with friends who know what you're running and are fine with it. It is not a tool for public lobbies against strangers. Keep it in lobbies you host or run, and everyone keeps having a good time.

SkidMenu is not affiliated with, endorsed by, or sponsored by Innersloth in any way. Whatever you choose to do with this menu, including any bans, account issues, or other consequences, is entirely your own responsibility, not the menu's.

## Credits

**SkidMenu is a fork of [HyperMenu](https://github.com/The-HyperMenu-Team/HyperMenu/) ([GitLab mirror](https://gitlab.com/the-hypermenu-team1/hypermenu)). Full credits to its official owner [ADHyperActive](https://github.com/ADHyperActive).**

<p align="center">
  <a href="https://github.com/The-HyperMenu-Team/HyperMenu/"><img src="https://img.shields.io/badge/Based%20on-HyperMenu%20(GitHub)-blue?style=for-the-badge" alt="HyperMenu"></a>
  <a href="https://gitlab.com/the-hypermenu-team1/hypermenu"><img src="https://img.shields.io/badge/Based%20on-HyperMenu%20(GitLab)-blue?style=for-the-badge" alt="HyperMenu GitLab"></a>
  <a href="https://github.com/ADHyperActive"><img src="https://img.shields.io/badge/Owner-ADHyperActive-green?style=for-the-badge" alt="ADHyperActive"></a>
</p>

## Compatible Among Us versions

| Status | Versions |
|---|---|
| Supported | `2026.6.5`, `2026.8.18` |
| Tolerated (some features may not work) | `2026.2.24`, `2026.3.17`, `2026.3.31` |

Running a different version than the ones above isn't recommended, you'll get a warning popup in the main menu.

## Installation

1. Install **BepInEx (IL2CPP)** for Among Us, matching your game's architecture: x64 if you own the game on Epic Games or Microsoft Store, x86 if you own it on Steam or Itch.io.
2. Download the latest `SkidMenu.dll` from the [releases page](https://github.com/SNOWHAXX/SkidMenu/releases).
3. Copy `SkidMenu.dll` into your Among Us `BepInEx/plugins` folder.
4. Launch the game, and press **Delete** in the main menu to open the menu.

## Features

The menu is split into tabs (this isn't a full list of everything in the menu).

### Movement

NoClip, Invert controls, Lag Compensation, Speed modifier, Current Speed Changer, Teleport (to cursor with configurable binds, to player, to map locations, right-click map teleport with its own section and binds).

### Self

Set color, Snipe Color, Rainbow, Body Type, Immortality, Become Immortal keybind, Invisibility, Load Info, Task animations, No Ladder cooldown, No Zipline cooldown, Unlimited meetings, Chat theme/Font customization.

### ESP

See Roles, Ghosts, Phantoms, Player info, Notifications, Votes, Lobby Info, No Shadows, Zoom Out (bindable), Freecam (bindable), Match info (role colour + detected mod names in the player info line), Resize Buttons/HUD (size, height, offsets, opacity, hide toggle, works combined with Zoom Out), Radar (draggable live map window for Skeld, Mira HQ, Polus, Airship, Fungle with player dots in outfit colors, dead bodies, ghosts, crewmate icons with visors, click to teleport with configurable binds, click a room to close its doors, scale/alpha sliders, border, hide in meeting, lock position, show even if menu hidden).

### Roles

Fake Roles, Fake Alive, Kill Aura/Reach (bindable), Tracking, Engineer, Sabotage, Instant Pet, Spam Pet (bindable), Break Pet Infinite Protect Range, Shapeshifter, Judge stuff, No Kill Checks (Kill Other Impostors / Kill While Vanished).

### Players

Player targeted actions and Info, Teleport, Murder, Watch, Shapeshift, Copy Info, Protect, Save Info, Copy Player, Ban, Kick, Judge, Teleport to Vent, Frame, Immortal, Zip Down/Up, Rainbow Target (cycle a player's colour), Glitter Bomb (host-only per-player flicker + colour flash button).

### Ship

Doors Menu (show even if menu hidden), Meetings, Body Reports, Fake Tasks, Unlock Vents, Sabotages, Random Sabotage (bindable), No Map cooldowns (ladders + ziplines).

### Chat

Chat unlocks, Extended Chat, Chat History, Chat Windows (show even if menu hidden), Copy Message, Chat No CD, Chat sender (Manual Spam and Trigger based Messages on Join, Death, Meeting, Kill, and Ejection).

### Animations

Task animations, Skip Shhh Animation, Skip Roles Reveal.

### Console

In game event log (kills, vents, phantom vanishes, shapeshifts, sabotages, sabotage fixes, cameras, vitals, binoculars, meetings, ejections, votes, votekicks, chat, disconnects, joins, messages, and more), show even if menu hidden.

### Host

Host only tools (Disable Sabotages, Meetings, Cameras, pre-game role forcing, live role forcing, Disco party, Kill all, Force Start (bindable), END Game, Vote Immune, Protect), Protect Everyone (bindable), God Mode, God Mode All, Auto-Angel (automatic Guardian Angel protect), Disable Close Doors, Queued Lobby Crash, Protect Menu (show even if menu hidden).

### Fun Ban Exploit

A single ban related exploit that can be used to ban SOME players while in-game.

### Vent Kick Exploit

Better version of Fun Ban Exploit (Vent Kick All/Impostors/Random, all bindable).

### Sabotage

Sabotages, Random Sabotage (bindable), Schizo/FakeSab (Fake Sabotage Spam, Doors and Reactor).

### Host Settings

See, edit, save and load Host settings from ur lobby and different lobbies.

### AutoHost

Automatic Match Starting.

### Passive

Anti-overload, Unlock features, Free Cosmetics, Penalty Avoidance, Auto Return After Match, Copy code on Disconnect.

### Troll

Troll stuff such as Disabling Vents, Door troller, Block Sabotages, Vent Teleporter, Auto Report Bodies (bindable) and a Delay Setting for it, Zipline (per-player Down/Up, Select All, Down All, Up All), Zipline Spam, Glitter Bomb (phantom colour spam), Auto Expose Impostors (teleport witnesses to a vent when an impostor acts), Coloured chat (send with a custom chat colour).

### Votekick

Votekick Players, Votekick all, Auto Votekick All, Crews, Imps, Host, Finish the Kick, Votekick all + rejoin (auto-vote everyone, leave and rejoin the lobby), Auto-Rejoin on Votekick all / Votekick host.

### Protections

Client side hardening against malicious hosts and players (DTLS, overload protection, ladder validation, anti exploits, block forced ziplines, block forced vent teleports, body-report anti-crash).

### Anticheat

Host-side RPC validation and spoof detection across the game surface (murder, shapeshift, vanish, protect, role/task set, votes, overruling votes, settings sync, snap-to, platform use, task completion, body reports, lobby timer, more), Blacklist, known mod detection (BetterAmongUs/GreaterAmongUs, Starlight, custom RPCs), punishments, max level.

### Spoofing

Spoof Level, Platform, Name, Randomizers, Fully Randomize (bindable).

### Lobby Finding

Extended lobby list with filters for finding the right kind of lobby, community lobbies.

### Modes

Streamer Mode.

### Settings

Menu keybind (click-and-press), menu size, max fps, Disable/Spoof Telemetry, Disable/Spoof Device Id.

### Config

Save and load your profile, Clear Notifications button (bindable, sits on top of the keybind list).

### Info

About, Disclaimer, fully customizable action keybinds with per-key capture, keybind notifications.

## Keybinds

Default action keys. Every one of them can be changed from the Info tab (click a key, press a new one, Esc for None). All keybinds can be disabled from the Info tab, and keybind notifications can be toggled there too.

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
| F9 | Vent kick everyone in the lobby |
| F10 | Vent kick all impostors |
| F11 | Vent kick a random player |
| 0 | Call an emergency meeting (in game only) |
| 9 | Kill a random player (in game only, host or impostor) |
| 8 | Teleport kill a random player (in game only, host or impostor) |
| None | Become Invisible |
| None | Lag Compensation |
| None | NoClip |
| None | Kill Aura |
| None | Spam Pet |
| None | Zoom Out |
| None | Rainbow |
| None | Freecam |
| None | No Shadows |
| None | Random Sabotage |
| None | Protect Everyone |
| None | Force Start Game |
| None | Auto Report Bodies |
| None | Fully Randomize |
| None | Become Immortal |
| None | Show Radar |
| None | Show Doors Menu |
| None | Show Chat Window |
| None | Show Console |
| None | Show Protect Menu |
| None | Clear Notifications |

Toggle keybinds for individual menu features can also be set from the Config tab.

## Saving Your Setup

Use Save to Profile / Load from Profile in the Config tab to preserve your settings across sessions. Keybinds, menu key, HUD settings, radar settings, teleport binds, and keybind notification preference are all saved in the profile.

## Community

<div align="center">

<a href="https://discord.gg/zgwTD4FFFx"><img src="https://img.shields.io/badge/Join_the_Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white" alt="Discord"></a>
<a href="https://github.com/SNOWHAXX/SkidMenu/"><img src="https://img.shields.io/badge/Star_the_Repo-yellow?style=for-the-badge&logo=github" alt="Star"></a>

We have an official SkidMenu Discord server where you can report bugs, suggest features, or just hang out.

Pre-release and test builds drop in the Discord first, so join if you want new stuff early. Got a feature idea? Suggest it there, the good ones get built. And if you like the menu, drop a star on the project, more stars means more updates.

</div>

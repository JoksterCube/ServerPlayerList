# Server Player List

## See who’s online and how far they are from you!

Ever wondered how many players are currently online on your server, or how far your friends are from you?  
With **Server Player List**, you get a small, unobtrusive panel showing the total number of online players — and, if they’re sharing their location, their names and distances too.

---

### 🧭 Features

- Displays **number of online players** in a small HUD panel.  
- Shows **player names and distances** (only for players who share their location).  
- Toggle between compact and expanded view using **Right Control + O**.  
  - Compact view: only shows the online player count.  
  - Expanded view: shows names and distances.  
- Right-click a player name in the expanded list to add or remove that player as a favorite. Favorites are highlighted, fill the row limit first, remain visible if they exceed it, and bypass local display filters.
- Optionally hide your own row or players with private positions, filter by maximum distance, and limit visible rows. Toggle the private-position filter with **Right Control + P**; it does not change the online count.
- Show a relative direction arrow for players sharing their position. Choose camera-relative or character-relative directions; `↑` means ahead of the selected reference.
- The panel is **draggable** — click and drag to reposition it (easiest with the mouse cursor visible, e.g. in the ESC menu).  
- The basic player list is **client-side** and does not require server installation. Optional RCON filtering requires server installation.

---

### 📸 Screenshot

![Screenshot](https://raw.githubusercontent.com/JoksterCube/ServerPlayerList/refs/heads/main/Screenshots/screenshot.jpg)

---

## ⚙️ Manual Installation Instructions
*You must have BepInEx installed.*

1. Locate your Valheim game folder.  
2. Extract the contents of the archive into the `BepInEx\plugins` folder.  
3. Launch the game and look for the Server Player List panel.  
4. Adjust settings in `BepInEx\config\JoksterCube.ServerPlayerList.cfg` if needed.

### Configuration Updates

Some updates may change configuration sections or keys. If settings do not carry over or behave as expected after updating, back up and delete `BepInEx\config\JoksterCube.ServerPlayerList.cfg`, then relaunch the game to generate a fresh config. This resets all custom settings.

---

### 🧩 Compatibility

- The basic player list works on any server, even if others do not have the mod installed.
- Optional RCON filtering requires ServerPlayerList on the server and matching versions on connecting clients. ValheimRcon is only needed on the server.

#### RCON Player Filtering

This feature supports [ValheimRcon by Tristan-dvr](https://thunderstore.io/c/valheim/p/Tristan/ValheimRcon/) (`org.tristan.rcon`).

- **Setup:** On the server, enable `Ignore RCON user` under `Player List`. Requires ValheimRcon on the server and matching ServerPlayerList versions on clients.
- **Effect:** Hides one exact match for ValheimRcon's `[3. Chat]` / `Server name` when that player's position is private (`N/A`). Only the name is synced; RCON credentials stay on the server.
- **Note:** A same-name player with a private position may be hidden. Reload ValheimRcon after changing its server name.

#### Ignore List

- **Setup:** Enter comma-separated names under `Player List` → `Ignored users` (for example, `Server, AnotherPlayer`). Empty by default; uses the server's setting when the mod is installed there, otherwise your local setting.
- **Matching:** Exact and case-sensitive; surrounding spaces are ignored. Each name occurrence hides one player from the list and count, preferring `N/A` entries. Applied after RCON filtering; can also hide your own entry.

#### Configuration Reference

The configuration file is `BepInEx\config\JoksterCube.ServerPlayerList.cfg`. Settings marked **Server-synced** are controlled by the server when ServerPlayerList is installed there; all others are local to your client.

| Group | Setting | Default | Description |
| --- | --- | --- | --- |
| General | Lock Configuration | On | When enabled, only server admins can change server-synced settings. **Server-synced.** |
| General | Refresh Delay | 0.25 seconds | Time between player list refreshes. **Server-synced.** |
| Player List | Show Players | On | Show player rows in the expanded panel. |
| Player List | Hide Local Player | On | Hide your own non-favorite row; does not change the online count. |
| Player List | Hide Players with N/A Distance | Off | Hide non-favorite players who are not sharing their position. Does not change the online count. Toggle with **Right Control + P** by default. |
| Player List | Ignore RCON user | Off | Hide one player matching ValheimRcon's configured server chat name when their position is private. Requires the mod on the server. **Server-synced.** |
| Player List | Ignored users | Empty | Comma-separated, exact, case-sensitive names to hide. Each occurrence hides one matching player. **Server-synced when the mod is installed on the server.** |
| Player List | Use Kilometers | On | Display distances of 1,000 m or more in kilometers. |
| Player List | Maximum Player Distance | 0 m | Hide non-favorite players beyond this distance. `0` disables the filter. Valid range: 0–20,000 m. |
| Player List | Maximum Visible Players | 0 | Maximum player rows to display. Favorites fill the limit first and all remain visible if they exceed it. `0` means unlimited; valid range: 0–20. |
| Player List | Favorite Players | Empty | Comma-separated, exact, case-sensitive names. Favorites remain visible while online, bypass local display filters, and remain visible beyond the row limit. Explicit ignored-user and RCON filters still apply. The local player, when shown, remains first. |
| Player List | Show Player Direction | On | Show a direction arrow for players sharing their position. |
| Player List | Direction Reference | Camera | Choose camera-relative or character-relative direction. Camera mode falls back to character facing if the main camera is unavailable. |
| Player List | Local Player Tag | U+16DD symbol | Text shown in place of your distance when your row is visible. Leave empty to hide it. |
| Appearance | Anchor Position | (-239.75, -243.25) | HUD panel offset from the upper-right corner. Drag the panel to change and save it. |
| Appearance | Width | 199.5 | Width of the panel. |
| Appearance | Background Color | RGBA (0, 0, 0, 0.3) | Panel background color. |
| Appearance | Header Text Color | White | Header text color. |
| Appearance | Header Highlight Text Color | RGBA (1, 0.84, 0, 1) | Color of the online player count in the header. |
| Appearance | Header Font Size | 28 | Header text size. |
| Appearance | List Font Size | 22 | Player row text size. |
| Appearance | Favorite Player Name Color | RGBA (1, 0.84, 0, 1) | Name color used to identify favorite players. |
| Appearance | Header Text | Currently online: | Text displayed before the online player count. |
| Inputs | Show List Keyboard shortcut | Right Control + O | Toggle the player list between compact and expanded modes. |
| Inputs | Toggle N/A Distance Filter Shortcut | Right Control + P | Toggle hiding players who are not sharing their position. |

---

### 💬 Feedback & Support

Report issues or suggest improvements via the mod’s Thunderstore page or GitHub repository.

---

**Author:** JoksterCube<br>
**Version:** 1.2.0<br>
**License:** MIT

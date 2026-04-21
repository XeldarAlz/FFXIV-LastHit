<p align="center">
  <img src="LastHitPlugin/images/icon.png" width="220" alt="LastHit icon" />
</p>

<h1 align="center">LastHit (WORK IN PROGRESS)</h1>

<p align="center">
  Fires your PvP Limit Break at a configurable HP threshold.
</p>

---

## What it does

Monitors enemy HP during PvP. When the target's HP drops below the configured threshold, uses your job's PvP Limit Break. Good for classes like Ninja or Machinist.

## Features

- Configurable threshold, expressed as either a percent of max HP or an absolute HP value.
- Optional auto-target: picks the lowest-HP hostile in range when no manual target is set.
- Works for every PvP job.
- Status window: current target, HP / max HP / percent, threshold state, time since last fire.
- Respects the game's action availability and animation lock.

## Install

LastHit is distributed through a custom Dalamud plugin repository.

1. In-game, run `/xlsettings` → **Experimental**.
2. Under **Custom Plugin Repositories**, paste:
   ```
   https://raw.githubusercontent.com/XeldarAlz/FFXIV-LastHit/master/repo.json
   ```
   Tick **Enabled**, click the **+**, then **Save and Close**.
3. Open `/xlplugins` → **All Plugins**, search for **LastHit**, and install.

Updates are delivered automatically whenever a new release is cut.

### Install from source (developers only)

1. Build the solution in `Release`.
2. `/xlsettings` → **Experimental** → add the full path to `LastHitPlugin.dll` under **Dev Plugin Locations**. Build output is at `LastHitPlugin/bin/x64/Release/LastHitPlugin/`.
3. `/xlplugins` → **Dev Tools → Installed Dev Plugins** → enable **LastHit**.

## Commands

| Command | Action |
|---|---|
| `/lasthit` | Toggle the status window |
| `/lasthit config` | Open settings |

## Configuration

- **Enabled** — master switch.
- **Threshold mode** — percent of max HP, or absolute HP value.
- **Threshold value** — slider (percent) or numeric input (absolute).
- **Auto-select lowest-HP enemy** — used when no manual target is set.
- **Auto-select range** — yalms, 5–50.

## License

AGPL-3.0-or-later. See [LICENSE.md](LICENSE.md).

<p align="center">
  <img src="PvpAutoLb/Images/Icon.png" width="220" alt="PVP Auto LB icon" />
</p>

<h1 align="center">PVP Auto LB</h1>

---

## What it does

Monitors enemy HP during PvP. When the target's HP drops below the configured threshold, uses your job's PvP Limit Break. Good for classes like Ninja or Machinist to auto kill enemies.

Jobs whose PvP Limit Break is defensive or support-focused (e.g., Paladin's Phalanx) are flagged in the status window and not auto-fired — the low-HP-enemy gate doesn't apply to them.

## Features

- **Configurable threshold** — percent of max HP, or absolute HP value. Per-job overrides supported.
- **Range- and shape-aware targeting.** Single-target LBs respect the action's actual cast range; circle-around-target LBs (e.g. Sky Shatter) prefer clustered targets to maximize AoE catch; PBAoE LBs fire when any below-threshold enemy is in radius.
- **Shield-aware HP** — threshold checks compare against effective HP (`CurrentHp + ShieldHp`) so the LB doesn't fire prematurely on a shielded target.
- **Skip doomed targets** — estimates each enemy's HP-per-second rate; skips targets that will die before the LB lands.
- **Player blocklist + duty filter** — names listed are never auto-targeted; allowed-duties checkboxes scope auto-fire to specific PvP modes (CC / Frontline / Rival Wings / Custom Match / Other).
- **Auto-target** — picks the lowest-effective-HP hostile in range when on; falls back to your manual hard target when off.
- **Status window** — current target, HP bar with shield overlay and threshold marker, distance, range/shape descriptor, granular readiness states (`READY` / `FIRING` / `PAUSED` / `OUT OF RANGE` / `GAUGE LOW` / `DEFENSIVE`).
- **Session + lifetime stats** — fires, attributed kills, total enemies hit. Lifetime persists across reloads.
- **Optional feedback** — chat sound (`/se1`–`/se16`) and/or chat line on fire.
- Works for every PvP job. Defensive/support LBs are recognized and not auto-fired.
- Respects the game's action availability and animation lock.

## Screenshots

<p align="center">
  <img src="PvpAutoLb/Images/Main.png" alt="PVP Auto LB main window" width="380" />
</p>

Settings walkthrough:

<p align="center">
  <img src="PvpAutoLb/Images/Settings.gif" alt="Settings walkthrough" width="500" />
</p>

## Install

PVP Auto LB is distributed through a custom Dalamud plugin repository.

1. In-game, run `/xlsettings` → **Experimental**.
2. Under **Custom Plugin Repositories**, paste:
   ```
   https://raw.githubusercontent.com/XeldarAlz/FFXIV-PvPAutoLB/master/repo.json
   ```
   Tick **Enabled**, click the **+**, then **Save and Close**.
3. Open `/xlplugins` → **All Plugins**, search for **PVP Auto LB**, and install.

Updates are delivered automatically whenever a new release is cut.

## Commands

| Command | Action |
|---|---|
| `/pvpautolb` | Toggle the status window |
| `/palb` | Alias for `/pvpautolb` |
| `/pvpautolb config` | Open settings |

## Configuration

Open via `/pvpautolb config` or the gear icon in the main window's toolbar.

**Threshold**
- Mode (percent of max / absolute HP) and value. Below this, the LB fires.

**Per-job override**
- Each job can have its own threshold mode + value, overriding the global setting.

**Targeting**
- Auto-select toggle and scan radius (5–50 yalms). When off, only your manual hard target is considered.

**Filters**
- Skip doomed targets (predicted time-to-death < LB animation lock).
- Allowed duty types: Crystalline Conflict, Frontline, Rival Wings, Custom Match, Other PvP.

**Player blocklist**
- Names listed here are never auto-targeted, even when below threshold.

**Feedback**
- Optional chat sound on fire (sound id 1–16, like `/se1` in chat).
- Optional chat line on fire.

**About**
- Repo, issues, discussions, security advisory links. Open via the info-circle icon in the main window's toolbar.

## Job compatibility

PvP Limit Breaks are resolved dynamically from game data, so every job is wired up automatically. Jobs whose LB is defensive or support-focused are flagged in the status window and not auto-fired — the low-HP-enemy gate doesn't apply to them. The table below tracks what has actually been verified in live PvP matches. If you test a job, please open an issue or PR so this list can be updated.

**Legend:** ✅ confirmed working · ❔ not tested yet · 🛡 defensive/support — not auto-fired (out of scope)

### Tanks
| Job | Status | Notes |
|---|---|---|
| Paladin | 🛡 | Phalanx — defensive (party barrier + Stoneskin) |
| Warrior | 🛡 | Primal Scream — defensive |
| Dark Knight | ✅ | Confirmed working in CC |
| Gunbreaker | ✅ | Confirmed working in CC |

### Healers
| Job | Status | Notes |
|---|---|---|
| White Mage | ✅ | Confirmed working in CC |
| Scholar | ❔ | Not tested yet |
| Astrologian | 🛡 | Celestial River — support |
| Sage | 🛡 | Mesotes — support |

### Melee DPS
| Job | Status | Notes |
|---|---|---|
| Monk | ✅ | Confirmed working in CC |
| Dragoon | ✅ | Confirmed working in CC |
| Ninja | ✅ | Confirmed working in CC |
| Samurai | ✅ | Confirmed working in CC |
| Reaper | 🛡 | Tenebrae Lemurum — support |
| Viper | ✅ | Confirmed working in CC |

### Physical Ranged DPS
| Job | Status | Notes |
|---|---|---|
| Bard | 🛡 | Final Fantasia — support |
| Machinist | ✅ | Confirmed working in CC |
| Dancer | 🛡 | Contradance — support |

### Magical Ranged DPS
| Job | Status | Notes |
|---|---|---|
| Black Mage | 🛡 | Soul Resonance — support |
| Summoner | ❔ | Not tested yet |
| Red Mage | ❔ | Not tested yet |
| Pictomancer | 🛡 | Advent of Chocobastion — defensive |

## License

AGPL-3.0-or-later. See [LICENSE.md](LICENSE.md).

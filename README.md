# damage log

A client-side mod that adds a damage log to the HUD to show what you have taken damage from recently.

![damage log sample screenshot](https://github.com/itsschwer/ror2-damage-log/blob/main/xtra/demo.png?raw=true)

## why?

sometimes it can be hard to tell what you are taking damage from.

## controls

`cycleUserKey` *(default: `left alt`)* can be used to cycle between player damage logs in multiplayer.

## versions
> *Check the **changelog** to see version differences!*

- If playing on the *Seekers of the Storm* patch, use the latest version available of this mod
- If playing on a game version ***before*** the *Seekers of the Storm* patch, please use version `1.1.3` of this mod

## configurable

> The configuration file is automatically reloaded when the damage log UI is created *(at the start of each stage)*

- the log only being visible in the scoreboard can be enabled
- how long entries are logged for can be adjusted
- the keys to cycle between player (and boss) damage logs can be adjusted
- the log being presented in simple text mode can be enabled
- *\[ portraits mode \]*
    - the maximum number of portraits to display at a time can be adjusted
- *\[ simple text mode \]*
    - compacting (removing) empty line separators can be enabled
    - hiding the damage timer can be enabled
    - showing raw damage taken instead of percentage can be enabled
- *\[ **bonus** :: experimental \]* generating damage logs for bosses can be enabled

Portraits mode | Simple text mode
--- | ---
![portraits mode damage log sample screenshot](https://github.com/itsschwer/ror2-damage-log/blob/main/xtra/compare-portrait.png?raw=true) | ![simple text mode damage log sample screenshot](https://github.com/itsschwer/ror2-damage-log/blob/main/xtra/compare-text.png?raw=true)

- the size of various damage log UI elements can be adjusted *(under the `m_Debug` section)*

## notes

*please report any issues to the [GitHub repository](https://github.com/itsschwer/ror2-damage-log/issues)!*

- damage inflicted by enemies that have been killed *(debuffs, delayed explosions, projectiles, trails)* may be attributed to *The Planet* as the reference to the original attacker no longer exists
- damage from Pots *(Abandoned Aqueduct)* and Fusion Cells *(Rallypoint Delta)* are attributed to *The Planet*
    - *sometimes attributed correctly on non-hosts?*
    - *why do Sulfur Pods (Sulfur Pools) not have the same issue?*
<!-- -->
- *\[ experimental \]* boss damage logs:
    - bosses don't always have a damage log generated when playing as client?
    - ~~name doesn't capture elite prefix when playing as host?~~ — fixed?
    - occasionally has significant discrepencies between host and client(s?)?
    - sometimes doesn't capture the killing instance of damage?
    - damage log timers continue counting after a boss is killed with Trophy Hunter's Tricorn?

## see also

- [DamageHistory](https://thunderstore.io/package/Bubbet/DamageHistory/) <sup>[*src*](https://github.com/Bubbet/Risk-Of-Rain-Mods/tree/master/DamageHistory)</sup> by [Bubbet](https://thunderstore.io/package/Bubbet/) — alternative, inspired this implementation
    - tracks damage until fully healed(?)
    - text only
    - a bit hard to parse
- [ShowDeathCause](https://thunderstore.io/package/NotTsunami/ShowDeathCause/) <sup>[*src*](https://github.com/NotTsunami/ShowDeathCause)</sup> by [NotTsunami](https://thunderstore.io/package/NotTsunami/) — shows extra information about the attacker that killed you on the game end screen

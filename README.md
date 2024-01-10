# damage log

A client-side mod that adds a damage log to the HUD to show what you have taken damage from recently.

![damage log sample screenshot](https://github.com/itsschwer/ror2-damage-log/blob/main/xtra/demo.png?raw=true)

## why?

sometimes it can be hard to tell what you are taking damage from.

## configurable

> The configuration file is automatically reloaded when the UI is created *(at the start of each stage)*

- how long entries are logged for can be adjusted
- how many entries are logged can be adjusted
- the log only being visible in the scoreboard can be enabled
- the log being presented in simple text mode can be enabled

Portraits mode | Simple text mode
--- | ---
![portraits mode damage log sample screenshot](https://github.com/itsschwer/ror2-damage-log/blob/main/xtra/compare-portrait.png?raw=true) | ![simple text mode damage log sample screenshot](https://github.com/itsschwer/ror2-damage-log/blob/main/xtra/compare-text.png?raw=true)

- the size of various DamageLog UI elements can be adjusted *(under the `m_Debug` section)*

## issues

*please report any issues to the [GitHub repository](https://github.com/itsschwer/ror2-damage-log/issues)!*

- damage inflicted by enemies that have been killed *(debuffs, delayed explosions, projectiles, trails)* may be attributed to *The Planet* as the reference to the original attacker no longer exists
- damage from Pots *(Abandoned Aqueduct)* and Fusion Cells *(Rallypoint Delta)* are attributed to *The Planet*
    - *sometimes attributed correctly on non-hosts?*
    - *why do Sulfur Pods (Sulfur Pools) not have the same issue?*

## see also

- [DamageHistory](https://thunderstore.io/package/Bubbet/DamageHistory/) <sup>[*src*](https://github.com/Bubbet/Risk-Of-Rain-Mods/tree/master/DamageHistory)</sup> by [Bubbet](https://thunderstore.io/package/Bubbet/) — alternative, inspired this implementation
    - tracks damage until fully healed(?)
    - text only
    - a bit hard to parse
- [ShowDeathCause](https://thunderstore.io/package/NotTsunami/ShowDeathCause/) <sup>[*src*](https://github.com/NotTsunami/ShowDeathCause)</sup> by [NotTsunami](https://thunderstore.io/package/NotTsunami/) — shows extra information about the attacker that killed you on the game end screen

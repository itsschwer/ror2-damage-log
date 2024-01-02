# damage log

A client-side mod that adds a damage log to the HUD to show what you have taken damage from recently.

> ***sample image here please!***

## why?

sometimes it can be hard to tell what you are taking damage from.

## configurable

> The configuration file is automatically reloaded when the UI is created *(at the start of each stage)*

> ***tba***

## issues

- Has not yet been thoroughly tested as non-host
- Damage from Pots *(Abandoned Aqueduct)* and Fusion Cells *(Rallypoint Delta)* are attributed to *The Planet*
    - *null attacker in `DamageDealtMessage`*
        - *how does `GameEndReportPanelController` know this attacker??*
        - *why do Sulfur Pods (Sulfur Pools) not have the same issue?*
- Damage inflicted by enemies that have been killed *(debuffs, delayed explosions, projectiles, trails)* may be attributed to *The Planet* as the reference to the original attacker no longer exists

## see also

- [DamageHistory](https://thunderstore.io/package/Bubbet/DamageHistory/) <sup>[*src*](https://github.com/Bubbet/Risk-Of-Rain-Mods/tree/master/DamageHistory)</sup> by [Bubbet](https://thunderstore.io/package/Bubbet/) — alternative, inspired this implementation
    - tracks damage until fully healed(?)
    - text only
    - a bit hard to parse
- [ShowDeathCause](https://thunderstore.io/package/NotTsunami/ShowDeathCause/) <sup>[*src*](https://github.com/NotTsunami/ShowDeathCause)</sup> by [NotTsunami](https://thunderstore.io/package/NotTsunami/) — shows extra information about the attacker that killed you on the game end screen

# damage indicator

## todo
- test
    - as non-host
    - dio respawn
- blood shrine
- void cradle / potential *(voidtouched buff icon? `AffixVoid`)*
- abandoned aqueduct tar pots *&* rallypoint delta fusion cells *(`texMysteryIcon`)*
    - null attacker in `DamageDealtMessage` *— how does `GameEndReportPanelController` know this attacker?*
        - *might not be possible with current implementation / without hooks / as client?*
- void fog icon color?

## reference

- [Bubbet · DamageHistory](https://github.com/Bubbet/Risk-Of-Rain-Mods/tree/master/DamageHistory) *(inspiration, design, Harmony patches)*
- [NotTsunami · ShowDeathCause](https://github.com/NotTsunami/ShowDeathCause) *(finding `RoR2.Util.GetBestBodyName()`, fall/void damage icons)*
- [xoxfaby · BetterUI](https://github.com/xoxfaby/BetterUI) *(tooltips)*

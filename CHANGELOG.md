### 1.2.1
- Build [1.2.0](#120) changes for Seekers of the Storm

# 1.2.0
- Add no break tags to numbers in simple text mode
    - Prevent numbers from being split on to a separate line without its negative sign
- Add new configuration options for simple text mode
    - `compactLines` *(default: `false`, new behaviour)*
    - `hideDamageTimer` *(default: `false`, same behaviour as before)*
    - `showRawDamageInsteadOfPercentage` *(default: `false`, same behaviour as before)*
- Convert hardcoded UI layout values into new configuration options (under the "m_Debug" section)
    - `canvasWidth`
    - `canvasOffsetRight`
        - *Try a value of `1820` (for 1920\*1080 displays) to position the damage log on the left of the HUD*
    - `canvasOffsetTop`
    - *Note: the "m_Debug" section is still primarily a development tool and is not very user-friendly*
- Reorganise configuration options
    - Add two new sections: *Display: Portraits Mode* and *Display: Simple Text Mode*
- Refactor damage source identifier generation
- Adjust how damage values are presented *(negate instead of prepend `-`)*
- Adjust default UI layout values
    - `textSize`: `12` → `14`
    - `portraitSpacing`: `6` → `8`
    - `canvasOffsetRight`: `8` → `12`

### 1.1.7
- Fix boss damage logs on host not having elite prefixes in the name *(tentative)*
- Rework damage log pruning to only be time-based
    - *i.e. damage sources will only be pruned/forgotten once they are older than `entryMaxRetainTime`*
- Rename configuration option `entryMaxCount` (Constraints) to `maximumPortraitCount` (Display)
- Code refactoring

### 1.1.6
- Start assembly versioning
- Update icon to be less blurry *(but more pixelated)*
- Refactor config reloading
- Log error and disable self instead of breaking the game if installed on incompatible version

### 1.1.5
- Restore custom attacker portrait for Artifact Reliquary
- Code refactoring
- Add 'versions' section to README

### 1.1.4
- Hotfix for the Seekers of the Storm patch
    - Fixes damage logs not populating with entries when taking damage
        - *`[Error  : Unity Log] Failed at InvokeHandler, probably malformed packet!`*
    - *Please use the previous version of this mod if playing on an older game version!*

### 1.1.3
- Track bosses by network id instead of instance id
    - *Improve ability to identify shared boss logs between clients, replacing unreliable encounter counting*

### 1.1.2
- Fix error preventing Run History entries from being viewed properly

### 1.1.1
- Add 'controls' section to readme to better indicate the ability to cycle between user damage logs in multiplayer
- Fix potential `ArgumentNullException` spam when the UI is initialised without a user

## 1.1.0
- Add ability to generate damage logs for bosses
    - Must be enabled in configuration file
- Change portrait for void cradles/potentials
    - Now uses the *Survivors of the Void* icon rather than the Voidtouched icon
- Renamed configuration option `textModeFontSize` to `textSize`
- Separate damage source pruning logic from display logic *(make pruning behaviour more consistent)*
- Limit the lifetime of damage logs to that of a run
    - *Prevent potential memory leak where `NetworkUsers` accumulate without being cleared*
- Fix logic errors *(`DamageLog.Cease()`, `DamageLog.Record()`)*
- Code refactoring

# 1.0.0
- Rework how time of death is recorded *(fix timers continuing until body is destroyed on non-hosts)*
- Change simple text mode damage color to be consistent with portraits mode

### 0.2.4
- Fix portraits mode displaying white squares when `DamageLogUI` is initialised without a `DamageLog`
- Fix issue where `DamageLog`s would fail to be created when playing as non-host
    - *Sometimes `NetworkUser` and `CharacterBody` would not yet be linked together(?), despite both existing*

### 0.2.3
- Fix Voidtouched elite icon color
- Fix portraits not updating on GameEnd screen *(not showing the correct damage log in multiplayer)*

### 0.2.2
- Add configuration option `showDamageIdentifier`

### 0.2.1
- Thunderstore release
- Finish 'configurable' section of README
- Fix simple text mode UI positioning
- Replace icon

## 0.2.0
- Improve portraits for Shrine of Blood and Artifact Reliquary
- Added damage information to identifiers for unknown damage sources
- Fix portrait color for void damage sources
- Add description to manifest.json
- Start writing proper README
- Create CHANGELOG
<!--  -->
- \[debug\] Improve debugging features

## 0.1.0
- Satisfactory build

# Protect the VIP

Start the game with a monster ally. Try to beat the game (killing Mithirx) without your ally dieing. If your ally dies, your game immediatly ends in failure.

## First Time Setup

No setup needed. If you want to customize the allies you need to boot the game at least once to generate the configuration file.

# Default Allies

|Spawn Card|Name|Difficultly|
|----------|----|-----------|
|`SpawnCards/CharacterSpawnCards/cscBeetle`|Beetle|Hard|
|`SpawnCards/CharacterSpawnCards/cscLemurian`|Lemurian|Medium|
|`SpawnCards/CharacterSpawnCards/cscLemurianBruiser`|Elder Lemurian|Easy|
|`SpawnCards/CharacterSpawnCards/cscMiniMushroom`|Mini Mushrum|Hard|
|`SpawnCards/CharacterSpawnCards/cscParent`|Parent|Easy|
|`SpawnCards/CharacterSpawnCards/cscVulture`|Alloy Vulture|Medium|
|`SpawnCards/CharacterSpawnCards/cscBison`|Bighorn Bison|Medium|
|`SpawnCards/CharacterSpawnCards/cscBell`|Brass Contraption|Easy|
|`SpawnCards/CharacterSpawnCards/cscClayBruiser`|Clay Templar|Medium|
|`SpawnCards/CharacterSpawnCards/cscGolem`|Stone Golem|Medium|
|`SpawnCards/CharacterSpawnCards/cscImp`|Imp|Hard|
|`SpawnCards/CharacterSpawnCards/cscScav`|Scavenger|Easy|

These are "default" via the `CustomSpawnCards` configuration:

|Spawn Card|Name|Difficultly|
|----------|----|-----------|
|`SpawnCards/CharacterSpawnCards/cscJellyfish`|Jellyfish|Very, Very Hard|
|`SpawnCards/CharacterSpawnCards/cscRoboBallMini`|Solus Probe|Medium|

## In-game Spawn Cards

Add these to the `CustomSpawnCards` configuration to add them as allies to the game.
Some monsters are broken (and utterly broken), so you're milage may vary.
Loading more then 10 custom monsters (excluding the default allies) may causes problems in the game.

|Spawn Card|Name|
|----------|----|
|`SpawnCards/CharacterSpawnCards/cscArchWisp`|Malachite Urchin|
|`SpawnCards/CharacterSpawnCards/cscBackupDrone`|Backup Drone|
|`SpawnCards/CharacterSpawnCards/cscBeetle`|Beetle|
|`SpawnCards/CharacterSpawnCards/cscBeetleCrystal`|???|
|`SpawnCards/CharacterSpawnCards/cscBeetleGuard`|Beetle Guard|
|`SpawnCards/CharacterSpawnCards/cscBeetleGuardAlly`|Beetle Guard (Ally)|
|`SpawnCards/CharacterSpawnCards/cscBeetleGuardCrystal`|???|
|`SpawnCards/CharacterSpawnCards/cscBeetleQueen`|Beetle Queen|
|`SpawnCards/CharacterSpawnCards/cscBell`|Brass Contraption|
|`SpawnCards/CharacterSpawnCards/cscBison`|Bighorn Bison|
|`SpawnCards/CharacterSpawnCards/cscBrother`|Mithrix (Stage 1)|
|`SpawnCards/CharacterSpawnCards/cscBrotherGlass`|Mithrix (Stage 3)|
|`SpawnCards/CharacterSpawnCards/cscBrotherHurt`|Mithrix (Stage 4)|
|`SpawnCards/CharacterSpawnCards/cscClayBoss`|Clay Dunestrider|
|`SpawnCards/CharacterSpawnCards/cscClayBruiser`|Clay Templar|
|`SpawnCards/CharacterSpawnCards/cscElectricWorm`|Overloading Worm|
|`SpawnCards/CharacterSpawnCards/cscGolem`|Stone Golem|
|`SpawnCards/CharacterSpawnCards/cscGravekeeper`|Grovetender|
|`SpawnCards/CharacterSpawnCards/cscGreaterWisp`|Greater Wisp|
|`SpawnCards/CharacterSpawnCards/cscHermitCrab`|Hermit Crab|
|`SpawnCards/CharacterSpawnCards/cscImp`|Imp|
|`SpawnCards/CharacterSpawnCards/cscImpBoss`|Imp Overlord|
|`SpawnCards/CharacterSpawnCards/cscJellyfish`|Jellyfish|
|`SpawnCards/CharacterSpawnCards/cscLemurian`|Lemurian|
|`SpawnCards/CharacterSpawnCards/cscLemurianBruiser`|Elder Lemurian|
|`SpawnCards/CharacterSpawnCards/cscLesserWisp`|Lesser WIsp|
|`SpawnCards/CharacterSpawnCards/cscLunarGolem`|Lunar Chimera (Golem)|
|`SpawnCards/CharacterSpawnCards/cscLunarWisp`|Lunar Chimera (Wisp)|
|`SpawnCards/CharacterSpawnCards/cscMagmaWorm`|Magma Worm|
|`SpawnCards/CharacterSpawnCards/cscMiniMushroom`|Mini Mushrum|
|`SpawnCards/CharacterSpawnCards/cscNullifier`|Void Reaver|
|`SpawnCards/CharacterSpawnCards/cscParent`|Parent|
|`SpawnCards/CharacterSpawnCards/cscParentPod`|Parent Pod|
|`SpawnCards/CharacterSpawnCards/cscRoboBallBoss`|Solus Control Unit|
|`SpawnCards/CharacterSpawnCards/cscRoboBallMini`|Solus Probe|
|`SpawnCards/CharacterSpawnCards/cscScav`|Scavenger|
|`SpawnCards/CharacterSpawnCards/cscScavLunar`|Twisted Scavenger|
|`SpawnCards/CharacterSpawnCards/cscSquidTurret`|Squid Turret|
|`SpawnCards/CharacterSpawnCards/cscSuperRoboBallBoss`|Alloy Worship Unit|
|`SpawnCards/CharacterSpawnCards/cscTitanGold`|Aurelionite|
|`SpawnCards/CharacterSpawnCards/cscTitanGoldAlly`|Aurelionite (Ally)|
|`SpawnCards/CharacterSpawnCards/cscVagrant`|Wandering Vagrant|
|`SpawnCards/CharacterSpawnCards/cscVulture`|Alloy Vulture|
|`SpawnCards/CharacterSpawnCards/cscGrandparent`|Grandparent|
|`SpawnCards/CharacterSpawnCards/cscTitanBlackBeach`|Stone Titan|
|`SpawnCards/CharacterSpawnCards/cscTitanDampCave`|Stone Titan|
|`SpawnCards/CharacterSpawnCards/cscTitanGolemPlains`|Stone Titan|
|`SpawnCards/CharacterSpawnCards/cscTitanGooLake`|Stone Titan|

# Configurations

## SpawnCards

|Config|Type|Default|Notes|
|------|----|-------|-----|
|`LogToConsole`|true/false|false|Log all available spawn cards from `SpawnCards/CharacterSpawnCards/` when the game starts.|
|`CustomSpawnCards`|text|`SpawnCards/CharacterSpawnCards/cscJellyfish; SpawnCards/CharacterSpawnCards/cscRoboBallMini`|Semi-colon seperated (;) entries of Spawn Cards.|

# Changelog

### 1.0.7

See more info: https://github.com/JustDerb/RoR2-VsTwitch/milestone/5?closed=1

* Removed R2API dependency
* Update code to work with the new Risk of Rain 2 DLC: Survivors of the Void

### 1.0.6

* Fixed bug where you could only play the mod once before it broke

### 1.0.3

See more info: https://github.com/JustDerb/RoR2-ProtectTheVIP/milestone/3?closed=1

* Updated library for new RoR2 release

### 1.0.2

See more info: https://github.com/JustDerb/RoR2-ProtectTheVIP/milestone/2?closed=1

* Ally's no longer count towards the "Player Controlled" count; which should not increase the difficulty of teleporters (and other things)
* Players now correctly start the first stage with the pre-defined starting Gold, instead of zero

### 1.0.1

See more info: https://github.com/JustDerb/RoR2-ProtectTheVIP/milestone/1?closed=1

* Non-ally runs no longer activate mod on Moon stage (Mithrix)
* README.md updates (formatting)

### 1.0.0

* Initial release
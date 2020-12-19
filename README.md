# Protect the VIP

Start the game with a monster ally. Try to beat the game (killing Mithirx) without your ally dieing. If your ally dies, your game immediatly ends in failure.

## First Time Setup

No setup needed. If you want to customize the allies you need to boot the game at least once to generate the configuration file.

# Default Allies

|Spawn Card|Name|Difficultly|
|----------|----|-----------|
|`SpawnCards/CharacterSpawnCards/cscBeetle`|Beetle|Hard|
|`SpawnCards/CharacterSpawnCards/cscLemurian`|Lemurian|Medium|
|`SpawnCards/CharacterSpawnCards/cscLemurianBruiser`|ELder Lemurian|Easy|
|`SpawnCards/CharacterSpawnCards/cscMiniMushroom`|Mioni Mushrum|Hard|
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
|`SpawnCards/CharacterSpawnCards/cscBackupDrone|Backup Drone|
|`SpawnCards/CharacterSpawnCards/cscBeetle|Beetle|
|`SpawnCards/CharacterSpawnCards/cscBeetleCrystal|???|
|`SpawnCards/CharacterSpawnCards/cscBeetleGuard|Beetle Guard|
|`SpawnCards/CharacterSpawnCards/cscBeetleGuardAlly|Beetle Guard (Ally)|
|`SpawnCards/CharacterSpawnCards/cscBeetleGuardCrystal|???|
|`SpawnCards/CharacterSpawnCards/cscBeetleQueen|Beetle Queen|
|`SpawnCards/CharacterSpawnCards/cscBell|Brass Contraption|
|`SpawnCards/CharacterSpawnCards/cscBison|Bighorn Bison|
|`SpawnCards/CharacterSpawnCards/cscBrother|Mithrix (Stage 1)|
|`SpawnCards/CharacterSpawnCards/cscBrotherGlass|Mithrix (Stage 3)|
|`SpawnCards/CharacterSpawnCards/cscBrotherHurt|Mithrix (Stage 4)|
|`SpawnCards/CharacterSpawnCards/cscClayBoss|Clay Dunestrider|
|`SpawnCards/CharacterSpawnCards/cscClayBruiser|Clay Templar|
|`SpawnCards/CharacterSpawnCards/cscElectricWorm|Overloading Worm|
|`SpawnCards/CharacterSpawnCards/cscGolem|Stone Golem|
|`SpawnCards/CharacterSpawnCards/cscGravekeeper|Grovetender|
|`SpawnCards/CharacterSpawnCards/cscGreaterWisp|Greater Wisp|
|`SpawnCards/CharacterSpawnCards/cscHermitCrab|Hermit Crab|
|`SpawnCards/CharacterSpawnCards/cscImp|Imp|
|`SpawnCards/CharacterSpawnCards/cscImpBoss|Imp Overlord|
|`SpawnCards/CharacterSpawnCards/cscJellyfish|Jellyfish|
|`SpawnCards/CharacterSpawnCards/cscLemurian|Lemurian|
|`SpawnCards/CharacterSpawnCards/cscLemurianBruiser|Elder Lemurian|
|`SpawnCards/CharacterSpawnCards/cscLesserWisp|Lesser WIsp|
|`SpawnCards/CharacterSpawnCards/cscLunarGolem|Lunar Chimera (Golem)|
|`SpawnCards/CharacterSpawnCards/cscLunarWisp|Lunar Chimera (Wisp)|
|`SpawnCards/CharacterSpawnCards/cscMagmaWorm|Magma Worm|
|`SpawnCards/CharacterSpawnCards/cscMiniMushroom|Mini Mushrum|
|`SpawnCards/CharacterSpawnCards/cscNullifier|Void Reaver|
|`SpawnCards/CharacterSpawnCards/cscParent|Parent|
|`SpawnCards/CharacterSpawnCards/cscParentPod|Parent Pod|
|`SpawnCards/CharacterSpawnCards/cscRoboBallBoss|Solus Control Unit|
|`SpawnCards/CharacterSpawnCards/cscRoboBallMini|Solus Probe|
|`SpawnCards/CharacterSpawnCards/cscScav|Scavenger|
|`SpawnCards/CharacterSpawnCards/cscScavLunar|Twisted Scavenger|
|`SpawnCards/CharacterSpawnCards/cscSquidTurret|Squid Turret|
|`SpawnCards/CharacterSpawnCards/cscSuperRoboBallBoss|Alloy Worship Unit|
|`SpawnCards/CharacterSpawnCards/cscTitanGold|Aurelionite|
|`SpawnCards/CharacterSpawnCards/cscTitanGoldAlly|Aurelionite (Ally)|
|`SpawnCards/CharacterSpawnCards/cscVagrant|Wandering Vagrant|
|`SpawnCards/CharacterSpawnCards/cscVulture|Alloy Vulture|
|`SpawnCards/CharacterSpawnCards/cscGrandparent|Grandparent|
|`SpawnCards/CharacterSpawnCards/cscTitanBlackBeach|Stone Titan|
|`SpawnCards/CharacterSpawnCards/cscTitanDampCave|Stone Titan|
|`SpawnCards/CharacterSpawnCards/cscTitanGolemPlains|Stone Titan|
|`SpawnCards/CharacterSpawnCards/cscTitanGooLake|Stone Titan|

# Configurations

## SpawnCards

|Config|Type|Default|Notes|
|------|----|-------|-----|
|`LogToConsole`|true/false|false|Log all available spawn cards from `SpawnCards/CharacterSpawnCards/` when the game starts.|
|`CustomSpawnCards`|text|`SpawnCards/CharacterSpawnCards/cscJellyfish; SpawnCards/CharacterSpawnCards/cscRoboBallMini`|Semi-colon seperated (;) entries of Spawn Cards.|

# Changelog

### 1.0.0

* Initial release
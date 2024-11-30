# StringPortraitFramework
### StringPortraitFramework is a simplistic framework by XxHarvzBackxX (commissioned by DolphinIsNotaFish) that leverages CP statements to allow in-game object dialogues to display portraits alongside their dialogue.
Find below a simple example content pack for CP to see how the framework should be used:
```jsonc
// manifest.json -- Basic stuff, with dependency on SPF and a CP content pack notice.
{
	"Name": "[CP] SPF Example Pack",
	"Author": "XxHarvzBackxX",
	"Version": "1.0.0",
	"Description": "Example pack for XxHarvzBackxX's StringPortraitFramework mod",
	"UniqueID": "harv.SPF.ExamplePack",
	"MinimumApiVersion": "4.1.5",
	"UpdateKeys": [ "Nexus:???" ],
	"Dependencies": [
		{
			"UniqueID": "harv.SPF",
			"IsRequired": true
		}
	],
	"ContentPackFor": {
		"UniqueID": "Pathoschild.ContentPatcher"
	}
}
```

```jsonc
// CP Content Pack file -- loads a portrait image and writes a new patch to the SPF log
{
	"Format": "2.4.0",
	"Changes": [
		{
			"LogName": "StringPortraitFramework example pack loading",
			"Action": "EditData",
			"Target": "Mods/harv.SPF/Strings",
			"Entries": {
				"Strings/StringsFromMaps:BusStop.1": {
					"ImagePath": "Mods/harv.SPF.ExamplePack/image", // game content relative
					"NPCName": "Bus Stop Sign",
					"ShouldTrimColon": false, // trims the name of an NPC off. e.g: "Dave: Hello there!" would become "Hello there!"
					"FuzzRatio": 100 // so this is necessary because it checks the value of the string when evaluating.
							 // if the string you're patching has dynamic tokens that you won't know the value of (i.e. they can change)
							 // then you should decrease this to maybe around 90 / 95. 100 = exact match.
				}
			}
		},
		{
			"Action": "Load",
			"Target": "Mods/harv.SPF.ExamplePack/image",
			"FromFile": "assets/example.png"
		}
		
	]
}
```

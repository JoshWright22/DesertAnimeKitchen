# Desert Anime Kitchen

This is a top down factory game in Unity 6 where anime girl workers mine ingredients in a desert and turn them into desserts. You sell desserts for coins and stars, and the stars go into a gacha that gets you more girls. Every girl you pull also plays the next part of her story as a visual novel cutscene.

## How the factory works

Everything happens on a grid. `DesertMap` holds the Unity `Grid` and its tilemaps for the sand, the deposits and the colored deposit outlines. `Factory` tracks which building is on which cell and updates every building each frame. All buildings inherit from `Building`, which handles the footprint, facing direction and passing items to whatever is in front of it. `MinerGirl` mines from deposits, `Conveyor` moves items along, `CookGirl` crafts recipes and `DessertStall` sells desserts into the shared `Stockpile`.

Each building is a prefab in `Assets/Prefabs/Buildings` and the factory just instantiates whichever one its `BuildingDef` points at. Prep Girl and Pastry Girl share the Cook Girl prefab and get their own look from the sprite on their def. Items on belts use the Item View prefab and the placement preview is the Build Ghost prefab.

## Content

Items, deposits, recipes, buildings, characters and cutscenes are all ScriptableObjects from `Assets/Scripts/Data`. You make them from the Create menu under Dessert Factory and edit them in the Inspector. The test ones are in `Assets/TestContent`. `TestGameContent` lists everything the game uses. `TestLayout` is a prebuilt factory you can assign as the Starting Layout on `GameBootstrap` if you want to test with everything already running. If a scene has no content assigned the game falls back to the placeholder content in `GameContent.CreateDefault()`. `Dessert Factory > Build Test Content` only makes assets that are missing, so it won't wipe anything you changed by hand.

Art goes in `Assets/Art`. Pixel art should use Point filtering with no compression. Anything without a sprite gets a plain placeholder shape.

## Stars, gacha and story

Every dessert has a star value on top of its coin price, and the stall adds both to the stockpile when it sells one. `Gacha` spends stars on a pull and picks a girl based on her roll weight. Girls are limited, so each copy you own lets you place one more of her. Copies from `startingCopies` and from the starting layout count as ones you already own.

Each pull also plays the next cutscene in that girl's story list. The first one introduces her and the later ones keep her plot going. `CutscenePlayer` shows them visual novel style with a portrait, a name box and text that types out. Click, space or enter goes to the next line and Esc skips. The factory pauses while a scene is playing.

### Adding a new girl

1. Make a `BuildingDef` for her job or reuse one, and give it her chibi as the sprite.
2. Make a Character asset, point it at that building and give her a portrait.
3. Make Cutscene assets for her story and add them to her story list in order.
4. Add her to the characters list on `TestGameContent`.

## Scenes

Open `Assets/Scenes/TestFactory.unity` to try things out. It starts with an empty map so you build everything yourself. Everything the game needs is already placed in that scene and wired up in the Inspector, like the map, factory, camera, HUD, gacha and cutscene player. `GameBootstrap` on the Game object just generates the map, places the starting layout if there is one and hands the content to everything else. A new game scene needs the same objects, so copying them over from TestFactory is the easiest way to start one.

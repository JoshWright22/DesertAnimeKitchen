# Desert Anime Kitchen

A top down factory builder made in Unity 6. You're building a dessert factory in the middle of a desert. There's no player character; you place anime girl workers, belts and stalls straight onto the grid, and they dig up ingredients, cook them into desserts and sell them for coins.

## How it works

The game is built around a grid. `DesertMap` owns a Unity `Grid` with tilemap layers for the sand, the ingredient deposits, and a colored outline around every deposit cell so you can see where to dig. `Factory` keeps track of which building sits on which cell, handles placing and removing, and ticks every building each frame. Every building extends `Building`, which knows its footprint, which way it faces, and the cell in front of it where it hands items off. Digger Girls pull items out of the deposit under them, Conveyor Belts carry items from cell to cell, Prep and Pastry Girls hold ingredients until they have enough for their recipe and then cook, and Dessert Stalls sell desserts into the shared `Stockpile`. Items only move by one building offering them to the building in front of it, so any chain you build works the same way.

All the content is data. Items, deposits, recipes and buildings are ScriptableObjects (`ItemDef`, `DepositDef`, `RecipeDef`, `BuildingDef`) grouped together in a `GameContent` asset, so adding a new ingredient or worker doesn't need code. `GameBootstrap` wires everything up when the scene starts: it builds the map, creates the factory, and spawns the camera controls, build controls and HUD (regular Unity UI, loaded from `Resources/GameHud`). If a scene has no bootstrap it adds one on its own. `Dessert Factory > Build Test Content` in the editor menu saves the placeholder content out as assets under `Assets/TestContent`, and `Assets/Scenes/TestFactory.unity` is a ready to play test map.

## Controls

- WASD - move the camera
- Right drag - pan the map
- Scroll - zoom in and out
- 1-5 - pick a building, R - rotate
- Left click - build (hold to drag)
- Right click - remove
- Click a Prep or Pastry Girl to change her recipe

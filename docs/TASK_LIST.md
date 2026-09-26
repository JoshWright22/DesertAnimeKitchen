# Task List

Everything that goes into the final game. Updated 9/25 from the whiteboards, both asset docs, the stats sheet and the repo. This list isn't complete yet, so add to it.

## How the game plays
From the whiteboard.

Corporate deploys you to the desert and you're gunning for a promotion. You get 3 weeks to be profitable, with a new profit milestone each week (500k, 1 mil and 3 mil). Choices in dialogue push you toward the workers (wage increase) or toward corporate (demand overtime), and there are 3 endings: unionize, fired and promoted. Basic girls sell early on, rare specialized girls make better desserts, and the Green Tea Girl carries desserts to the truck for the sale.

## Programming

### Already in the game
- [x] Grid map with deposits, camera pan and zoom
- [x] Placing, rotating and removing girls and belts
- [x] Miner girls, conveyors, baker girls with recipes and the dessert stall
- [x] Selling desserts for coins and stars
- [x] Gacha that spends stars on girls, with copies limiting how many you can place
- [x] Visual novel cutscene player (portrait, name box, typed text, skip)
- [x] Yarn Spinner installed and the intro script started (DevJables)

### Core loop
- [ ] Day clock. The workday runs to 5 pm and days count down to each milestone
- [ ] Wages. Every girl costs money per day, paid out of revenue
- [ ] Overtime. Keep girls past 5 pm for 1.25x pay (the choice from Red Velvet's intro)
- [ ] Profit tracker on the HUD, revenue minus wages minus debt
- [ ] Starting loan (debt) and 3 starter girls
- [ ] Milestone check at the end of each week for 500k, 1 mil and 3 mil profit. Missing one is the fired ending
- [ ] Delivery. The Green Tea Girl collects desserts and takes them to the truck, which replaces or feeds the stall
- [ ] Girl tiers. Basic girls sell straight away and rare specialized girls make combo desserts
- [ ] Combo desserts, recipes that take two ingredients like chocolate croissant or strawberry cake
- [ ] Lands. Split the map into regions with their own ores (sand, cream and ice cream, fruit, bread, pumpkin, cookie, chocolate, cake)
- [ ] New ores for pumpkin, cookie and red velvet cake, plus whatever the recipe design picks
- [ ] Gacha rarity with 3 tiers. Miners are common, most bakers are rare and story girls like Red Velvet are super rare

### Story systems
- [ ] Hook Yarn Spinner up to the cutscene UI that's already in the game
- [ ] Yarn commands for showing a portrait, swapping between the default and cringe expressions, changing wages and starting overtime
- [ ] Affinity variables for workers vs corporate (Jennifer), set by dialogue choices
- [ ] Max corporate affinity reveals Jennifer's real name
- [ ] 3 endings picked from affinity and milestones: unionize, fired and promoted
- [ ] Hire dialogue that plays when you pull a girl from the gacha
- [ ] Action dialogue, short lines when a girl is placed, working or on overtime
- [ ] Tutorial where Jennifer walks you through your first miner, belt, baker, sale and gacha pull, waiting for you to do each step. The skip option is already in the script

### UI
- [ ] Bottom toolbar for placing girls and belts
- [ ] Side tabs for desserts, girls and assets
- [ ] Stock counters (have / need) for recipes
- [ ] Coin, star, profit and day/clock display
- [ ] Roll button and gacha pull result screen
- [ ] Milestone popup with the goal, days left and current profit
- [ ] Title screen, pause menu and settings (music and sfx volume)
- [ ] Ending screens for each of the 3 endings

### Wrap up
- [ ] Save and load, or decide it's not needed for the jam
- [ ] Hook up all music and sfx through one audio manager
- [ ] Swap placeholder art for final art as it comes in
- [ ] Playtest the 3 weeks for balance. Can you hit 3 mil?
- [ ] WebGL build for itch, and an itch page with screenshots

## Audio
Anything past this is a bonus.

- [ ] Music: a title track, a gameplay loop and a cutscene track
- [ ] Sound effects for placing girls, selling desserts, gacha pulls, UI clicks and dialogue text

## Writing
- [ ] Finish the intro, tutorial and day one scripts (started)
- [ ] Hire dialogue for every girl
- [ ] Action dialogue lines for every girl
- [ ] Jennifer check-ins at each milestone
- [ ] Workers vs corporate choice scenes across the 3 weeks
- [ ] The 3 endings
- [ ] Story scenes for every girl, starting with an intro for when you first pull her
- [ ] Lock the milestone numbers. The whiteboard says 500k / 1 mil / 3 mil but the script says 1 mil is due in five days

## Art
Pixel art uses a 32x32 canvas with at least 1 pixel of wiggle room around the edges. Anything that takes up 2x2 cells, like the Pastry Girl and the stall, goes on a 64x64 canvas so the pixels match. Girls only get one front view since the arrow already shows which way she hands things off. Finished art goes in `Assets/Art`. The ores/lands tab of the asset doc has more detail on the lands.

### Character design
- [x] Ice cream girl
- [x] Cookie girl
- [x] Strawberry/fruit girl
- [x] Corpo-sama/tutorial-chan, candy themed (her real name is Jennifer)
- [x] Green tea girl, she moves desserts to the truck
- [ ] Fork and spoon girl (miners)
- [ ] Prep girl, only if we keep the prep step
- [ ] Vanilla girl? Nobody remembers who she was, so probably not

### Pixel sprites
Jennifer doesn't need one since she's only in the story and the tutorial.

- [ ] Fork and spoon girl (miners)
- [x] Croissant girl
- [ ] Cream puff girl
- [x] Pumpkin girl
- [ ] Ice cream girl
- [x] Chocolate girl
- [ ] Cake girl (Red Velvet). The game is using the croissant girl chibi as a placeholder
- [ ] Pie girl
- [ ] Strawberry girl
- [ ] Cookie girl
- [ ] Green tea girl
- [ ] Prep girl

### Half body portraits
Every girl speaks in the story, so every girl needs a default and a cringe expression. Red Velvet's portraits (1500x1700, transparent background) are the size and framing to match.

- [x] Red Velvet, default and cringe
- [ ] Jennifer (tsundere tutorial-chan), due tomorrow EOD
- [ ] Chocolate croissant girl
- [ ] Fork and spoon girl
- [ ] Croissant girl
- [ ] Cream puff girl
- [ ] Pumpkin girl
- [ ] Ice cream girl
- [ ] Chocolate girl
- [ ] Pie girl
- [ ] Strawberry girl
- [ ] Cookie girl
- [ ] Green tea girl
- [ ] Prep girl

### Animations (Dara)
Girls stay in one spot like machines, so nobody needs running or walking animations. Each animation is 4 frames at 8 fps, laid out as one horizontal strip with every frame the same size as the sprite. Jennifer doesn't need any.

- [ ] Working and idle for each factory girl
- [ ] Mining and idle for each miner girl
- [ ] Collecting and idle for the delivery girl
- [ ] Placement animation for each girl, plays when she gets put down

### World (Zoe)
- [ ] Conveyor
- [ ] Desert sand tile
- [ ] Tiles for each land
- [ ] Grid lines
- [ ] Output arrow that shows which way a girl hands items off
- [ ] Dessert stall, or the truck if delivery replaces it

### Ores
Each ore needs a ground tile for the map and a small icon for when it's riding a belt.

- [x] Bread
- [ ] Ice cream
- [ ] Fruit
- [ ] Chocolate
- [ ] Cream
- [ ] Pumpkin
- [ ] Cookie
- [ ] Red velvet cake

### Items
- [x] Croissant
- [x] Strawberry
- [x] Chocolate strawberry
- [ ] Cookie
- [ ] Chocolate
- [ ] Pumpkin pie
- [ ] Fruit pie
- [ ] Ice cream
- [ ] Combo desserts we pick, like chocolate croissant, chocolate cake, chocolate cookie, chocolate pie, chocolate ice cream, pumpkin cookie, pumpkin cake, strawberry cake, strawberry pie or strawberry ice cream. Every combo needs its own icon and recipe

### UI
- [ ] Confirm, back and buy buttons
- [ ] Coin icon
- [ ] Star icon
- [ ] Toolbar buttons for placing girls and belts
- [ ] Stock, info and help panels
- [ ] Roll button
- [ ] Gacha pull result screen, and maybe a pull animation
- [ ] Cutscene text box and name plate
- [ ] Cutscene backgrounds
- [ ] Title screen and logo
- [ ] Color palette

## Design decisions still open
The recipes in the game right now are placeholder desert stuff like dates, cactus and millet.

- [ ] Game name. Desert Anime Kitchen is just a temp name
- [ ] Which ores give what, like whether fruit ore gives strawberries
- [ ] Which desserts each baker makes and what they need
- [ ] Whether there's a prep step or ores go straight to the bakers. No prep step means we can drop the prep girl
- [ ] Which combo desserts come first
- [ ] Coin and star price for each dessert, and the wage for each girl
- [ ] Dango and macaron girls? (ideas tab)

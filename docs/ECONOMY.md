# Economy and progression

How the desserts, girls, wages, stars and milestones fit together, and where each number lives.

## The core decision

The real limit is girls. The gacha hands out random copies of Honey (miner), Mocha (baker) and Red Velvet (pastry), so every run you have a different mix. Then you have to decide:

- **Cash or stars.** Coins clear milestones; stars buy more girls. Every girl mix has a dessert that's better for each, so you can't max both at once.
- **What to do with the girls you got.** Lots of Honeys, lots of Mochas, or a spare Red Velvet each point to different desserts.
- **Where your shared ingredients go.** Chocolate goes into Cookies, Chocolate Croissants, Chocolate Strawberries and Red Velvet Cake. Ice cream goes into Cones, Cream and Sundaes. Dough goes into almost everything.
- **When.** Stars matter most early, while pulls are cheap and each new girl pays off for longer. Cash matters most late, when pulls cost thousands and the milestones loom. A good factory gets rebuilt partway through.

## What each dessert is for

| Girls a line needs | Cash pick | Stars pick |
|---|---|---|
| Mocha, no Red Velvet | **Croissant** (1 Honey, 2 Mocha) | **Chocolate Chip Cookie** (2 Honey, 2 Mocha) |
| Red Velvet, no spare Mocha | **Apple Pie** (2 Honey, 1 Red Velvet) | **Red Velvet Cake** (2 Honey, 1 Red Velvet, rare Cake land) |
| A small Red Velvet line | **Ice Cream Sundae** (2 / 1 / 1, three lands) | **Ice Cream Cone** (2 / 1 / 1, quick and simple) |
| A big line | **Chocolate Croissant** (2 / 2 / 1, longest chain), **Pumpkin Pie** (3 / 1 / 1, for spare Honeys) | **Chocolate Strawberry** (3 / 2 / 1) |

Raw ore sells for coins but never stars. Bread, Strawberry, Cream and Cookie are stepping stones: better than raw, worse than finishing them.

Craft times are picked so one miner keeps one baker busy, and nobody in a chain sits half idle.

## Numbers

Prices and stars are set by hand on each item asset. Wages are on each girl's building asset. Run **Dessert Factory → Pipeline Report** after changing anything: it works out every line from the real assets and logs girls, coins, wages and stars per day.

| Dessert | Coins | Stars | Craft time |
|---|---|---|---|
| Raw ore (any) | 400 | 0 | 1.2 s to mine |
| Bread | 1,550 | 1 | 2 s |
| Strawberry, Cream | 1,750 | 1 | 2.4 s |
| Cookie | 3,600 | 3 | 3 s |
| Croissant | 7,800 | 3 | 4 s |
| Chocolate Chip Cookie | 5,600 | 10 | 3 s |
| Apple Pie | 11,000 | 3 | 5 s |
| Red Velvet Cake | 7,500 | 18 | 5 s |
| Ice Cream Cone | 3,600 | 13 | 2 s |
| Chocolate Strawberry | 8,650 | 18 | 2.4 s |
| Ice Cream Sundae | 16,650 | 6 | 4.8 s |
| Pumpkin Pie | 24,500 | 8 | 6 s |
| Chocolate Croissant | 17,650 | 10 | 4 s |

**Wages per day:** Honey 24,000, Mocha 40,000, Red Velvet 60,000. That's 18–37% of a line's revenue, so idle girls hurt and the overtime and raise choices cost real money. Dialogue costs are scaled to match: umbrellas cost 50,000, approving the insurance claim 60,000, and the raise is 20% on every wage.

**Gacha** (on the Gacha object in the scene):
- The first pull is free and always Mocha.
- After that it costs 200 stars, then ×1.35 a pull (250, 350, 500, 650, 900 …).
- Red Velvet is guaranteed by the third pull.
- Roll weights: Honey 10, Mocha 8, Red Velvet 4.
- Recipes come from the gacha too. A girl shows up knowing only her first recipe (Bread for Mocha, Ice Cream Cone for Red Velvet), and every paid pull has a 40% chance to be a recipe for a girl you already have instead of another girl. So every run has a different menu, and the factory gets rebuilt around whatever turns up. Recipes she hasn't learned show greyed out on her recipe panel.
- The whole-run numbers below were worked out before recipes were locked, so they're optimistic until they're redone.

**Milestones** (on the Campaign object in the scene): 1,000,000 by day 5, 3,500,000 by day 10, 7,000,000 by day 15. Day 5 stays at one million because Jennifer says so in Kitchen_Intro.

## How it was tested

**1. Every production line on its own.** For every dessert, the model worked out what one full-speed line needs (girls of each kind, which lands), what it makes a day, and its wages and stars. This is what Pipeline Report prints.

**2. Every girl mix.** For 3–8 Honeys, 0–4 Mochas and 0–2 Red Velvets, the model found the best possible factory in early, mid and late game, valuing a star at 520, 260 and 80 coins. The table counts how often each dessert is part of the best factory, out of 90 girl mixes per phase:

| Dessert | Early | Mid | Late |
|---|---|---|---|
| Cookie | 15 | 11 | 12 |
| Croissant | 6 | 4 | 34 |
| Chocolate Chip Cookie | 23 | 27 | 0 |
| Apple Pie | 0 | 0 | 17 |
| Red Velvet Cake | 21 | 17 | 0 |
| Ice Cream Cone | 21 | 0 | 0 |
| Chocolate Strawberry | 28 | 18 | 0 |
| Ice Cream Sundae | 0 | 4 | 6 |
| Pumpkin Pie | 0 | 24 | 26 |
| Chocolate Croissant | 5 | 9 | 22 |

Every finished dessert is the right call somewhere, and the right call moves from star desserts to cash desserts as the weeks go on. Before this pass, the Chocolate Croissant was best at everything, and the pies and bread earned less than selling raw dough.

**3. Whole runs.** 300 runs of 15 days with random pulls. The model is a player who keeps building the best factory for the girls they have. It assumes 60% of perfect speed, to allow for building time and belt routing. Profit is shown as worst 10% / median / best 10%:

| Play style | Day 5 | Day 10 | Day 15 |
|---|---|---|---|
| Uses whatever fits their girls | 1.03 / 1.16 / 1.45M | 3.4 / 4.6 / 5.8M | 6.5 / 8.7 / 10.8M |
| Mocha desserts only | 0.6M | 1.3M | 2.2M |
| Pies and sundaes only | 0.6M | 2.0M | 4.5M |
| Star desserts only | 0.6M | 2.0M | 4.4M |
| Raw ore only | 0.2M | 0.4M | 0.6M |

A player who adapts to their pulls clears all three milestones with some room. Sticking to one family, or ignoring stars, doesn't. Overtime is the catch-up lever: about +25% net a day at an affinity cost.

## Still needs a real playtest

- Distance between lands and belt routing aren't modelled. Pumpkin Pie (three lands) and Red Velvet Cake (rare Cake land) may feel harder than the numbers say.
- Day 2 wages for the starting 3 Honeys and a Mocha are about 112,000. A slow first day could leave you short of coins to build with.

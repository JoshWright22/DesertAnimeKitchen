# Palette

16 colors pulled from the game's art: every sprite, land tile and portrait in `Assets/Art`, with each file weighted the same. Every color here already shows up in the sprites, so new art and UI built from it will match. The swatch strip is `Assets/Art/UI/palette.png`, one 8x8 square per color in this order.

| | Name | Hex | Where it comes from | UI use |
|---|---|---|---|---|
| 1 | Ink | `#271d1a` | conveyor shadows, outlines | dark panel outline, cutscene dim |
| 2 | Belt | `#392b26` | conveyor belts | dark panel fill |
| 3 | Cocoa | `#492a13` | chocolate, crust outlines | text, panel and button outlines |
| 4 | Suit | `#40414f` | Jennifer's suit | |
| 5 | Velvet | `#970b23` | red velvet cake, strawberry shade | error text, accent button shade |
| 6 | Berry | `#bf394f` | cake land, strawberries | accent button (Roll), star icon |
| 7 | Crust | `#914f12` | croissant and pie outlines | secondary text, coin rim |
| 8 | Caramel | `#c28142` | baked crust | panel inner border, item slots |
| 9 | Pumpkin | `#e67905` | pumpkins | coin shade |
| 10 | Cookie | `#dc9c56` | cookie land, dough shade | pressed buttons |
| 11 | Sand | `#e3ba66` | desert sand | button shade |
| 12 | Honey | `#ffbf57` | pumpkin land, Honey's hair | selected build button, coin |
| 13 | Olive | `#72843c` | fruit land | |
| 14 | Mint | `#8ee79d` | ice cream land | |
| 15 | Cream | `#f8e4c8` | dough land, whipped cream | panel fill, hovered buttons |
| 16 | Frosting | `#fffbef` | frosting, sparkles | button fill, text on dark panels |

## UI art
The UI sprites in `Assets/Art/UI` are drawn in this palette at 25 pixels per unit, so one art pixel is 4 UI units on the 1920x1080 canvas. Panels and buttons are 9-sliced. The font is Oleo Script (SIL Open Font License, see `OleoScript-OFL.txt`), used through TextMeshPro SDF font assets so it stays sharp at any size. `<b>` switches to the real bold face.

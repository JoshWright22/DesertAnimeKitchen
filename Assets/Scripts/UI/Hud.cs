using System.Collections.Generic;
using UnityEngine;

namespace DessertFactory
{
    // Quick IMGUI hud to get things playable, swap for real UI later
    public class Hud : MonoBehaviour
    {
        Factory factory;
        GameContent content;
        BuildController builder;

        readonly List<Rect> uiRects = new List<Rect>();
        GUIStyle boxStyle;
        GUIStyle labelStyle;
        bool showHelp = true;

        public void Init(Factory factory, GameContent content, BuildController builder)
        {
            this.factory = factory;
            this.content = content;
            this.builder = builder;
        }

        public bool IsOverUi(Vector2 screenPos)
        {
            var guiPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
            foreach (var rect in uiRects)
            {
                if (rect.Contains(guiPos))
                    return true;
            }
            return false;
        }

        void OnGUI()
        {
            if (factory == null)
                return;

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, padding = new RectOffset(8, 8, 6, 6) };
                labelStyle = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
            }

            // rects get rebuilt once per frame on the layout pass, Update reads them for input checks
            if (Event.current.type == EventType.Layout)
                uiRects.Clear();

            DrawStock();
            DrawToolbar();
            DrawHoverInfo();
            DrawHelp();
        }

        void DrawStock()
        {
            var stock = factory.Stockpile;
            var rect = new Rect(10, 10, 220, 34 + stock.Items.Count * 22);
            Panel(rect);

            GUILayout.BeginArea(rect);
            GUILayout.Label($"<b>Coins: {stock.Coins}</b>   Sold: {stock.DessertsSold}", labelStyle);
            foreach (var pair in stock.Items)
                GUILayout.Label($"{pair.Key.displayName}: {pair.Value}", labelStyle);
            GUILayout.EndArea();
        }

        void DrawToolbar()
        {
            const float buttonWidth = 130f;
            const float buttonHeight = 48f;
            float totalWidth = content.buildings.Count * (buttonWidth + 6f);
            var rect = new Rect((Screen.width - totalWidth) / 2f, Screen.height - buttonHeight - 16f, totalWidth, buttonHeight + 8f);
            Panel(rect);

            for (int i = 0; i < content.buildings.Count; i++)
            {
                var def = content.buildings[i];
                var buttonRect = new Rect(rect.x + 3f + i * (buttonWidth + 6f), rect.y + 4f, buttonWidth, buttonHeight);

                bool selected = builder.Selected == def;
                var old = GUI.backgroundColor;
                if (selected)
                    GUI.backgroundColor = new Color(1f, 0.8f, 0.4f);

                if (GUI.Button(buttonRect, $"[{i + 1}] {def.displayName}\n{def.price} coins"))
                    builder.Select(selected ? null : def);

                GUI.backgroundColor = old;
            }
        }

        void DrawHoverInfo()
        {
            if (!builder.PointerOverWorld)
                return;

            var cell = builder.HoveredCell;
            var lines = new List<string>();

            if (builder.Selected != null)
            {
                lines.Add($"<b>{builder.Selected.displayName}</b>  (facing {builder.Facing})");
                lines.Add(builder.Selected.description);
                if (builder.PlaceError != null)
                    lines.Add($"<color=#ff8080>{builder.PlaceError}</color>");
            }

            var building = factory.GetBuilding(cell);
            if (building != null)
            {
                lines.Add($"<b>{building.Def.displayName}</b>");
                lines.Add(building.GetStatus());
            }
            else
            {
                var deposit = factory.Map.GetDeposit(cell);
                if (deposit != null)
                    lines.Add($"{deposit.item.displayName} deposit ({factory.Map.GetAmount(cell)})");
            }

            if (lines.Count == 0)
                return;

            var rect = new Rect(Screen.width - 290, 10, 280, 30 + lines.Count * 22);
            Panel(rect);
            GUILayout.BeginArea(rect);
            foreach (var line in lines)
                GUILayout.Label(line, labelStyle);
            GUILayout.EndArea();
        }

        void DrawHelp()
        {
            var rect = showHelp ? new Rect(10, Screen.height - 190, 250, 170) : new Rect(10, Screen.height - 40, 80, 30);
            Panel(rect);

            if (!showHelp)
            {
                if (GUI.Button(new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 8), "Controls"))
                    showHelp = true;
                return;
            }

            GUILayout.BeginArea(rect);
            GUILayout.Label(
                "WASD / MMB drag - move camera\n" +
                "Scroll - zoom\n" +
                "1-9 - pick building, R - rotate\n" +
                "G - toggle grid lines\n" +
                "LMB - build (hold to drag)\n" +
                "RMB - remove\n" +
                "Click a cook girl to swap recipe\n" +
                "Esc / Q - cancel", labelStyle);
            if (GUILayout.Button("Hide"))
                showHelp = false;
            GUILayout.EndArea();
        }

        void Panel(Rect rect)
        {
            if (Event.current.type == EventType.Layout)
                uiRects.Add(rect);
            GUI.Box(rect, GUIContent.none, boxStyle);
        }
    }
}

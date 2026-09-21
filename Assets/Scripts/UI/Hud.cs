using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DessertFactory
{
    public class Hud : MonoBehaviour
    {
        [Header("Stock")]
        [SerializeField] Text stockText;

        [Header("Hover info")]
        [SerializeField] GameObject infoPanel;
        [SerializeField] Text infoText;

        [Header("Toolbar")]
        [SerializeField] RectTransform toolbar;
        [SerializeField] Button buildButtonTemplate;
        [SerializeField] Color selectedColor = new Color(1f, 0.8f, 0.4f);

        [Header("Help")]
        [SerializeField] GameObject helpPanel;
        [SerializeField] Button hideHelpButton;
        [SerializeField] Button showHelpButton;

        Factory factory;
        BuildController builder;

        readonly List<BuildingDef> buttonDefs = new List<BuildingDef>();
        readonly List<Button> buttons = new List<Button>();
        readonly List<int> shownLeft = new List<int>();
        BuildingDef highlighted;
        readonly StringBuilder sb = new StringBuilder();

        public void Init(Factory factory, GameContent content, BuildController builder)
        {
            this.factory = factory;
            this.builder = builder;

            buildButtonTemplate.gameObject.SetActive(false);
            for (int i = 0; i < content.buildings.Count; i++)
            {
                var def = content.buildings[i];
                var button = Instantiate(buildButtonTemplate, toolbar);
                button.name = def.displayName;
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() => builder.Select(builder.Selected == def ? null : def));

                buttonDefs.Add(def);
                buttons.Add(button);
                shownLeft.Add(-1);
            }

            hideHelpButton.onClick.AddListener(() => SetHelpVisible(false));
            showHelpButton.onClick.AddListener(() => SetHelpVisible(true));
            SetHelpVisible(true);

            factory.Stockpile.Changed += RefreshStock;
            RefreshStock();
            RefreshHighlight();
        }

        void OnDestroy()
        {
            if (factory != null)
                factory.Stockpile.Changed -= RefreshStock;
        }

        public bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        void Update()
        {
            if (factory == null)
                return;

            if (highlighted != builder.Selected)
                RefreshHighlight();

            RefreshButtonLabels();

            RefreshInfo();
        }

        void SetHelpVisible(bool visible)
        {
            helpPanel.SetActive(visible);
            showHelpButton.gameObject.SetActive(!visible);
        }

        void RefreshStock()
        {
            var stock = factory.Stockpile;
            sb.Clear();
            sb.Append($"<b>Coins: {stock.Coins}    Stars: {stock.Stars}</b>    Sold: {stock.DessertsSold}");
            foreach (var pair in stock.Items)
                sb.Append($"\n{pair.Key.displayName}: {pair.Value}");
            stockText.text = sb.ToString();
        }

        // girls show how many more you can place, which changes whenever one is placed or pulled
        void RefreshButtonLabels()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var def = buttonDefs[i];
                bool limited = factory.Stockpile.IsLimited(def);
                int left = limited ? factory.Stockpile.Owned(def) - factory.CountPlaced(def) : 0;
                if (left == shownLeft[i])
                    continue;

                shownLeft[i] = left;
                var label = $"[{i + 1}] {def.displayName}\n{def.price} coins";
                if (limited)
                    label += $", {left} left";
                buttons[i].GetComponentInChildren<Text>().text = label;
            }
        }

        void RefreshHighlight()
        {
            highlighted = builder.Selected;
            for (int i = 0; i < buttons.Count; i++)
            {
                var colors = buttons[i].colors;
                var tint = buttonDefs[i] == highlighted ? selectedColor : Color.white;
                colors.normalColor = tint;
                colors.selectedColor = tint;
                buttons[i].colors = colors;
            }
        }

        void RefreshInfo()
        {
            sb.Clear();

            if (builder.PointerOverWorld)
            {
                var cell = builder.HoveredCell;

                if (builder.Selected != null)
                {
                    sb.Append($"<b>{builder.Selected.displayName}</b>  (facing {builder.Facing})\n");
                    sb.Append(builder.Selected.description);
                    if (builder.PlaceError != null)
                        sb.Append($"\n<color=#c03030>{builder.PlaceError}</color>");
                }

                var building = factory.GetBuilding(cell);
                if (building != null)
                {
                    if (sb.Length > 0)
                        sb.Append("\n\n");
                    sb.Append($"<b>{building.Def.displayName}</b>\n{building.GetStatus()}");
                }
                else
                {
                    var deposit = factory.Map.GetDeposit(cell);
                    if (deposit != null)
                    {
                        if (sb.Length > 0)
                            sb.Append("\n\n");
                        sb.Append($"{deposit.item.displayName} deposit ({factory.Map.GetAmount(cell)})");
                    }
                }
            }

            bool show = sb.Length > 0;
            if (infoPanel.activeSelf != show)
                infoPanel.SetActive(show);
            if (show)
                infoText.text = sb.ToString();
        }
    }
}

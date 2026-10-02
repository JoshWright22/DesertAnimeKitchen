using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DessertFactory
{
    public class Hud : MonoBehaviour
    {
        [Header("Stock")]
        [SerializeField] TMP_Text coinsText;
        [SerializeField] TMP_Text starsText;
        [Tooltip("Running tally of everything the stall has sold, hidden until the first sale")]
        [FormerlySerializedAs("storagePanel")]
        [SerializeField] GameObject soldPanel;
        [SerializeField] RectTransform itemGrid;
        [SerializeField] ItemSlot itemSlotTemplate;

        [Header("Clock")]
        [SerializeField] TMP_Text dayText;
        [Tooltip("Under the day: when the girls go home, or that it's overtime")]
        [SerializeField] TMP_Text shiftText;
        [SerializeField] PixelClock clock;

        [Header("Profit")]
        [SerializeField] TMP_Text profitText;
        [Tooltip("What corporate wants and by when")]
        [SerializeField] TMP_Text milestoneText;
        [Tooltip("Stretched across its parent as profit closes in on the next milestone")]
        [SerializeField] RectTransform profitBarFill;
        [SerializeField] Color profitColor = new Color(0.29f, 0.16f, 0.07f);

        [Header("Goal")]
        [Tooltip("What the tutorial is waiting for you to do")]
        [SerializeField] GameObject goalPanel;
        [SerializeField] TMP_Text goalText;

        [Header("Ending")]
        [SerializeField] GameObject endingPanel;
        [SerializeField] TMP_Text endingTitle;
        [SerializeField] TMP_Text endingText;
        [SerializeField] Button playAgainButton;

        [Header("Name tag")]
        [Tooltip("Follows the cursor and says what's under it: a girl, something on a belt, a sold item or the ground")]
        [SerializeField] RectTransform nameTag;
        [SerializeField] TMP_Text nameTagText;

        [Header("Recipes")]
        [Tooltip("Opens when you click a baker or pastry girl")]
        [SerializeField] RecipePanel recipePanel;

        [Header("Toolbar")]
        [SerializeField] RectTransform toolbar;
        [SerializeField] BuildButton buildButtonTemplate;
        [SerializeField] Color selectedColor = new Color(1f, 0.75f, 0.34f);
        [SerializeField] Color errorColor = new Color(0.59f, 0.04f, 0.14f);

        Factory factory;
        BuildController builder;
        Campaign campaign;
        int shownMinute = -1;

        readonly List<BuildingDef> buttonDefs = new List<BuildingDef>();
        readonly List<BuildButton> buttons = new List<BuildButton>();
        readonly List<ItemSlot> itemSlots = new List<ItemSlot>();
        readonly List<int> shownLeft = new List<int>();
        readonly List<bool> shownLocked = new List<bool>();
        // which girl works each building, so buttons and tags can use her name
        readonly Dictionary<BuildingDef, CharacterDef> girls = new Dictionary<BuildingDef, CharacterDef>();
        BuildingDef highlighted;

        public void Init(Factory factory, GameContent content, BuildController builder, Campaign campaign)
        {
            this.factory = factory;
            this.builder = builder;
            this.campaign = campaign;

            foreach (var girl in content.characters)
            {
                if (girl != null && girl.building != null)
                    girls[girl.building] = girl;
            }

            buildButtonTemplate.gameObject.SetActive(false);
            for (int i = 0; i < content.buildings.Count; i++)
            {
                var def = content.buildings[i];
                var button = Instantiate(buildButtonTemplate, toolbar);
                button.name = def.displayName;
                button.gameObject.SetActive(true);
                button.Show(def);
                button.Button.onClick.AddListener(() => builder.Select(builder.Selected == def ? null : def));

                buttonDefs.Add(def);
                buttons.Add(button);
                shownLeft.Add(-1);
                shownLocked.Add(false);
            }

            nameTag.gameObject.SetActive(false);

            itemSlotTemplate.gameObject.SetActive(false);

            endingPanel.SetActive(false);
            playAgainButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
            campaign.Ended += ShowEnding;

            factory.Stockpile.Changed += RefreshStock;
            RefreshStock();
            RefreshHighlight();
        }

        void OnDestroy()
        {
            if (factory != null)
                factory.Stockpile.Changed -= RefreshStock;
            if (campaign != null)
                campaign.Ended -= ShowEnding;
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
            RefreshClock();

            bool hasGoal = !string.IsNullOrEmpty(campaign.Goal);
            if (goalPanel.activeSelf != hasGoal)
                goalPanel.SetActive(hasGoal);
            if (hasGoal)
                goalText.text = campaign.Goal;

            RefreshNameTag();
        }

        void RefreshClock()
        {
            clock.Show(campaign.Hour);

            int minute = Mathf.FloorToInt(campaign.Hour * 60f);
            // odd numbers mean overtime, so the label updates the moment it starts or stops
            int shown = minute * 2 + (campaign.InOvertime ? 1 : 0);
            if (shown == shownMinute)
                return;
            shownMinute = shown;

            dayText.text = $"Day {campaign.Day}";
            shiftText.text = campaign.InOvertime
                ? $"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>Overtime until {HourName(campaign.OvertimeUntil)}</color>"
                : $"Closes at {HourName(campaign.ClosingHour)}";
        }

        static string HourName(int hour)
        {
            return $"{(hour + 11) % 12 + 1} {(hour < 12 ? "am" : "pm")}";
        }

        void ShowEnding(Campaign.Ending ending)
        {
            endingTitle.text = ending switch
            {
                Campaign.Ending.Jennifer => "Corner Office for Two",
                Campaign.Ending.RedVelvet => "Sweetheart of the Sands",
                _ => ending.ToString(),
            };
            endingText.text = $"Day {campaign.Day}, profit {Campaign.Money(factory.Stockpile.Profit)}";
            endingPanel.SetActive(true);
        }

        void RefreshStock()
        {
            var stock = factory.Stockpile;
            coinsText.text = stock.Coins.ToString("N0");
            starsText.text = stock.Stars.ToString("N0");

            var next = campaign.NextMilestone;
            profitText.text = stock.Profit.ToString("N0");
            profitText.color = stock.Profit < 0 ? errorColor : profitColor;
            milestoneText.text = next != null ? $"goal {next.profit:N0} by day {next.day}" : "";
            float progress = next != null ? Mathf.Clamp01((float)stock.Profit / next.profit) : 1f;
            profitBarFill.anchorMax = new Vector2(progress, 1f);

            int shown = 0;
            foreach (var pair in stock.Sold)
            {
                if (pair.Value <= 0)
                    continue;
                if (shown == itemSlots.Count)
                    itemSlots.Add(Instantiate(itemSlotTemplate, itemGrid));
                itemSlots[shown].gameObject.SetActive(true);
                itemSlots[shown].Show(pair.Key, pair.Value);
                shown++;
            }
            for (int i = shown; i < itemSlots.Count; i++)
                itemSlots[i].gameObject.SetActive(false);
            soldPanel.SetActive(shown > 0);
        }

        // girls show how many more you can place, which changes whenever one is placed or pulled.
        // Ones you haven't pulled yet stay greyed out.
        void RefreshButtonLabels()
        {
            var stock = factory.Stockpile;
            for (int i = 0; i < buttons.Count; i++)
            {
                var def = buttonDefs[i];
                bool gacha = stock.IsGachaGirl(def);
                bool locked = campaign.IsLocked(def) || gacha && !stock.HasMet(def);
                if (locked != shownLocked[i])
                {
                    shownLocked[i] = locked;
                    buttons[i].SetLocked(locked);
                }

                int left = gacha ? stock.Owned(def) - factory.CountPlaced(def) : 0;
                if (left == shownLeft[i])
                    continue;

                shownLeft[i] = left;
                // girls show how many more of her you can put down, and what she costs a day
                string label = !gacha
                    ? $"{i + 1}. {def.displayName}\n{def.price:N0} coins"
                    : stock.HasMet(def)
                        ? $"{i + 1}. {def.displayName}  x{left}\n{def.price:N0} coins\n<size=85%>{def.wage:N0} a day</size>"
                        : $"{i + 1}. ???\n<size=85%>Pull her from the gacha</size>";
                buttons[i].SetLabel(label);
            }
        }

        void RefreshHighlight()
        {
            highlighted = builder.Selected;
            for (int i = 0; i < buttons.Count; i++)
            {
                var colors = buttons[i].Button.colors;
                var tint = buttonDefs[i] == highlighted ? selectedColor : Color.white;
                colors.normalColor = tint;
                colors.selectedColor = tint;
                buttons[i].Button.colors = colors;
            }
        }

        public void ShowRecipes(CookGirl cook)
        {
            string name = girls.TryGetValue(cook.Def, out var girl) ? girl.displayName : cook.Def.displayName;
            recipePanel.Show(cook, name);
        }

        public void HideRecipes()
        {
            recipePanel.Hide();
        }

        void RefreshNameTag()
        {
            string text = HoverText();
            bool show = text != null;
            if (nameTag.gameObject.activeSelf != show)
                nameTag.gameObject.SetActive(show);
            if (!show)
                return;

            nameTagText.text = text;
            nameTag.position = Mouse.current.position.ReadValue() + new Vector2(18f, -18f);
        }

        string HoverText()
        {
            if (ItemSlot.Hovered != null && ItemSlot.Hovered.Item != null)
                return ItemText(ItemSlot.Hovered.Item);
            if (!builder.PointerOverWorld)
                return null;

            // while you're placing something only the ground gets a tag, to help find a spot
            var building = builder.Selected == null ? factory.GetBuilding(builder.HoveredCell) : null;
            if (building is Conveyor belt)
            {
                var item = belt.ItemNear(builder.PointerWorld);
                return item != null ? ItemText(item) : null;
            }
            if (building != null)
                return BuildingText(building);

            var deposit = factory.Map.GetDeposit(builder.HoveredCell);
            if (deposit == null)
                return null;
            return $"{deposit.displayName}\n<size=75%>Miners dig up {ItemNames(deposit.items)}</size>";
        }

        string BuildingText(Building building)
        {
            if (!girls.TryGetValue(building.Def, out var girl))
                return building.Def.displayName;

            string job = building switch
            {
                CookGirl cook when cook.Recipe != null => cook.Recipe.displayName,
                MinerGirl miner when miner.Digging != null => $"Digging up {ItemNames(miner.Digging.items)}",
                _ => null,
            };
            return job != null ? $"{girl.displayName}\n<size=75%>{job}</size>" : girl.displayName;
        }

        static string ItemText(ItemDef item)
        {
            return $"{item.displayName}\n<size=75%>Sells for {item.sellPrice:N0} coins, {item.stars} {(item.stars == 1 ? "star" : "stars")}</size>";
        }

        static string ItemNames(List<ItemDef> items)
        {
            var names = new List<string>();
            foreach (var item in items)
                names.Add(item.displayName);
            return string.Join(" and ", names);
        }
    }
}

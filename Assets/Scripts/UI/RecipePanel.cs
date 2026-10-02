using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DessertFactory
{
    // Click a baker or pastry girl: what she's making, what it takes, what it sells for, and her other recipes to switch to
    public class RecipePanel : MonoBehaviour
    {
        [SerializeField] TMP_Text title;
        [Tooltip("Ingredients go in front of the arrow, what she makes goes after it")]
        [SerializeField] RectTransform recipeRow;
        [SerializeField] ItemSlot slotTemplate;
        [SerializeField] RectTransform arrow;
        [SerializeField] TMP_Text details;
        [SerializeField] RectTransform recipeList;
        [SerializeField] BuildButton recipeButtonTemplate;
        [SerializeField] Color selectedColor = new Color(1f, 0.75f, 0.34f);

        CookGirl cook;
        int shownRecipe = -1;
        int shownKnown;
        readonly List<ItemSlot> inputSlots = new List<ItemSlot>();
        readonly List<ItemSlot> outputSlots = new List<ItemSlot>();
        readonly List<BuildButton> recipeButtons = new List<BuildButton>();
        readonly StringBuilder text = new StringBuilder();

        public CookGirl Cook => cook;

        public void Show(CookGirl girl, string girlName)
        {
            cook = girl;
            shownRecipe = -1;
            title.text = girlName;

            foreach (var button in recipeButtons)
                Remove(button.gameObject);
            recipeButtons.Clear();
            for (int i = 0; i < girl.Def.recipes.Count; i++)
            {
                int index = i;
                var recipe = girl.Def.recipes[i];
                var button = Instantiate(recipeButtonTemplate, recipeList);
                button.name = recipe.displayName;
                button.gameObject.SetActive(true);
                button.Show(recipe.outputs.Count > 0 ? recipe.outputs[0].item.icon : null);
                // what each one is worth, so picking between them is a real choice
                var made = recipe.outputs[0].item;
                // ones she hasn't learned stay on the list greyed out, so you know what the gacha could still bring
                bool known = girl.Knows(recipe);
                button.SetLabel(known
                    ? $"{recipe.displayName}\n<size=80%>{made.sellPrice:N0} coins, {made.stars} stars</size>"
                    : $"{recipe.displayName}\n<size=80%>Not learned yet, try the gacha</size>");
                button.SetLocked(!known);
                button.Button.onClick.AddListener(() => cook.SetRecipe(index));
                recipeButtons.Add(button);
            }
            shownKnown = girl.KnownRecipes;

            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            cook = null;
            gameObject.SetActive(false);
        }

        void Update()
        {
            // she got picked up
            if (cook == null)
            {
                Hide();
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Hide();
                return;
            }

            // a pull taught her something while the panel was open
            if (cook.KnownRecipes != shownKnown)
                Show(cook, title.text);

            Refresh();
        }

        void Refresh()
        {
            var recipe = cook.Recipe;
            if (recipe == null)
                return;

            if (shownRecipe != cook.RecipeIndex)
            {
                shownRecipe = cook.RecipeIndex;
                BuildRow(recipe);
                for (int i = 0; i < recipeButtons.Count; i++)
                {
                    var colors = recipeButtons[i].Button.colors;
                    colors.normalColor = i == shownRecipe ? selectedColor : Color.white;
                    colors.selectedColor = colors.normalColor;
                    recipeButtons[i].Button.colors = colors;
                }
            }

            // what she's holding changes every few frames
            for (int i = 0; i < recipe.inputs.Count; i++)
            {
                var input = recipe.inputs[i];
                inputSlots[i].Show(input.item, $"{cook.Has(input.item)}/{input.amount}");
            }

            text.Clear();
            foreach (var output in recipe.outputs)
                text.Append($"{output.item.displayName} sells for {output.item.sellPrice:N0} coins and {output.item.stars} stars\n");
            if (cook.Blocked)
                text.Append("<color=#970b23>Nowhere to put it, run a belt out of her front</color>");
            else if (cook.Cooking)
                text.Append($"Baking... {Mathf.RoundToInt(cook.Progress * 100f)}%");
            else
                text.Append($"Waiting on ingredients, {recipe.craftTime:0.#} s a batch");
            if (!cook.PickedByHand)
                text.Append("\n<size=80%>Picks her recipe from whatever shows up first</size>");
            details.text = text.ToString();
        }

        // ingredients, arrow, results
        void BuildRow(RecipeDef recipe)
        {
            foreach (var slot in inputSlots)
                Remove(slot.gameObject);
            foreach (var slot in outputSlots)
                Remove(slot.gameObject);
            inputSlots.Clear();
            outputSlots.Clear();

            foreach (var input in recipe.inputs)
                inputSlots.Add(AddSlot(input.item, $"0/{input.amount}"));
            arrow.SetAsLastSibling();
            foreach (var output in recipe.outputs)
                outputSlots.Add(AddSlot(output.item, $"x{output.amount}"));
        }

        // Destroy waits for the end of the frame, hiding it first keeps it out of this frame's layout
        static void Remove(GameObject old)
        {
            old.SetActive(false);
            Destroy(old);
        }

        ItemSlot AddSlot(ItemDef item, string label)
        {
            var slot = Instantiate(slotTemplate, recipeRow);
            slot.name = item.displayName;
            slot.gameObject.SetActive(true);
            slot.Show(item, label);
            return slot;
        }
    }
}

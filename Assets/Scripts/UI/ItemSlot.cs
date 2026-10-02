using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DessertFactory
{
    // An item icon with a number next to it, like how many have sold or what a recipe needs
    public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Image icon;
        [SerializeField] TMP_Text count;

        // the slot under the mouse, so the HUD can say what's in it
        public static ItemSlot Hovered { get; private set; }

        public ItemDef Item { get; private set; }

        float sparkleOffset;

        public void Show(ItemDef item, int amount)
        {
            Show(item, amount.ToString());
        }

        public void Show(ItemDef item, string label)
        {
            if (Item != item)
                sparkleOffset = Random.value * item.sparkleEvery;
            Item = item;
            icon.sprite = item.IconAt(Time.unscaledTime + sparkleOffset);
            count.text = label;
        }

        void Update()
        {
            if (Item != null && Item.Sparkles)
                icon.sprite = Item.IconAt(Time.unscaledTime + sparkleOffset);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Hovered = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Hovered == this)
                Hovered = null;
        }

        void OnDisable()
        {
            if (Hovered == this)
                Hovered = null;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DessertFactory
{
    // One toolbar button: what she looks like, her name and price
    public class BuildButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text label;

        public Button Button => button;

        public void Show(BuildingDef def)
        {
            Show(def.prefab.SpriteFor(def, Direction.Up));
        }

        public void Show(Sprite sprite)
        {
            icon.sprite = sprite;
            // no art yet means no gap where it would be
            icon.gameObject.SetActive(sprite != null);
        }

        public void SetLabel(string text)
        {
            label.text = text;
        }

        // Locked during the tutorial until Jennifer gets to her
        public void SetLocked(bool locked)
        {
            button.interactable = !locked;
            var faded = locked ? new Color(0.45f, 0.45f, 0.45f, 0.6f) : Color.white;
            icon.color = faded;
            label.alpha = locked ? 0.45f : 1f;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DessertFactory
{
    // Fast forward for the workday. The clock, the girls and the belts all speed up together, talking still pauses everything.
    public class GameSpeed : MonoBehaviour
    {
        [SerializeField] CutscenePlayer cutscenes;
        [SerializeField] Button button;
        [SerializeField] TMP_Text label;
        [Tooltip("Clicking the button or pressing F steps through these")]
        [SerializeField] float[] speeds = { 1f, 2f, 3f };

        int index;

        public float Speed => speeds[index];

        void Start()
        {
            button.onClick.AddListener(Next);
            Refresh();
        }

        void Update()
        {
            button.interactable = !cutscenes.Playing;
            if (cutscenes.Playing)
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
                Next();

            // the cutscene player puts back whatever speed it found, so only touch it while nobody's talking
            Time.timeScale = Speed;
        }

        void Next()
        {
            index = (index + 1) % speeds.Length;
            Refresh();
        }

        void Refresh()
        {
            label.text = $"{Speed:0.#}x";
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DessertFactory
{
    public class GachaMenu : MonoBehaviour
    {
        [SerializeField] Gacha gacha;
        [SerializeField] Button rollButton;
        [SerializeField] TMP_Text rollText;

        [Header("Result")]
        [Tooltip("Pops up over the roll button and fades away by itself, the factory never stops for it")]
        [SerializeField] CanvasGroup resultPanel;
        [SerializeField] Image resultImage;
        [SerializeField] TMP_Text resultText;
        [SerializeField] float showResultFor = 3f;
        [SerializeField] float fadeTime = 0.5f;

        float resultLeft;
        int shownCost = -1;

        void Start()
        {
            rollButton.onClick.AddListener(Roll);
            resultPanel.gameObject.SetActive(false);
        }

        void Update()
        {
            rollButton.interactable = gacha.CanRoll;
            if (shownCost != gacha.RollCost)
            {
                shownCost = gacha.RollCost;
                rollText.text = shownCost == 0 ? "Roll for a girl\nFree!" : $"Roll for a girl\n{shownCost:N0} stars";
            }

            if (resultLeft <= 0f)
                return;
            resultLeft -= Time.unscaledDeltaTime;
            resultPanel.alpha = Mathf.Clamp01(resultLeft / fadeTime);
            if (resultLeft <= 0f)
                resultPanel.gameObject.SetActive(false);
        }

        void Roll()
        {
            var pull = gacha.Roll();
            if (pull.girl == null)
                return;

            var girl = pull.girl;
            if (pull.recipe != null)
            {
                resultImage.sprite = pull.recipe.outputs[0].item.icon;
                resultText.text = $"New recipe!\n<b>{girl.displayName}</b> can make <b>{pull.recipe.displayName}</b>";
            }
            else
            {
                resultImage.sprite = girl.building.sprite != null ? girl.building.sprite : girl.portrait;
                resultText.text = pull.firstTime
                    ? $"New girl!\n<b>{girl.displayName}</b> x{pull.copies} joined the team"
                    : $"<b>{girl.displayName}</b> x{pull.copies}!\nMore of her to place";
            }
            resultImage.enabled = resultImage.sprite != null;
            resultPanel.gameObject.SetActive(true);
            resultPanel.alpha = 1f;
            resultLeft = showResultFor;
        }
    }
}

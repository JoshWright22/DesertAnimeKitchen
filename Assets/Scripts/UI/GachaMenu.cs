using UnityEngine;
using UnityEngine.UI;

namespace DessertFactory
{
    public class GachaMenu : MonoBehaviour
    {
        [SerializeField] Gacha gacha;
        [SerializeField] Button rollButton;
        [SerializeField] Text rollText;

        [Header("Result")]
        [SerializeField] GameObject resultPanel;
        [SerializeField] Image resultImage;
        [SerializeField] Text resultText;
        [SerializeField] Button continueButton;

        CharacterDef pulled;

        void Start()
        {
            rollText.text = $"Roll for a girl\n{gacha.RollCost} stars";
            rollButton.onClick.AddListener(Roll);
            continueButton.onClick.AddListener(CloseResult);
            resultPanel.SetActive(false);
        }

        void Update()
        {
            rollButton.interactable = gacha.CanRoll && !resultPanel.activeSelf;
        }

        void Roll()
        {
            pulled = gacha.Roll(out bool firstTime);
            if (pulled == null)
                return;

            var job = pulled.building;
            resultImage.sprite = job.sprite != null ? job.sprite : pulled.portrait;
            resultImage.enabled = resultImage.sprite != null;
            resultText.text = firstTime
                ? $"New girl!\n<b>{pulled.displayName}</b> joined as a {job.displayName}"
                : $"<b>{pulled.displayName}</b> again!\nYou can place one more {job.displayName}";
            resultPanel.SetActive(true);
        }

        void CloseResult()
        {
            resultPanel.SetActive(false);
            gacha.PlayNextScene(pulled);
        }
    }
}

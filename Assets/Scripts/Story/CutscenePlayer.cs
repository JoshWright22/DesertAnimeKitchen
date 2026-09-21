using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DessertFactory
{
    // Visual novel style scenes: portrait in the middle, name and text box along the bottom.
    // Click, space or enter to go on, escape skips the whole scene.
    public class CutscenePlayer : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image background;
        [SerializeField] Image portrait;
        [SerializeField] GameObject nameBox;
        [SerializeField] Text nameText;
        [SerializeField] Text lineText;
        [SerializeField] Color dimColor = new Color(0f, 0f, 0f, 0.7f);
        [SerializeField] float charsPerSecond = 40f;

        Cutscene scene;
        Action onDone;
        int index;
        float shown;
        float timeScaleBefore;

        public bool Playing => scene != null;

        void Awake()
        {
            root.SetActive(false);
        }

        public void Play(Cutscene cutscene, Action done = null)
        {
            if (Playing)
                Finish();

            scene = cutscene;
            onDone = done;
            index = -1;

            // the factory waits while she talks
            timeScaleBefore = Time.timeScale;
            Time.timeScale = 0f;

            background.sprite = cutscene.background;
            background.color = cutscene.background != null ? Color.white : dimColor;
            portrait.gameObject.SetActive(false);
            root.SetActive(true);
            NextLine();
        }

        void Update()
        {
            if (!Playing)
                return;

            var text = CurrentText;
            if (shown < text.Length)
            {
                shown += charsPerSecond * Time.unscaledDeltaTime;
                lineText.text = text.Substring(0, Mathf.Min(text.Length, (int)shown));
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Finish();
                return;
            }

            bool advance = (mouse != null && mouse.leftButton.wasPressedThisFrame)
                || (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame));
            if (!advance)
                return;

            // first press finishes typing the line, the next one moves on
            if (shown < text.Length)
            {
                shown = text.Length;
                lineText.text = text;
            }
            else
            {
                NextLine();
            }
        }

        string CurrentText => scene.lines[index].text ?? string.Empty;

        void NextLine()
        {
            index++;
            shown = 0f;
            lineText.text = string.Empty;
            if (index >= scene.lines.Count)
            {
                Finish();
                return;
            }

            var line = scene.lines[index];
            var speaker = line.speaker;
            nameBox.SetActive(speaker != null);
            if (speaker != null)
            {
                nameText.text = speaker.displayName;
                nameText.color = speaker.nameColor;
            }

            // narration leaves whoever is on screen, a speaker with no portrait clears it
            var sprite = line.portrait != null ? line.portrait : speaker != null ? speaker.portrait : null;
            if (sprite != null)
            {
                portrait.sprite = sprite;
                portrait.gameObject.SetActive(true);
            }
            else if (speaker != null)
            {
                portrait.gameObject.SetActive(false);
            }
        }

        void Finish()
        {
            root.SetActive(false);
            Time.timeScale = timeScaleBefore;
            scene = null;

            var done = onDone;
            onDone = null;
            done?.Invoke();
        }
    }
}

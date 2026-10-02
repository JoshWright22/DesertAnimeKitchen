using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DessertFactory
{
    // Visual novel style scenes: portrait in the middle, name and text box along the bottom.
    // Plays Cutscene assets by itself, or shows the lines and choices Yarn hands it.
    // Click, space or enter to go on, escape skips the whole scene.
    public class CutscenePlayer : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image background;
        [SerializeField] Image portrait;
        [SerializeField] GameObject nameBox;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text lineText;
        [SerializeField] Color dimColor = new Color(0f, 0f, 0f, 0.7f);
        [SerializeField] float charsPerSecond = 40f;

        [Header("Portrait slide")]
        [Tooltip("How far off to the side she starts, in canvas units")]
        [SerializeField] float slideDistance = 420f;
        [SerializeField] float slideInTime = 0.35f;
        [SerializeField] float slideOutTime = 0.15f;
        [Tooltip("Same speaker changing faces gets a little bounce instead of a whole slide")]
        [SerializeField] float popTime = 0.18f;
        [SerializeField] float popSize = 0.06f;

        [Header("Choices")]
        [SerializeField] RectTransform choiceList;
        [SerializeField] Button choiceTemplate;

        Cutscene scene;
        Action onDone;
        int index;
        float shown;
        int length;
        float timeScaleBefore;
        float ignoreInputUntil;

        // A frame after alt tabbing back can cover however long you were away, so typing never jumps more than this
        const float MaxTypingStep = 0.1f;
        readonly List<Button> choices = new List<Button>();
        Action<int> onChoice;
        enum SlidePhase
        {
            None,
            Out,
            In
        }

        Vector2 portraitHome;
        CharacterDef onScreen;
        float popLeft;
        SlidePhase slidingTo;
        float slideTime;
        Sprite slideNext;

        public bool Playing { get; private set; }

        // Yarn waits on this before moving to the next line
        public bool LineFinished { get; private set; }

        // Escape during a Yarn scene, the presenter stops the runner
        public event Action SkipRequested;

        void Awake()
        {
            root.SetActive(false);
            choiceTemplate.gameObject.SetActive(false);
            portraitHome = portrait.rectTransform.anchoredPosition;
        }

        // The dialogue runner closes its scene on the way out of play mode, which can be after this UI is gone
        // The click that brings the window back into focus shouldn't also count as reading on
        void OnApplicationFocus(bool focused)
        {
            if (focused)
                ignoreInputUntil = Time.unscaledTime + 0.25f;
        }

        void OnDestroy()
        {
            Playing = false;
        }

        public void Play(Cutscene cutscene, Action done = null)
        {
            if (Playing)
                Finish();

            Begin(cutscene.background);
            scene = cutscene;
            onDone = done;
            index = -1;
            NextLine();
        }

        public void Begin(Sprite backdrop)
        {
            // the factory waits while she talks
            timeScaleBefore = Time.timeScale;
            Time.timeScale = 0f;
            Playing = true;

            background.sprite = backdrop;
            background.color = backdrop != null ? Color.white : dimColor;
            StopSlide();
            onScreen = null;
            popLeft = 0f;
            portrait.rectTransform.localScale = Vector3.one;
            portrait.gameObject.SetActive(false);
            nameBox.SetActive(false);
            lineText.text = string.Empty;
            ClearChoices();
            root.SetActive(true);
        }

        public void End()
        {
            if (!Playing)
                return;

            ClearChoices();
            root.SetActive(false);
            Time.timeScale = timeScaleBefore;
            Playing = false;
            scene = null;
        }

        // A speaker without a CharacterDef still gets her name shown, just in plain colours.
        // Leave the name empty to use the speaker's own.
        public void ShowLine(CharacterDef speaker, string speakerName, Sprite pose, string text)
        {
            ClearChoices();
            if (speaker != null && string.IsNullOrEmpty(speakerName))
                speakerName = speaker.displayName;

            bool named = !string.IsNullOrEmpty(speakerName);
            nameBox.SetActive(named);
            if (named)
            {
                nameText.text = speakerName;
                nameText.color = speaker != null ? speaker.nameColor : Color.white;
            }

            // narration leaves whoever is on screen, a speaker with no portrait clears it
            var sprite = pose != null ? pose : speaker != null ? speaker.portrait : null;
            // she's still on her way in or already here, so just change her face rather than sending her back out
            bool sameSpeaker = speaker != null && speaker == onScreen && portrait.gameObject.activeSelf && slidingTo != SlidePhase.Out;
            if (sameSpeaker && sprite != null)
            {
                if (sprite != portrait.sprite && slidingTo == SlidePhase.None)
                    popLeft = popTime;
                portrait.sprite = sprite;
                slideNext = sprite;
            }
            else if (sprite != null)
            {
                SlideTo(sprite);
            }
            else if (named)
            {
                SlideTo(null);
            }
            if (named)
                onScreen = speaker;

            lineText.text = text ?? string.Empty;
            lineText.maxVisibleCharacters = 0;
            lineText.ForceMeshUpdate();
            length = lineText.textInfo.characterCount;
            shown = 0f;
            LineFinished = false;
        }

        // New speakers swoop in from the side and settle with a little overshoot, Hades style.
        // Whoever was there slides back out first.
        void SlideTo(Sprite sprite)
        {
            bool showing = portrait.gameObject.activeSelf;
            if (showing && portrait.sprite == sprite && slidingTo == SlidePhase.None)
                return;
            if (!showing && sprite == null)
                return;

            slideNext = sprite;
            if (showing)
            {
                slidingTo = SlidePhase.Out;
                slideTime = 0f;
            }
            else
            {
                StartSlideIn();
            }
        }

        void StartSlideIn()
        {
            slideTime = 0f;
            portrait.gameObject.SetActive(slideNext != null);
            if (slideNext == null)
            {
                StopSlide();
                return;
            }

            portrait.sprite = slideNext;
            slidingTo = SlidePhase.In;
            portrait.rectTransform.anchoredPosition = portraitHome + Vector2.right * slideDistance;
            SetPortraitAlpha(0f);
        }

        void UpdateSlide()
        {
            if (slidingTo == SlidePhase.None)
                return;

            var rect = portrait.rectTransform;
            if (slidingTo == SlidePhase.Out)
            {
                slideTime += Time.unscaledDeltaTime / slideOutTime;
                float t = Mathf.Min(slideTime, 1f);
                rect.anchoredPosition = portraitHome + Vector2.left * slideDistance * t * t;
                SetPortraitAlpha(1f - t);
                if (slideTime >= 1f)
                    StartSlideIn();
                return;
            }

            slideTime += Time.unscaledDeltaTime / slideInTime;
            float k = Mathf.Min(slideTime, 1f);
            rect.anchoredPosition = Vector2.LerpUnclamped(portraitHome + Vector2.right * slideDistance, portraitHome, EaseOutBack(k));
            SetPortraitAlpha(Mathf.Clamp01(k * 3f));
            if (slideTime >= 1f)
                StopSlide();
        }

        void UpdatePop()
        {
            if (popLeft <= 0f)
                return;
            popLeft = Mathf.Max(0f, popLeft - Time.unscaledDeltaTime);
            float bounce = Mathf.Sin(popLeft / popTime * Mathf.PI) * popSize;
            portrait.rectTransform.localScale = Vector3.one * (1f + bounce);
        }

        void StopSlide()
        {
            slidingTo = SlidePhase.None;
            portrait.rectTransform.anchoredPosition = portraitHome;
            SetPortraitAlpha(1f);
        }

        void SetPortraitAlpha(float alpha)
        {
            var color = portrait.color;
            color.a = alpha;
            portrait.color = color;
        }

        static float EaseOutBack(float t)
        {
            const float overshoot = 1.4f;
            t -= 1f;
            return 1f + t * t * ((overshoot + 1f) * t + overshoot);
        }

        public void ShowChoices(IReadOnlyList<string> labels, Action<int> picked)
        {
            ClearChoices();
            onChoice = picked;
            for (int i = 0; i < labels.Count; i++)
            {
                int choice = i;
                var button = Instantiate(choiceTemplate, choiceList);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TMP_Text>().text = labels[i];
                button.onClick.AddListener(() => Choose(choice));
                choices.Add(button);
            }
            choiceList.gameObject.SetActive(true);
        }

        void Choose(int choice)
        {
            var picked = onChoice;
            ClearChoices();
            picked?.Invoke(choice);
        }

        void ClearChoices()
        {
            foreach (var button in choices)
                Destroy(button.gameObject);
            choices.Clear();
            onChoice = null;
            choiceList.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!Playing)
                return;

            UpdateSlide();
            UpdatePop();

            if (shown < length)
            {
                shown += charsPerSecond * Mathf.Min(Time.unscaledDeltaTime, MaxTypingStep);
                lineText.maxVisibleCharacters = Mathf.Min(length, (int)shown);
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (scene != null)
                    Finish();
                else
                    SkipRequested?.Invoke();
                return;
            }

            // picking a choice is done with its button
            if (choices.Count > 0)
                return;

            if (Time.unscaledTime < ignoreInputUntil)
                return;

            bool advance = (mouse != null && mouse.leftButton.wasPressedThisFrame)
                || (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame));
            if (!advance)
                return;

            // first press finishes typing the line, the next one moves on
            if (shown < length)
            {
                shown = length;
                lineText.maxVisibleCharacters = length;
            }
            else if (scene != null)
            {
                NextLine();
            }
            else
            {
                LineFinished = true;
            }
        }

        void NextLine()
        {
            index++;
            if (index >= scene.lines.Count)
            {
                Finish();
                return;
            }

            var line = scene.lines[index];
            ShowLine(line.speaker, null, line.portrait, line.text);
        }

        void Finish()
        {
            End();
            var done = onDone;
            onDone = null;
            done?.Invoke();
        }
    }
}

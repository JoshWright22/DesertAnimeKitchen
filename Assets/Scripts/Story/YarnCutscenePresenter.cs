#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Yarn.Unity;

namespace DessertFactory
{
    // Shows Yarn dialogue in the cutscene player, so .yarn scenes look like the rest of the story
    public class YarnCutscenePresenter : DialoguePresenterBase
    {
        [SerializeField] CutscenePlayer player = null!;
        [SerializeField] DialogueRunner runner = null!;
        [Tooltip("Who can talk. Matched against the name before the colon in a line.")]
        [SerializeField] List<CharacterDef> cast = new List<CharacterDef>();

        // the writers put *stars* around words they want leaned on
        static readonly Regex Emphasis = new Regex(@"\*(.+?)\*");

        void OnEnable()
        {
            player.SkipRequested += Skip;
        }

        void OnDisable()
        {
            player.SkipRequested -= Skip;
        }

        void Skip()
        {
            if (runner.IsDialogueRunning)
                runner.Stop().Forget();
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            player.Begin(null);
            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            player.End();
            return YarnTask.CompletedTask;
        }

        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            var name = line.CharacterName;
            var speaker = cast.Find(c => c != null && c.displayName == name);
            if (speaker != null && !string.IsNullOrEmpty(speaker.alias) && !Revealed(speaker))
                name = speaker.alias;
            player.ShowLine(speaker, name, speaker != null ? speaker.PortraitFor(line.Metadata) : null, Format(line));

            while (!player.LineFinished && !token.IsNextLineRequested)
                await YarnTask.Yield();
        }

        public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken token)
        {
            var available = dialogueOptions.Where(o => o.IsAvailable).ToList();
            int picked = -1;
            player.ShowChoices(available.Select(o => Format(o.Line)).ToList(), choice => picked = choice);

            while (picked < 0 && !token.IsNextLineRequested)
                await YarnTask.Yield();
            return picked < 0 ? null : available[picked];
        }

        bool Revealed(CharacterDef speaker)
        {
            return runner.VariableStorage.TryGetValue<bool>(speaker.revealedBy, out var known) && known;
        }

        static string Format(LocalizedLine line)
        {
            return Emphasis.Replace(line.TextWithoutCharacterName.Text, "<i>$1</i>");
        }
    }
}

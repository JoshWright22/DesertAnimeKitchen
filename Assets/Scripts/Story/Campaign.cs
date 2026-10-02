using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace DessertFactory
{
    // The three weeks: the workday clock, wages, the profit milestones and which story plays when.
    // Dialogue lives in .yarn, this only decides which node to run and hands it the numbers.
    public class Campaign : MonoBehaviour
    {
        public enum Ending
        {
            None,
            Fired,
            Promoted,
            Unionized,
            // promoted or unionized, and you fell for the one you sided with
            Jennifer,
            RedVelvet
        }

        [Serializable]
        public class Milestone
        {
            [Tooltip("Checked when this day ends")]
            public int day = 5;
            public int profit = 1000000;
        }

        [SerializeField] Factory factory;
        [SerializeField] Gacha gacha;
        [SerializeField] DialogueRunner story;
        [SerializeField] CutscenePlayer cutscenes;
        [Tooltip("Watched by the tutorial to see you've learned to move it")]
        [SerializeField] Camera cam;

        [Header("Workday")]
        [Tooltip("Real seconds for one hour on the clock")]
        [SerializeField] float secondsPerHour = 30f;
        [SerializeField] int openingHour = 9;
        [SerializeField] int closingHour = 17;
        [SerializeField] int overtimeUntil = 20;
        [Tooltip("Overtime hours cost this much more than normal ones")]
        [SerializeField] float overtimeRate = 1.25f;

        [Header("Goals")]
        [SerializeField] List<Milestone> milestones = new List<Milestone>
        {
            new Milestone { day = 5, profit = 1000000 },
            new Milestone { day = 10, profit = 2000000 },
            new Milestone { day = 15, profit = 3000000 },
        };
        [Tooltip("Worker affinity she needs, and more than corporate's, for the girls to unionize instead of you getting promoted")]
        [SerializeField] int unionAffinity = 4;
        [Tooltip("Affinity on the winning side for the ending to turn into a love story")]
        [SerializeField] int loveAffinity = 8;

        [Header("Story nodes")]
        [SerializeField] string openingNode = "Opening";
        [Tooltip("Plays at closing time when nobody has agreed to overtime yet")]
        [SerializeField] string closingTimeNode = "Closing_Time";
        [SerializeField] string dayEndNode = "Day_End";
        [SerializeField] string firedNode = "Ending_Fired";
        [SerializeField] string promotedNode = "Ending_Promoted";
        [SerializeField] string unionizedNode = "Ending_Unionized";
        [SerializeField] string jenniferNode = "Ending_Jennifer";
        [SerializeField] string redVelvetNode = "Ending_Red_Velvet";

        public int Day { get; private set; } = 1;
        public float Hour { get; private set; }
        public bool OnTheClock { get; private set; }
        public bool InOvertime => OnTheClock && Hour > closingHour;
        public int ClosingHour => closingHour;
        public int OvertimeUntil => overtimeUntil;
        public Ending Result { get; private set; }

        // What the tutorial is waiting on, shown on the HUD while the dialogue is hidden
        public string Goal { get; private set; }

        // once the day's over the HUD and the story look ahead to the next one
        public Milestone NextMilestone => Result != Ending.None ? null : milestones.Find(m => dayOver ? m.day > Day : m.day >= Day);

        public event Action<Ending> Ended;

        Stockpile Stock => factory.Stockpile;

        GameContent content;
        readonly HashSet<BuildingDef> locked = new HashSet<BuildingDef>();
        bool dayOver;
        bool overtimeToday;
        bool overtimeEveryDay;
        int revenueToday;
        int wagesToday;
        // raises the girls were given, 0.2 is everyone paid a fifth more
        float raise;

        void Awake()
        {
            Hour = openingHour;

            story.AddCommandHandler("overtime", () => overtimeToday = true);
            story.AddCommandHandler<bool>("overtime_every_day", on => overtimeEveryDay = on);
            story.AddCommandHandler<string>("goal", text => Goal = text);
            // the stockpile doesn't exist yet in Awake, so these look it up when they run
            story.AddCommandHandler<int>("give_stars", amount => Stock.AddStars(amount));
            story.AddCommandHandler<int>("spend", amount => Stock.Pay(amount));
            story.AddCommandHandler<float>("raise_wages", amount => raise += amount);
            story.AddCommandHandler<string, int>("wait_for_build", WaitForBuild);
            story.AddCommandHandler<int>("wait_for_sales", count => WaitUntil(() => Stock.DessertsSold >= count));
            story.AddCommandHandler("wait_for_pull", WaitForPull);
            story.AddCommandHandler("wait_for_camera", WaitForCamera);
            story.AddCommandHandler("lock_all", () => locked.UnionWith(content.buildings));
            story.AddCommandHandler("unlock_all", () => locked.Clear());
            story.AddCommandHandler<string>("unlock", building => locked.RemoveWhere(b => b.displayName == building));
        }

        // The tutorial greys out the toolbar and hands buildings over one at a time
        public bool IsLocked(BuildingDef def) => locked.Contains(def);

        // Called once the map and factory are ready
        public void Begin(GameContent content)
        {
            this.content = content;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            // the tutorial runs in here, before the clock starts
            yield return Talk(openingNode);

            while (true)
            {
                Hour = openingHour;
                int revenueBefore = Stock.Revenue;
                yield return Talk($"Day_{Day}_Start");

                yield return Work(closingHour);
                if (!overtimeToday && !overtimeEveryDay)
                    yield return Talk(closingTimeNode);
                bool overtime = overtimeToday || overtimeEveryDay;
                if (overtime)
                    yield return Work(overtimeUntil);

                factory.Working = false;
                dayOver = true;
                revenueToday = Stock.Revenue - revenueBefore;
                PayWages(overtime);
                yield return Talk(dayEndNode);
                yield return Talk($"Day_{Day}_End");

                var milestone = milestones.Find(m => m.day == Day);
                if (milestone != null)
                {
                    if (Stock.Profit < milestone.profit)
                    {
                        yield return Finish(Ending.Fired, firedNode);
                        yield break;
                    }

                    yield return Talk($"Milestone_{milestones.IndexOf(milestone) + 1}");
                    if (milestone == milestones[milestones.Count - 1])
                    {
                        float workers = Affinity("$workers");
                        float corporate = Affinity("$corporate");
                        if (workers >= unionAffinity && workers > corporate)
                            yield return workers >= loveAffinity ? Finish(Ending.RedVelvet, redVelvetNode) : Finish(Ending.Unionized, unionizedNode);
                        else
                            yield return corporate >= loveAffinity ? Finish(Ending.Jennifer, jenniferNode) : Finish(Ending.Promoted, promotedNode);
                        yield break;
                    }
                }

                Day++;
                dayOver = false;
                overtimeToday = false;
            }
        }

        IEnumerator Work(int until)
        {
            factory.Working = true;
            OnTheClock = true;
            while (Hour < until)
            {
                Hour = Mathf.Min(until, Hour + Time.deltaTime / secondsPerHour);
                yield return null;
            }
            OnTheClock = false;
        }

        void PayWages(bool overtime)
        {
            float extraHours = overtimeUntil - closingHour;
            float normalHours = closingHour - openingHour;
            float wages = 0f;
            foreach (var building in factory.Buildings)
            {
                // corporate paid day one up front, only overtime comes out of your pocket
                if (Day > 1)
                    wages += building.Def.wage;
                if (overtime)
                    wages += building.Def.wage * overtimeRate * extraHours / normalHours;
            }

            wagesToday = Mathf.RoundToInt(wages * (1f + raise));
            Stock.Pay(wagesToday);
        }

        IEnumerator Finish(Ending ending, string node)
        {
            Result = ending;
            factory.Working = false;
            yield return Talk(node);
            Ended?.Invoke(ending);
        }

        IEnumerator Talk(string node)
        {
            if (!story.Dialogue.NodeExists(node))
                yield break;

            ShareNumbers();
            story.StartDialogue(node).Forget();
            while (story.IsDialogueRunning)
                yield return null;
        }

        // Everything the writers might want to say out loud, already formatted
        void ShareNumbers()
        {
            var vars = story.VariableStorage;
            var next = NextMilestone;
            vars.SetValue("$day", Day);
            vars.SetValue("$profit", Money(Stock.Profit));
            vars.SetValue("$revenue_today", Money(revenueToday));
            vars.SetValue("$wages_today", Money(wagesToday));
            vars.SetValue("$goal", next != null ? Money(next.profit) : "");
            vars.SetValue("$days_left", next != null ? next.day - Day : 0);
            vars.SetValue("$on_track", next != null && Stock.Profit >= next.profit);
            vars.SetValue("$milestone_today", dayOver && milestones.Exists(m => m.day == Day));
        }

        float Affinity(string variable)
        {
            return story.VariableStorage.TryGetValue<float>(variable, out var value) ? value : 0f;
        }

        public static string Money(int amount)
        {
            return amount < 0 ? $"-${-amount:N0}" : $"${amount:N0}";
        }

        YarnTask WaitForBuild(string building, int count)
        {
            var def = content.buildings.Find(b => b.displayName == building);
            if (def == null)
            {
                Debug.LogWarning($"wait_for_build: no building called {building}");
                return YarnTask.CompletedTask;
            }
            return WaitUntil(() => factory.CountPlaced(def) >= count);
        }

        YarnTask WaitForCamera()
        {
            var start = cam.transform.position;
            float zoom = cam.orthographicSize;
            return WaitUntil(() => Vector2.Distance(cam.transform.position, start) > 3f || Mathf.Abs(cam.orthographicSize - zoom) > 1f);
        }

        YarnTask WaitForPull()
        {
            int pulls = gacha.Pulls;
            return WaitUntil(() => gacha.Pulls > pulls);
        }

        // Hides the dialogue so the player can go and do the thing, then picks it back up
        async YarnTask WaitUntil(Func<bool> done)
        {
            cutscenes.End();
            while (!done() || cutscenes.Playing)
                await YarnTask.Yield();
            Goal = null;
            cutscenes.Begin(null);
        }
    }
}

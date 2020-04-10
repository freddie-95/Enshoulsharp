using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Utility;


namespace Mastery_Badge_Spammer
{
    public static class Program
    {
        public static Menu Menu;
        public static int LastEmoteSpam = 0;
        public static int MyKills = 0;
        public static int MyAssits = 0;
        public static int MyDeaths = 0;
        public static Random Random;
        public static SpellSlot FlashSlot = SpellSlot.Unknown;
        public static SpellSlot IgniteSlot = SpellSlot.Unknown;

        public static string[] KnownDisrespectStarts = new[]
        {
            "", "gj ", "nice ", "wp ", "lol gj ", "nice 1 ", "gg ", "very wp ", "ggwp ", "sweet ", "ty ", "thx ",
            "wow nice ", "lol ", "wow ", "so good ", "heh ", "hah ", "haha ", "hahaha ", "hahahaha ", "u did well ",
            "you did well ", "loved it ", "loved that ", "love u ", "love you ", "ahaha ", "ahahaha "
        };

        public static string[] KnownDisrespectEndings = new[]
        {
            "", " XD", " XDD", " XDDD", " XDDD", "XDDDD", " haha", " hahaha", " hahahaha", " ahaha", " ahahaha", " lol",
            " rofl", " roflmao"
        };

        public static uint LastDeathNetworkId = 0;
        public static int LastChat = 0;
        public static Dictionary<uint, int> DeathsHistory = new Dictionary<uint, int>();

        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad()
        {
            Menu = new Menu("masteryemotespammermenu", "Mastery Emote Spammer", true);
            Menu.Add(new MenuList("mode", "Mode", new[] {"MASTERY", "LAUGH", "DISABLED"}));
            Menu.Add(
                new MenuList("chatdisrespectmode", "Chat Disrespect Mode",
                    new[] {"DISABLED", "CHAMPION NAME", "SUMMONER NAME"}));
            Menu.Add(new MenuBool("onkill", "After Kill").SetValue(true));
            Menu.Add(new MenuBool("onassist", "After Assist").SetValue(true));
            Menu.Add(new MenuBool("ondeath", "After Death").SetValue(false));
            Menu.Add(new MenuBool("neardead", "Near Dead Bodies").SetValue(true));
            Menu.Add(new MenuBool("ondodgedskillshot", "After you dodge a skillshot").SetValue(false));
            Menu.Add(new MenuBool("afterignite", "Dubstep Ignite").SetValue(true));
            Menu.Add(new MenuBool("afterflash", "Challenger Flash").SetValue(false));
            Menu.Add(new MenuBool("afterq", "After Q").SetValue(false));
            Menu.Add(new MenuBool("afterw", "After W").SetValue(false));
            Menu.Add(new MenuBool("aftere", "After E").SetValue(false));
            Menu.Add(new MenuBool("afterr", "After R").SetValue(false));
            Menu.Add(new MenuBool("humanizer", "Use Humanizer?").SetValue(true));
            Menu.Add(new MenuBool("gentlemanmode", "Use GENTLEMAN Pack?").SetValue(true));
            Menu.Add(new MenuBool("zodiacmode", "Use zodiac Pack?").SetValue(true));
            Menu.Add(new MenuBool("myomode", "Use myo Pack?").SetValue(true));
            Menu.Add(new MenuBool("bonobomode", "Use Icy Pack?").SetValue(true));
            Menu.Add(new MenuBool("guccimode", "Use GUCCI Pack?").SetValue(true));
            Menu.Add(new MenuBool("classic", "classic").SetValue(true));
            Menu.Attach();
            Random = new Random();
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            
            Game.OnUpdate += OnUpdate;
            
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;

            //init chat disrespekter
            foreach (var en in ObjectManager.Get<AIHeroClient>().Where(h => h.IsEnemy))
            {
                DeathsHistory.Add(en.NetworkId, en.Deaths);
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var sData = SpellDatabase.GetByName(args.SData.Name);
            if (Menu.GetValue<MenuBool>("ondodgedskillshot") && sender.IsEnemy && sData != null &&
                ObjectManager.Player.Distance(sender) < sData.Range)
            {
                DelayAction.Add(
                    (int) Math.Round(sData.Delay + sender.Distance(ObjectManager.Player)/sData.MissileSpeed), DoEmote);
            }
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.Q && Menu.GetValue<MenuBool>("afterq"))
                {
                    DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (args.Slot == SpellSlot.W && Menu.GetValue<MenuBool>("afterw"))
                {
                    DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (args.Slot == SpellSlot.E && Menu.GetValue<MenuBool>("aftere"))
                {
                    DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (args.Slot == SpellSlot.R && Menu.GetValue<MenuBool>("afterr"))
                {
                    DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (IgniteSlot != SpellSlot.Unknown && args.Slot == IgniteSlot &&
                    Menu.GetValue<MenuBool>("afterignite"))
                {
                    DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
                if (FlashSlot != SpellSlot.Unknown && args.Slot == FlashSlot && Menu.GetValue<MenuBool>("afterflash"))
                {
                    DelayAction.Add(Random.Next(250, 500), DoEmote);
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (MenuGUI.IsChatOpen) return;
            if (ObjectManager.Player.ChampionsKilled > MyKills && Menu.GetValue<MenuBool>("onkill"))
            {
                MyKills = ObjectManager.Player.ChampionsKilled;
                DoEmote();
            }
            if (ObjectManager.Player.Assists > MyAssits && Menu.GetValue<MenuBool>("onassist"))
            {
                MyAssits = ObjectManager.Player.Assists;
                DoEmote();
            }
            if (ObjectManager.Player.Deaths > MyDeaths)
            {
                MyDeaths = ObjectManager.Player.Deaths;
                if (Menu.GetValue<MenuBool>("classic"))
                {
                    Game.Say("classic", true);
                }
                if (Menu.GetValue<MenuBool>("ondeath"))
                {
                    DoEmote();
                }
            }
            if (Menu.GetValue<MenuBool>("neardead") &&
                ObjectManager.Get<AIHeroClient>()
                    .Any(h => h.IsEnemy && h.IsVisible && h.IsDead && ObjectManager.Player.Distance(h) < 300))
            {
                DoEmote();
            }

            switch (Menu.GetValue<MenuList>("chatdisrespectmode").SelectedValue)
            {
                case "DISABLED":
                    break;
                case "CHAMPION NAME":
                    foreach (var en in ObjectManager.Get<AIHeroClient>().Where(h => h.IsEnemy))
                    {
                        if (DeathsHistory.FirstOrDefault(record => record.Key == en.NetworkId).Value < en.Deaths)
                        {
                            var championName = en.CharacterName.ToLower();
                            DeathsHistory.Remove(en.NetworkId);
                            DeathsHistory.Add(en.NetworkId, en.Deaths);
                            if (en.Distance(ObjectManager.Player) < 2000)
                            {
                                DelayAction.Add(Random.Next(1000, 5000), () => DoChatDisrespect(championName));
                            }
                        }
                    }
                    break;
                case "SUMMONER NAME":
                    foreach (var en in ObjectManager.Get<AIHeroClient>().Where(h => h.IsEnemy))
                    {
                        if (DeathsHistory.FirstOrDefault(record => record.Key == en.NetworkId).Value < en.Deaths)
                        {
                            var name = en.Name.ToLower();
                            DeathsHistory.Remove(en.NetworkId);
                            DeathsHistory.Add(en.NetworkId, en.Deaths);
                            if (en.Distance(ObjectManager.Player) < 2000)
                            {
                                DelayAction.Add(Random.Next(1000, 5000), () => DoChatDisrespect(name));
                            }
                        }
                    }
                    break;
            }
        }

        public static void DoEmote()
        {
            if (Variables.GameTimeTickCount - LastEmoteSpam > Random.Next(5000, 15000))
            {
                LastEmoteSpam = Variables.GameTimeTickCount;
                var mode = Menu.GetValue<MenuList>("mode").SelectedValue;
                if (mode == "DISABLED") return;
                Game.SendMasteryBadge();
            }
        }

        public static void DoChatDisrespect(string theTarget)
        {
            if (Variables.GameTimeTickCount - LastChat > Random.Next(5000, 20000) ||
                !Menu.GetValue<MenuBool>("humanizer"))
            {
                LastChat = Variables.GameTimeTickCount;
                switch (Random.Next(0, 4))
                {
                    case 0:
                    {
                        if (Menu.GetValue<MenuBool>("gentlemanmode"))
                        {
                            switch (Random.Next(0, 10))
                            {
                                case 0:
                                    Game.Say(
                                        String.Format(
                                            "What’s the difference between {0} and eggs? Eggs get laid and {0} doesn't.",
                                            theTarget), true);
                                    return;
                                case 1:
                                    Game.Say(
                                        String.Format(
                                            "{0}'s birth certificate is an apology letter from the condom factory.",
                                            theTarget), true);
                                    return;
                                case 2:
                                    Game.Say(
                                        String.Format(
                                            "{0} must have been born on a highway because that's where most accidents happen.",
                                            theTarget), true);
                                    return;
                                case 3:
                                    Game.Say(
                                        String.Format(
                                            "If I wanted to kill myself I'd climb your ego and jump to your skill {0}",
                                            theTarget), true);
                                    return;
                                case 4:
                                    Game.Say(
                                        String.Format(
                                            "Roses are red violets are blue, God made me great, the opposite of you. ",
                                            theTarget), true);
                                    return;
                                case 5:
                                    Game.Say(
                                        String.Format(
                                            "{0} you are so useless I would unplug your life support to charge my phone.",
                                            theTarget), true);
                                    return;
                                case 6:
                                    Game.Say(String.Format("You are a disgrace to your family {0}", theTarget), true);
                                    return;
                                case 7:
                                    Game.Say(
                                        String.Format(
                                            "Somewhere out there is a tree, tirelessly producing oxygen so you can breathe. I think you owe it an apology {0}.",
                                            theTarget), true);
                                    return;
                                case 8:
                                    Game.Say(
                                        String.Format(
                                            "You are so bad Riot will bring back the unskilled report option {0}.",
                                            theTarget), true);
                                    return;
                                case 9:
                                    Game.Say(String.Format("My deepest condolences, {0}.", theTarget), true);
                                    return;
                                case 10:
                                    Game.Say(
                                        String.Format(
                                            "Congratulations {0}, you have been recognized as one of the worst players in League of Legends. You're a downgrading force whose plays inspire even the worst teammates to even worse accomplishments.",
                                            theTarget), true);
                                    return;

                            }
                        }
                        break;
                    }
                    case 1:
                    {
                        if (Menu.GetValue<MenuBool>("zodiacmode"))
                        {
                            switch (Random.Next(0, 15))
                            {
                                case 0:
                                    Game.Say(
                                        String.Format(
                                            "I don't know what techniques you are doing there {0} , but... keep doing them!",
                                            theTarget), true);
                                    return;
                                case 1:
                                    Game.Say(
                                        String.Format(
                                            "If you don't stop using your abilities like a monkey {0}, this game ain't get better!",
                                            theTarget), true);
                                    return;
                                case 2:
                                    Game.Say(String.Format("How does it feel to be retarded {0} ?",
                                        theTarget), true);
                                    return;
                                case 3:
                                    Game.Say(
                                        String.Format(
                                            "Is it just in League {0}, or are you acting like a handicapped fish everywhere?",
                                            theTarget), true);
                                    return;
                                case 4:
                                    Game.Say(
                                        String.Format(
                                            "Because of players like you {0}, riot will change the surrender time to 10 minutes soon.",
                                            theTarget), true);
                                    return;
                                case 5:
                                    Game.Say(
                                        String.Format(
                                            "HAHA for a second I thought you stopped trolling {0}",
                                            theTarget), true);
                                    return;
                                case 6:
                                    Game.Say(
                                        String.Format(
                                            "We are currently experimenting with monkeys playing League in a team, we need one more player - are you interested {0} ?",
                                            theTarget), true);
                                    return;
                                case 7:
                                    Game.Say(
                                        String.Format(
                                            "After this {0}, I will NEVER EVER call Kaceytron a troll again.",
                                            theTarget), true);
                                    return;
                                case 8:
                                    Game.Say(
                                        String.Format(
                                            "You must have been hammering your head on the wall while playing league {0}",
                                            theTarget), true);
                                    return;
                                case 9:
                                    Game.Say(
                                        String.Format(
                                            "Even with a steering wheel you can't play like that, tell me the trick {0}!", //WTF SORTA ENGLISH IS THAT?!
                                            theTarget), true);
                                    return;
                                case 10:
                                    Game.Say(String.Format("What drug can cause those mental issues {0}?",
                                        theTarget), true);
                                    return;
                                case 11:
                                    Game.Say(
                                        String.Format(
                                            "I had a dream how someone tried to play league by sitting with his booty on his keyboard, was that you {0} ?",
                                            theTarget), true);
                                    return;
                                case 12:
                                    Game.Say(
                                        String.Format(
                                            "WP {0}, that was actually spastic enough to create a youtube video of it",
                                            theTarget), true);
                                    return;
                                case 13:
                                    Game.Say(
                                        String.Format(
                                            "This is not League of Retards, you downloaded the wrong game {0}.",
                                            theTarget), true);
                                    return;
                                case 14:
                                    Game.Say(
                                        String.Format(
                                            "I wonder how you haven't gotten hit by a car yet {0} with this decision making!",
                                            theTarget), true);
                                    return;
                                case 15:
                                    Game.Say(String.Format("What kind of complexes do you have {0} ?",
                                        theTarget), true);
                                    return;
                            }
                        }
                        break;
                    }

                    case 2:
                    {
                        if (Menu.GetValue<MenuBool>("myomode"))
                        {
                            switch (Random.Next(0, 29))
                            {
                                case 0:
                                    Game.Say(String.Format("come on {0} atleast try", theTarget), true);
                                    return;
                                case 1:
                                    Game.Say(String.Format("you're boring me {0}", theTarget), true);
                                    return;
                                case 2:
                                    Game.Say(
                                        String.Format(
                                            "you know {0}.. you're so bad that I'm gonna open a support ticket for you",
                                            theTarget), true);
                                    return;
                                case 3:
                                    Game.Say(String.Format("my god {0} are you boosted or smth ROFLMAO",
                                        theTarget), true);
                                    return;
                                case 4:
                                    Game.Say(String.Format("{0} reminds me of trick2g bronze subwars",
                                        theTarget), true);
                                    return;
                                case 5:
                                    Game.Say(
                                        String.Format("my god this {0} guy is such a god.. at being bad",
                                            theTarget), true);
                                    return;
                                case 6:
                                    Game.Say(String.Format("is {0} a bot guys?", theTarget), true);
                                    return;
                                case 7:
                                    Game.Say(String.Format("you remind me of intro bots {0}", theTarget), true);
                                    return;
                                case 8:
                                    Game.Say(String.Format("your stupidity knows no boundaries {0}",
                                        theTarget), true);
                                    return;
                                case 9:
                                    Game.Say(String.Format("wp {0}! (jk that was soo EZreal)", theTarget), true);
                                    return;
                                case 10:
                                    Game.Say(String.Format("thanks for the free LP {0}", theTarget), true);
                                    return;
                                case 11:
                                    Game.Say(String.Format("haha this {0} is so troll", theTarget), true);
                                    return;
                                case 12:
                                    Game.Say(
                                        String.Format(
                                            "{0} is trolling no way someone can be this bad ROFL",
                                            theTarget), true);
                                    return;
                                case 13:
                                    Game.Say(String.Format("? {0} ???", theTarget), true);
                                    return;
                                case 14:
                                    Game.Say(String.Format("I feel so bad for owning {0}", theTarget), true);
                                    return;
                                case 15:
                                    Game.Say(String.Format(
                                        "sorry {0} I know it's unfair for me to play against you...",
                                        theTarget), true);
                                    return;
                                case 16:
                                    Game.Say(String.Format("how much did the boost cost {0}", theTarget), true);
                                    return;
                                case 17:
                                    Game.Say(
                                        String.Format(
                                            "I'm pretty sure that if monkeys would play league they'd do better than you {0}",
                                            theTarget), true);
                                    return;
                                case 18:
                                    Game.Say(String.Format("dude {0} I'm not even trying ROFL", theTarget), true);
                                    return;
                                case 19:
                                    Game.Say(String.Format("{0}.. you're such a fool man...", theTarget), true);
                                    return;
                                case 20:
                                    Game.Say(
                                        String.Format("add me after the game {0} I'll teach u how to play",
                                            theTarget), true);
                                    return;
                                case 21:
                                    Game.Say(String.Format(
                                        "my god {0} just go afk.. you're dragging your team down...",
                                        theTarget), true);
                                    return;
                                case 22:
                                    Game.Say(
                                        String.Format(
                                            "{0} the legend coming back once again with the gold for his daddy",
                                            theTarget), true);
                                    return;
                                case 23:
                                    Game.Say(String.Format("I'm going straight to the bank with this {0}",
                                        theTarget), true);
                                    return;
                                case 24:
                                    Game.Say(String.Format("ty {0} I really needed this gold", theTarget), true);
                                    return;
                                case 25:
                                    Game.Say(
                                        String.Format(
                                            "Please don't report {0} it's not his fault he has to play against me..",
                                            theTarget), true);
                                    return;
                                case 26:
                                    Game.Say("open mid?", true);
                                    return;
                                case 27:
                                    Game.Say("? Kappa?", true);
                                    return;
                                case 28:
                                    Game.Say("ff?", true);
                                    return;
                                case 29:
                                    Game.Say("surrender?", true);
                                    return;
                            }
                        }
                        break;
                    }

                    case 3:
                    {
                        if (Menu.GetValue<MenuBool>("bonobomode"))
                        {
                            switch (Random.Next(0, 9))
                            {
                                case 0:
                                    Game.Say(String.Format("{0} You're honestly trash", theTarget), true);
                                    return;
                                case 1:
                                    Game.Say(String.Format("Jaja {0}, try again", theTarget), true);
                                    return;
                                case 2:
                                    Game.Say(String.Format("{0} Jajajajajajajajajajajajajajajajajaja",
                                        theTarget), true);
                                    return;
                                case 3:
                                    Game.Say(String.Format("Thanks for the free gold {0}", theTarget), true);
                                    return;
                                case 4:
                                    Game.Say(String.Format("{0} Go and download scripts, you suck!",
                                        theTarget), true);
                                    return;
                                case 5:
                                    Game.Say(String.Format("{0} That was easy", theTarget), true);
                                    return;
                                case 6:
                                    Game.Say(String.Format("{0} are you okay?", theTarget), true);
                                    return;
                                case 7:
                                    Game.Say(String.Format("{0} It amazes me how someone can be so trash",
                                        theTarget), true);
                                    return;
                                case 8:
                                    Game.Say(
                                        String.Format(
                                            "{0} You lost that fight harder than germany lost the war",
                                            theTarget), true);
                                    return;
                                case 9:
                                    Game.Say(String.Format("{0} Can you stop feeding?", theTarget), true);
                                    return;
                            }
                        }
                        break;
                    }
                    case 4:
                    {
                        if (Menu.GetValue<MenuBool>("guccimode"))
                        {
                            switch (Random.Next(0, 15))
                            {
                                case 0:
                                    Game.Say(String.Format("HAHA {0} that was a refreshing experience!",
                                        theTarget), true);
                                    return;
                                case 1:
                                    Game.Say(String.Format("LOL {0} no match for me!", theTarget), true);
                                    return;
                                case 2:
                                    Game.Say(String.Format("Fantastic performance right there {0}!",
                                        theTarget), true);
                                    return;
                                case 3:
                                    Game.Say(String.Format("Can't touch this {0}", theTarget), true);
                                    return;
                                case 4:
                                    Game.Say(String.Format("{0}, you have been reformed!", theTarget), true);
                                    return;
                                case 5:
                                    Game.Say(String.Format("Completely smashed there {0}", theTarget), true);
                                    return;
                                case 6:
                                    Game.Say(String.Format("haha pathetic {0}", theTarget), true);
                                    return;
                                case 7:
                                    Game.Say(String.Format("true display of skill {0}", theTarget), true);
                                    return;
                                case 8:
                                    Game.Say(String.Format("better luck next time {0}", theTarget), true);
                                    return;
                                case 9:
                                    Game.Say(String.Format("Nice try for a monkey {0}", theTarget), true);
                                    return;
                                case 10:
                                    Game.Say(
                                        String.Format(
                                            "I see you've set aside this special time to humiliate yourself in public {0}",
                                            theTarget), true);
                                    return;
                                case 11:
                                    Game.Say(String.Format(" Who lit the fuse on your tampon {0}?",
                                        theTarget), true);
                                    return;
                                case 12:
                                    Game.Say(
                                        String.Format(
                                            "I like you {0}. You remind me of myself when I was young and stupid. ",
                                            theTarget), true);
                                    return;
                                case 13:
                                    Game.Say(
                                        String.Format(
                                            " {0}, I'll try being nicer if you'll try being more intelligent.",
                                            theTarget), true);
                                    return;
                                case 14:
                                    Game.Say(
                                        String.Format(
                                            "{0}, if you have something to say raise your hand... then place it over your mouth. ",
                                            theTarget), true);
                                    return;
                                case 15:
                                    Game.Say(
                                        String.Format(
                                            "Somewhere out there is a tree, tirelessly producing oxygen so you can breathe. I think you owe it an apology, {0}",
                                            theTarget), true);
                                    return;
                            }
                        }
                        break;
                    }
                }
                Game.Say(KnownDisrespectStarts[Random.Next(0, KnownDisrespectStarts.Length - 1)] +
                         (Random.Next(1, 2) == 1 ? theTarget : "") +
                         KnownDisrespectEndings[Random.Next(0, KnownDisrespectEndings.Length - 1)], true);
            }
        }
    }


}

using HarmonyLib;
using AmongUs.Data;
using System;
using System.Text;

namespace SkidMenu.features
{
    public static class NameSpoofer
    {
        public static bool Enabled = false;
        public static string SpoofedName = "";
        public static int RandomLength = 10;
        public static RandomizerMode Mode = RandomizerMode.RandomWords;

        private static string _originalName = "";

        public enum RandomizerMode
        {
            RandomString,
            RandomWords,
            SpaceThemed,
            AmongUsThemed,
            Leetspeak,
            Zalgo,
            RepeatingPattern,
            FakeTag,
            NumbersOnly,
            GamerTag,
            CursedMix
        }

        private static readonly string[] Adjectives =
        {
            "Red", "Blue", "Sussy", "Dark", "Loud", "Fast", "Cold", "Wild",
            "Tiny", "Mad", "Cool", "Bold", "Slim", "Pale", "Hot", "Odd",
            "Blank", "Blunt", "Sharp", "Rusty", "Dusty", "Fuzzy", "Dizzy",
            "Toxic", "Sneaky", "Silent", "Rogue", "Swift", "Numb", "Void",
            "Primal", "Grim", "Hollow", "Rotten", "Broken", "Neon", "Shady",
            "Cursed", "Warped", "Slick", "Gloomy", "Blaze", "Frosty", "Hyper",
            "Lazy", "Stiff", "Murky", "Crimson", "Ashen", "Wicked", "Static"
        };

        private static readonly string[] Nouns =
        {
            "Crewmate", "Impostor", "Ghost", "Vent", "Task", "Lobby", "Admin",
            "Reactor", "Oxygen", "Lights", "Nerd", "Rat", "Lurker", "Proxy",
            "Bean", "Hat", "Pet", "Skin", "Vote", "Body", "Ejector", "Caller",
            "Hacker", "Griefer", "Skipper", "Camper", "Drifter", "Menace",
            "Stalker", "Phantom", "Bandit", "Glitch", "Scamp", "Wraith",
            "Degen", "Goblin", "Rogue", "Clown", "Chaos", "Void", "Specter",
            "Gremlin", "Rascal", "Freak", "Nuker", "Raider", "Prowler", "Fiend"
        };

        private static readonly string[] SpaceWords =
        {
            "Nova", "Pulsar", "Quasar", "Nebula", "Comet", "Orbit", "Void",
            "Axion", "Photon", "Zenith", "Lunar", "Solar", "Astral", "Cosmo",
            "Vega", "Lyra", "Orion", "Cygnus", "Draco", "Hydra", "Phoebe",
            "Helios", "Titan", "Callisto", "Europa", "Ganymede", "Oberon",
            "Triton", "Charon", "Pluto", "Eris", "Sirius", "Rigel", "Altair",
            "Antares", "Arcturus", "Betelgeuse", "Cassini", "Hubble", "Kepler",
            "Aether", "Celeste", "Eclipse", "Solaris", "Interstellar", "Warp"
        };

        private static readonly string[] AUNames =
        {
            "NotSus", "InnoCent", "TrustMe", "WhoMe", "JustTask",
            "InElec", "EjectMe", "MissVent", "NoWitness", "FakeName",
            "Alt_Acc", "Lurking", "WasAfk", "NotIt", "SkipMe",
            "WasInNav", "DidMyTask", "NotTheImp", "PlsSkip", "ImCrew",
            "JustFixed", "InReactor", "WasCaming", "Innocent", "TrustBro",
            "NeverVent", "WasWith", "ISwear", "DefinitelyNot", "JustABean",
            "NotHacking", "GoodPlayer", "NoKillCD", "CleanHands", "TaskDone",
            "ClearMe", "NotSussy", "OkSus", "ActuallyReal", "LegitPlayer"
        };

        private static readonly string[] FakeTags =
        {
            "YT", "PRO", "MOD", "DEV", "ADMIN", "VIP", "STAFF", "BOT",
            "OWNER", "HACK", "GOD", "NULL", "ROOT", "SUS", "HOST",
            "REAL", "OG", "ALT", "MAIN", "MVP", "AIM", "ESP", "RAT"
        };

        private static readonly string AlphaNum = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static readonly string[] ZalgoChars =
        {
            "\u0300", "\u0301", "\u0302", "\u0303", "\u0308", "\u030A",
            "\u0330", "\u0331", "\u0332", "\u0333", "\u0334"
        };

        private static readonly string[] LeetFrom = { "a", "e", "i", "o", "s", "t", "l", "g" };
        private static readonly string[] LeetTo   = { "4", "3", "1", "0", "5", "7", "1", "9" };

        private static readonly Random _rng = new();

        public static string Generate()
        {
            string name = Mode switch
            {
                RandomizerMode.RandomString     => GenRandomString(),
                RandomizerMode.RandomWords      => GenRandomWords(),
                RandomizerMode.SpaceThemed      => GenSpaceThemed(),
                RandomizerMode.AmongUsThemed    => GenAUThemed(),
                RandomizerMode.Leetspeak        => GenLeetspeak(),
                RandomizerMode.Zalgo            => GenZalgo(),
                RandomizerMode.RepeatingPattern => GenRepeatingPattern(),
                RandomizerMode.FakeTag          => GenFakeTag(),
                RandomizerMode.NumbersOnly      => GenNumbersOnly(),
                RandomizerMode.GamerTag         => GenGamerTag(),
                RandomizerMode.CursedMix        => GenCursedMix(),
                _                               => GenRandomString()
            };

            return name.Length > RandomLength ? name[..RandomLength] : name;
        }

        private static string GenRandomString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < RandomLength; i++)
                sb.Append(AlphaNum[_rng.Next(AlphaNum.Length)]);
            return sb.ToString();
        }

        private static string GenRandomWords()
        {
            string adj  = Adjectives[_rng.Next(Adjectives.Length)];
            string noun = Nouns[_rng.Next(Nouns.Length)];
            return adj + noun;
        }

        private static string GenSpaceThemed()
        {
            string word = SpaceWords[_rng.Next(SpaceWords.Length)];
            return word + _rng.Next(10, 100);
        }

        private static string GenAUThemed()
        {
            return AUNames[_rng.Next(AUNames.Length)];
        }

        private static string GenLeetspeak()
        {
            string word = GenRandomWords().ToLower();
            for (int i = 0; i < LeetFrom.Length; i++)
                word = word.Replace(LeetFrom[i], LeetTo[i]);
            return word;
        }

        private static string GenZalgo()
        {
            int baseLen = Math.Max(2, RandomLength / 2);
            var sb = new StringBuilder();
            for (int i = 0; i < baseLen && sb.Length < RandomLength; i++)
            {
                sb.Append(AlphaNum[_rng.Next(26)]);
                if (sb.Length < RandomLength)
                    sb.Append(ZalgoChars[_rng.Next(ZalgoChars.Length)]);
            }
            return sb.ToString();
        }

        private static string GenRepeatingPattern()
        {
            int patLen = _rng.Next(2, 4);
            var sb = new StringBuilder();
            for (int i = 0; i < patLen; i++)
                sb.Append(AlphaNum[_rng.Next(AlphaNum.Length)]);
            string pat = sb.ToString();

            var result = new StringBuilder();
            while (result.Length + pat.Length <= RandomLength)
                result.Append(pat);
            return result.Length == 0 ? pat[..1] : result.ToString();
        }

        private static string GenFakeTag()
        {
            string tag = FakeTags[_rng.Next(FakeTags.Length)];
            string adj = Adjectives[_rng.Next(Adjectives.Length)];
            return tag + adj;
        }

        private static string GenNumbersOnly()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < RandomLength; i++)
                sb.Append(_rng.Next(0, 10));
            return sb.ToString();
        }

        private static string GenGamerTag()
        {
            string adj  = Adjectives[_rng.Next(Adjectives.Length)];
            string noun = Nouns[_rng.Next(Nouns.Length)];
            string word = adj + noun;
            string num  = _rng.Next(1, 999).ToString();
            string[] patterns =
            {
                $"xX{word}Xx",
                $"_{word}_",
                $"{word}{num}",
                $"ii{word}ii",
                $"{word}_{num}",
                $"[{word}]",
            };
            return patterns[_rng.Next(patterns.Length)];
        }

        private static string GenCursedMix()
        {
            string word = GenRandomWords();
            var sb = new StringBuilder();
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                int roll = _rng.Next(3);
                if (roll == 0) sb.Append(char.ToUpper(c));
                else if (roll == 1) sb.Append(char.ToLower(c));
                else sb.Append(c);
            }
            return sb.ToString();
        }

        public static void ApplyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!Enabled && _originalName == "")
                _originalName = DataManager.Player.Customization.Name ?? "";
            SpoofedName = name.Length > 10 ? name[..10] : name;
            Enabled = true;
            DataManager.Player.Customization.Name = SpoofedName;
        }

        public static void Disable()
        {
            Enabled = false;
            DataManager.Player.Customization.Name = _originalName;
            _originalName = "";
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class PlayerControl_FixedUpdate_NameEnforce
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (!Enabled || string.IsNullOrEmpty(SpoofedName)) return;
                if (__instance != PlayerControl.LocalPlayer) return;
                if (DataManager.Player.Customization.Name != SpoofedName)
                    DataManager.Player.Customization.Name = SpoofedName;
            }
        }
    }
}

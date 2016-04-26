using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EloBuddy;
using EloBuddy.SDK;

namespace SimpleRyze
{
    public static class Helpers
    {
        /// <summary>
        /// Last message tick
        /// </summary>
        public static float LastMessageTick;

        /// <summary>
        /// Last Message String
        /// </summary>
        public static string LastMessageString;

        /// <summary>
        /// Female champions list
        /// </summary>
        public static List<string> FemaleChampions = new List<string>
        {
            "Ahri", "Akali", "Anivia", "Annie",
            "Ashe", "Caitlyn", "Cassiopeia", "Diana",
            "Elise", "Evelynn", "Fiora", "Illaoi",
            "Irelia", "Janna", "Jinx", "Kalista",
            "Karma", "Katarina", "Kayle", "Kindred",
            "Leblanc", "Leona", "Lissandra", "Lulu",
            "Lux", "MissFortune", "Morgana", "Nami",
            "Nidalee", "Orianna", "Poppy", "Quinn",
            "RekSai", "Riven", "Sejuani", "Shyvana",
            "Sivir", "Sona", "Soraka", "Syndra",
            "Tristana", "Vayne",  "Vi", "Zyra"
        };

        public static bool IsPassiveCharged => Player.Instance.GetBuff("RyzePassiveCharged") != null;
        public static int GetPassiveStacks => Player.Instance.GetBuff("RyzePassiveStack") != null ? Player.Instance.GetBuff("RyzePassiveStack").Count : 0;
        public static bool HasRyzeRBuff => Player.Instance.GetBuff("RyzeR") != null;

        public static SpellSlot GetTearSpellSlot
        {
            get
            {
                if (Player.Instance.GetSpellSlotFromName("ArchAngelsDummySpell") != SpellSlot.Unknown)
                    return Player.Instance.Spellbook.GetSpell(Player.Instance.GetSpellSlotFromName("ArchAngelsDummySpell")).Slot;

                return Player.Instance.GetSpellSlotFromName("TearsDummySpell") != SpellSlot.Unknown ? Player.Instance.Spellbook.GetSpell(Player.Instance.GetSpellSlotFromName("TearsDummySpell")).Slot : SpellSlot.Unknown;
            }
        }

        public static bool IsTearReady
        {
            get
            {
                if (GetTearSpellSlot != SpellSlot.Unknown)
                {
                    return Player.Instance.Spellbook.GetSpell(GetTearSpellSlot).State == SpellState.Surpressed;
                }
                return false;
            }
        }

        public static bool IsTearStacked
        {
            get
            {
                if (GetTearSpellSlot != SpellSlot.Unknown)
                {
                    return Player.Instance.Spellbook.GetSpell(GetTearSpellSlot).CooldownExpires + 120 > Game.Time;
                }
                return true;
            }
        }


        public static int QMana = 40;
        public static int[] WMana = { 0, 60, 70, 80, 90, 100 };
        public static int[] EMana = { 0, 60, 70, 80, 90, 100 };

        public static void PrintInfoMessage(string message, bool possibleFlood = true)
        {
            if (possibleFlood && LastMessageTick + 500 > Game.Time * 1000 && LastMessageString == message)
                return;
            
            if (FemaleChampions.Any(key => message.Contains(key)))
            {
                message = Regex.Replace(message, "him", "her", RegexOptions.IgnoreCase);
                message = Regex.Replace(message, "his", "hers", RegexOptions.IgnoreCase);
            }

            LastMessageTick = Game.Time * 1000;
            LastMessageString = message;

            Chat.Print("<font color=\"#DA37F0\">[<b>SIMPLE RYZE</b>]</font> " + message + "</font>");
        }

        public static bool IsTargetRooted(this Obj_AI_Base unit)
        {
            return unit.HasBuff("RyzeW");
        }

        public static bool TargetHasSpellShield(this Obj_AI_Base unit)
        {
            return unit.HasBuffOfType(BuffType.SpellImmunity) || unit.HasBuffOfType(BuffType.SpellShield);
        }
    }
}
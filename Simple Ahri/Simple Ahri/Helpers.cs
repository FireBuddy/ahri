using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EloBuddy;

namespace SimpleAhri
{
    class Helpers
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

        public static int[] QMana = { 0, 65, 70, 75, 80, 85 };
        public static int WMana = 50;
        public static int EMana = 85;
        public static int RMana = 100;

        public static void PrintInfoMessage(string message, bool possibleFlood = true)
        {
            if (FemaleChampions.Any(key => message.Contains(key)))
            {
                message = Regex.Replace(message, "him", "her", RegexOptions.IgnoreCase);
                message = Regex.Replace(message, "his", "hers", RegexOptions.IgnoreCase);
            }

            if (possibleFlood && LastMessageTick + 500 > Game.Time * 1000 && LastMessageString == message)
                return;

            LastMessageTick = Game.Time * 1000;
            LastMessageString = message;

            Chat.Print("<font color=\"#8CD419\">[<b>SIMPLE AHRI</b>]</font> " + message + "</font>");
        }
    }
}

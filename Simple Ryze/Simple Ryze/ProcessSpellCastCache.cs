using EloBuddy;
using EloBuddy.SDK.Enumerations;

namespace SimpleRyze
{
    /// <summary>
    /// Class for easier List ussage
    /// </summary>
    public class ProcessSpellCastCache
    {
        /// <summary>
        /// Game*1000
        /// </summary>
        public int Tick;

        /// <summary>
        /// Sender
        /// </summary>
        public AIHeroClient Sender;

        /// <summary>
        /// Sender's NetworkID
        /// </summary>
        public int NetworkId;

        /// <summary>
        /// Danger Level
        /// </summary>
        public DangerLevel DangerLevel;
    }
}

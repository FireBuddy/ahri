using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace SimpleAhri
{
    public static class SpellManager
    {
        public static Spell.Skillshot EFlash { get; private set; }
        public static Spell.Skillshot Q { get; private set; }
        public static Spell.Active W { get; private set; }
        public static Spell.Skillshot E { get; private set; }
        public static Spell.Skillshot R { get; private set; }

        static SpellManager()
        {
            EFlash = new Spell.Skillshot(SpellSlot.E, 1350, SkillShotType.Linear, 250, 1500, 100)
            {
                AllowedCollisionCount = 0
            };
            Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1500, 125)
            {
                AllowedCollisionCount = int.MaxValue
            };
            W = new Spell.Active(SpellSlot.W, 580);
            E = new Spell.Skillshot(SpellSlot.E, 970, SkillShotType.Linear, 250, 1500, 100)
            {
                AllowedCollisionCount = 0
            };
            R = new Spell.Skillshot(SpellSlot.R, 400, SkillShotType.Linear);
        }

        public static void Initialize()
        {

        }
    }
}

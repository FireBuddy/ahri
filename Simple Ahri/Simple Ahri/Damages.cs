using EloBuddy;
using EloBuddy.SDK;

namespace SimpleAhri
{
    static class Damages
    {
        public static int[] RawQDamage = { 0, 40, 65, 90, 115, 140 };
        public static float QScaling = 0.35f;

        public static int[] RawWDamage = { 0, 40, 65, 90, 115, 140 };
        public static float WScaling = 0.4f;
        public static float WSecoundAndThirdHitMultiplier = 0.3f;

        public static int[] RawEDamage = { 0, 60, 95, 130, 165, 200 };
        public static float EScaling = 0.5f;

        public static int[] RawRDamage = { 0, 70, 110, 150 };
        public static float RScaling = 0.3f;

        public static float GetQDamage(this Obj_AI_Base unit, bool both = false)
        {
            if (!SpellManager.Q.IsLearned)
                return 0;

            var damage = RawQDamage[SpellManager.Q.Level] + Player.Instance.GetTotalAP() * QScaling;

            return both ? Player.Instance.CalculateDamageOnUnit(unit, DamageType.Magical, damage) + damage : Player.Instance.CalculateDamageOnUnit(unit, DamageType.Magical, damage);
        }

        public static float GetWDamage(this Obj_AI_Base unit, bool all = false)
        {
            if (!SpellManager.W.IsLearned)
                return 0;

            var damage = RawWDamage[SpellManager.W.Level] + Player.Instance.GetTotalAP() * WScaling;

            if (all)
            {
                var secound = damage * WSecoundAndThirdHitMultiplier;
                damage = damage + secound *2;
            }

            return unit.CalculateDamageOnUnit(unit, DamageType.Magical, damage);
        }

        public static float GetEDamage(this Obj_AI_Base unit)
        {
            if (!SpellManager.E.IsLearned)
                return 0;

            var damage = RawEDamage[SpellManager.E.Level] +  Player.Instance.GetTotalAP() * EScaling;

            return Player.Instance.CalculateDamageOnUnit(unit, DamageType.Magical, damage);
        }

        public static float GetRDamage(this Obj_AI_Base unit, bool all = false)
        {
            if (!SpellManager.R.IsLearned)
                return 0;

            var damage = RawRDamage[SpellManager.R.Level] + Player.Instance.GetTotalAP() * RScaling;

            return Player.Instance.CalculateDamageOnUnit(unit, DamageType.Magical, all ? damage * 3 : damage);
        }

        public static float GetComboDamage(this Obj_AI_Base unit)
        {
            var damage = 0f;
            var mana = Player.Instance.Mana;

            if (SpellManager.Q.IsReady() && mana > Helpers.QMana[SpellManager.Q.Level])
            {
                mana -= Helpers.QMana[SpellManager.Q.Level];
                damage += unit.GetQDamage(true);
            }

            if (SpellManager.W.IsReady() && mana >= Helpers.WMana)
            {
                mana -= Helpers.WMana;
                damage += unit.GetWDamage(true);
            }

            if (SpellManager.E.IsReady() && mana >= Helpers.EMana)
            {
                mana -= Helpers.EMana;
                damage += unit.GetEDamage();
            }

            if (SpellManager.R.IsReady() && mana >= Helpers.RMana)
            {
                damage += unit.GetRDamage(true);
            }

            return damage;
        }

        // ReSharper disable once InconsistentNaming
        public static float GetTotalAP(this AIHeroClient unit)
        {
            return Program.Rabadon.IsOwned()
                ? Player.Instance.TotalMagicalDamage + Player.Instance.TotalMagicalDamage*0.35f
                : Player.Instance.TotalMagicalDamage;
        }
    }
}
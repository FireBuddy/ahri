using EloBuddy;
using EloBuddy.SDK;

namespace SimpleRyze
{
    static class Damages
    {
        public static int[] RawQDamage = { 0, 60, 95, 130, 165, 200 };
        // ReSharper disable once InconsistentNaming
        public static float QAPScaling = 0.55f;
        public static float[] QManaScaling = { 0, 0.02f, 0.025f, 0.03f, 0.035f, 0.04f };

        public static int[] RawWDamage = { 0, 80, 100, 120, 140, 160 };
        // ReSharper disable once InconsistentNaming
        public static float WAPScaling = 0.4f;
        public static float WManaScaling = 0.025f;

        public static int[] RawEDamage = { 0, 36, 52, 68, 84, 100 };
        // ReSharper disable once InconsistentNaming
        public static float EAPScaling = 0.2f;
        public static float EManaScaling = 0.02f;
        
        public static float RScaling = 0.5f;

        public static float GetQDamage(this Obj_AI_Base unit)
        {
            if (!SpellManager.Q.IsLearned)
                return 0;

            var damage = RawQDamage[SpellManager.Q.Level] + Player.Instance.GetTotalAP() * QAPScaling + Player.Instance.MaxMana * QManaScaling[SpellManager.Q.Level];

            return Player.Instance.CalculateDamageOnUnit(unit, DamageType.Magical, damage);
        }

        public static float GetWDamage(this Obj_AI_Base unit)
        {
            if (!SpellManager.W.IsLearned)
                return 0;

            var damage = RawWDamage[SpellManager.W.Level] + Player.Instance.GetTotalAP() * WAPScaling + Player.Instance.MaxMana * WManaScaling;

            return unit.CalculateDamageOnUnit(unit, DamageType.Magical, damage);
        }

        public static float GetEDamage(this Obj_AI_Base unit)
        {
            if (!SpellManager.E.IsLearned)
                return 0;

            var damage = RawEDamage[SpellManager.E.Level] +  Player.Instance.GetTotalAP() * EAPScaling + Player.Instance.MaxMana * EManaScaling;

            return Player.Instance.CalculateDamageOnUnit(unit, DamageType.Magical, damage);
        }
        
        public static float GetComboDamage(this Obj_AI_Base unit)
        {
            var damage = 0f;
            var mana = Player.Instance.Mana;

            if (SpellManager.Q.IsReady() && mana > Helpers.QMana)
            {
                mana -= Helpers.QMana;
                damage += unit.GetQDamage();
            }

            if (SpellManager.W.IsReady() && mana >= Helpers.WMana[SpellManager.W.Level])
            {
                mana -= Helpers.WMana[SpellManager.W.Level];
                damage += unit.GetWDamage();
            }

            if (!SpellManager.E.IsReady() || !(mana >= Helpers.EMana[SpellManager.E.Level]))
                return damage;

            damage += unit.GetEDamage();
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
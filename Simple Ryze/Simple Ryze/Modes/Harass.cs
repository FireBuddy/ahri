using EloBuddy;
using EloBuddy.SDK;
using Settings = SimpleRyze.Config.Modes.Harass;

namespace SimpleRyze.Modes
{
    public sealed class Harass : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass);
        }

        public override void Execute()
        {
            if (Settings.UseQ && Q.IsReady() && Player.Instance.ManaPercent >= Settings.QMana)
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (qTarget != null)
                {
                    var qPrediction = Q.GetPrediction(qTarget);
                    if (qPrediction.HitChancePercent >= 80)
                    {
                        Q.Cast(qPrediction.CastPosition);
                    }
                }
            }

            if (!Settings.UseW || !W.IsReady() || !(Player.Instance.ManaPercent >= Settings.WMana))
                return;

            var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (wTarget != null)
            {
                W.Cast(wTarget);
            }
        }
    }
}
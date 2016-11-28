using EloBuddy;
using EloBuddy.SDK;
using Settings = SimpleAhri.Config.Modes.Harass;

namespace SimpleAhri.Modes
{
    public sealed class Harass : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass);
        }

        public override void Execute()
        {
            var target = Program.CurrentTarget;
            if (target == null)
                return;

            if (Q.IsReady() && (Settings.UseQ || !target.CanMove) && Player.Instance.ManaPercent >= Settings.QMana && target.IsInRange(Player.Instance, Q.Range - 30))
            {
                var prediction = Q.GetPrediction(target);

                if (prediction.HitChancePercent >= 85)
                {
                    Q.Cast(target);
                }
            }

            if (W.IsReady() && Settings.UseW && Player.Instance.ManaPercent >= Settings.WMana && target.IsInRange(Player.Instance, W.Range - 150))
            {
                W.Cast();
            }
        }
    }
}

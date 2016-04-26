using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using Settings = SimpleRyze.Config.Modes.Flee;

namespace SimpleRyze.Modes
{
    public sealed class Flee : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee);
        }

        public override void Execute()
        {
            if (!Settings.UseW || !W.IsReady())
                return;

            var enemy =
                EntityManager.Heroes.Enemies.Where(index => index.IsValidTarget(W.Range))
                    .OrderBy(index => index.Distance(Player.Instance.ServerPosition))
                    .ThenBy(index => index.HealthPercent).FirstOrDefault();

            if (enemy != null)
            {
                W.Cast(enemy);
            }
        }
    }
}
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using Settings = SimpleAhri.Config.Modes.LaneClear;

namespace SimpleAhri.Modes
{
    public sealed class LaneClear : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Settings.UseQ && Player.Instance.ManaPercent >= Settings.QMana && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear);
        }

        public override void Execute()
        {
            if(EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, 800).Any())
            {
                var target =
                    EntityManager.MinionsAndMonsters.GetLineFarmLocation(
                        EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, 800), 150, 800);

                Q.Cast(target.CastPosition);
            }
            else
            {
                if (Player.Instance.IsUnderHisturret() && Settings.UseQUnderTower)
                    return;

                var target =
                    EntityManager.MinionsAndMonsters.GetLineFarmLocation(
                        EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                            Player.Instance.Position, Q.Range), 150, (int)Q.Range);

                if (target.HitNumber >= Settings.HitMin)
                {
                    Q.Cast(target.CastPosition);
                }
            }
        }
    }
}
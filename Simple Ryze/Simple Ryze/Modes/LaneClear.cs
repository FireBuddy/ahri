using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using Settings = SimpleRyze.Config.Modes.LaneClear;

namespace SimpleRyze.Modes
{
    public sealed class LaneClear : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear);
        }

        public override void Execute()
        {
            if (EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, 600).Any())
            {
                var target = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, W.Range).FirstOrDefault();
                if (target == null)
                    return;

                if (Settings.UseR && R.IsReady() &&
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, W.Range)
                        .Count(where => @where.HealthPercent > 25) >= 2)
                {
                    if (Settings.ROnlyIfNoEnemiesNear && Player.Instance.CountEnemiesInRange(1000) == 0)
                    {
                        R.Cast();
                    }
                }
                if (Settings.UseQ && Q.IsReady() && Player.Instance.ManaPercent >= Settings.QMana && !target.IsDead)
                {
                    Q.Cast(target);
                }
                if (Settings.UseE && E.IsReady() && Player.Instance.ManaPercent >= Settings.EMana && !target.IsDead)
                {
                    E.Cast(target);
                }
            }
            else
            {
                if (Player.Instance.IsUnderHisturret() && Settings.UnderTowerFarm)
                    return;

                var target = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, W.Range).OrderBy(x=>x.Health).FirstOrDefault();
                if (target == null)
                    return;

                if (Settings.UseR && R.IsReady() &&
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, W.Range)
                        .Count(where => @where.HealthPercent > 50) >= 3)
                {
                    if (Settings.ROnlyIfNoEnemiesNear && Player.Instance.CountEnemiesInRange(1000) == 0)
                    {
                        R.Cast();
                    }
                }
                if (Settings.UseQ && Q.IsReady() && Player.Instance.ManaPercent >= Settings.QMana && !target.IsDead)
                {
                    if (Helpers.HasRyzeRBuff)
                    {
                        Q.AllowedCollisionCount = int.MaxValue;
                    }

                    var qPrediction = Q.GetPrediction(target);
                    if (qPrediction.HitChancePercent >= 80)
                    {
                        Q.Cast(target);
                    }
                }
                if (Settings.UseE && E.IsReady() && Player.Instance.ManaPercent >= Settings.EMana && !target.IsDead)
                {
                    E.Cast(target);
                }
            }
        }
    }
}
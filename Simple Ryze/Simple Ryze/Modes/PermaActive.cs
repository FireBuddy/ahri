using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace SimpleRyze.Modes
{
    public sealed class PermaActive : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return true;
        }

        public override void Execute()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(Q.Range)))
            {
                if (enemy.GetQDamage() > enemy.Health)
                {
                    var qPrediction = Q.GetPrediction(enemy);
                    if (qPrediction.HitChancePercent >= 70)
                    {
                        Q.Cast(qPrediction.CastPosition);
                    }
                }
                if (enemy.GetWDamage() > enemy.Health && enemy.IsValidTarget(W.Range))
                {
                    W.Cast(enemy);
                }
                if (enemy.GetEDamage() > enemy.Health && enemy.IsValidTarget(E.Range))
                {
                    E.Cast(enemy);
                }
                if (Config.MiscMenu.UseIgnite && Program.Ignite != null && Program.Ignite.IsReady() && Program.Ignite.IsInRange(enemy))
                {
                    var damage = Player.Instance.GetSummonerSpellDamage(enemy, DamageLibrary.SummonerSpells.Ignite);

                    if (damage >= Program.CurrentTarget.Health + Program.CurrentTarget.PARRegenRate * 0.4f * 5)
                    {
                        Program.Ignite.Cast(Program.CurrentTarget);
                    }
                }
            }

            if (W.IsReady())
            {
                var gapclosersQuery = from x in Program.CachedAntiGapclosers
                    where x.Sender.IsValidTarget(E.Range)
                    orderby x.DangerLevel descending
                    select x;

                var gapclosers = gapclosersQuery.FirstOrDefault();

                if (Config.GapcloserMenu.Enabled && gapclosers != null)
                {
                    var menudata =
                        Config.AntiGapcloserMenuValues.FirstOrDefault(x => x.Champion == gapclosers.Sender.ChampionName);

                    if (menudata == null || !menudata.Enabled)
                        return;

                    if (Player.Instance.HealthPercent <= menudata.PercentHp &&
                        Player.Instance.Position.CountEnemiesInRange(1200) <= menudata.EnemiesNear)
                    {
                        if (menudata.Delay <= 10)
                        {
                            Helpers.PrintInfoMessage("Casting charm on <font color=\"#EB54A2\"><b>" +
                                                     gapclosers.Sender.BaseSkinName +
                                                     "</b></font> to counter his gapcloser.");
                            W.Cast(gapclosers.Sender);
                        }
                        else
                        {
                            Core.DelayAction(delegate
                            {
                                Helpers.PrintInfoMessage("Casting charm on <font color=\"#EB54A2\"><b>" +
                                                         gapclosers.Sender.BaseSkinName +
                                                         "</b></font> to counter his gapcloser.");
                                W.Cast(gapclosers.Sender);
                            }, menudata.Delay);
                        }
                    }
                }
                if (Config.AutoHarass.Enabled && Config.AutoHarass.UseW && Helpers.GetPassiveStacks < 4 && !Helpers.IsPassiveCharged && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    foreach (
                        var enemy in
                            EntityManager.Heroes.Enemies.Where(
                                e =>
                                    e.IsValidTarget(W.Range) && !e.TargetHasSpellShield() && Config.AutoHarass.AutoHarassEnabledFor(e.ChampionName) &&
                                    !e.IsTargetRooted() && e.Distance(Player.Instance) < Config.AutoHarass.WRange)
                                .OrderByDescending(TargetSelector.GetPriority)
                                .ThenByDescending(e => e.Distance(Player.Instance)))
                    {
                        if(W.IsReady())
                            W.Cast(enemy);
                    }
                }
            }
            if (Config.AutoHarass.Enabled && Helpers.GetPassiveStacks < 4 && !Helpers.IsPassiveCharged && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                if (Q.IsReady() && Config.AutoHarass.UseQ && Player.Instance.ManaPercent >= Config.AutoHarass.QMana)
                {
                    foreach (
                        var enemy in
                            EntityManager.Heroes.Enemies.Where(
                                e => e.IsValidTarget(Q.Range) && Config.AutoHarass.AutoHarassEnabledFor(e.ChampionName) && Q.GetPrediction(e).HitChancePercent >= 90).OrderByDescending(TargetSelector.GetPriority).ThenByDescending(e=>e.Distance(Player.Instance)))
                    {
                        Q.Cast(Q.GetPrediction(enemy).CastPosition);
                    }
                }
            }
            if (Config.ExtrasMenu.UseQToFarm && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Helpers.GetPassiveStacks <= Config.ExtrasMenu.MaxPassiveStacksForQFarm &&
                Player.Instance.ManaPercent >= Config.ExtrasMenu.ManaForQ && Q.IsReady() && !Helpers.IsPassiveCharged)
            {
                foreach (
                    var minion in
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                            e => e.IsValidTarget(Q.Range) && Q.GetPrediction(e).HitChancePercent >= 80 && Prediction.Health.GetPrediction(e,150+Game.Ping/2) < e.GetQDamage()-15)
                            .OrderByDescending(e => e.Health)
                            .ThenByDescending(e => e.Distance(Player.Instance)))
                {
                    if(!minion.IsDead)
                        Q.Cast(Q.GetPrediction(minion).CastPosition);
                }
            }
        }
    }
}
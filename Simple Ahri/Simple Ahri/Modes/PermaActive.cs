using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace SimpleAhri.Modes
{
    public sealed class PermaActive : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return true;
        }

        public override void Execute()
        {
            if (E.IsReady())
            {
                var gapclosersQuery = from x in Program.CachedAntiGapclosers
                    where x.Sender.IsValidTarget(E.Range)
                    orderby x.DangerLevel descending 
                    select x;

                var interruptabilespellsQuery = from x in Program.CachedInterruptibleSpells
                    where x.Sender.IsValidTarget(E.Range)
                    orderby x.DangerLevel descending
                    select x;

                var gapclosers = gapclosersQuery.FirstOrDefault();
                var interruptabilespells = interruptabilespellsQuery.FirstOrDefault();

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
                            E.Cast(gapclosers.Sender);
                        }
                        else
                        {
                            Core.DelayAction(delegate
                            {
                                Helpers.PrintInfoMessage("Casting charm on <font color=\"#EB54A2\"><b>" +
                                                         gapclosers.Sender.BaseSkinName +
                                                         "</b></font> to counter his gapcloser.");
                                E.Cast(gapclosers.Sender);
                            }, menudata.Delay);
                        }
                    }
                }

                if (Config.InterrupterMenu.Enabled && interruptabilespells != null)
                {
                    var menudata =
                        Config.InterrupterMenuValues.FirstOrDefault(x => x.Champion == interruptabilespells.Sender.Hero);

                    if (menudata == null || !menudata.Enabled)
                        return;

                    if (Player.Instance.HealthPercent <= menudata.PercentHp &&
                        Player.Instance.Position.CountEnemiesInRange(1200) <= menudata.EnemiesNear)
                    {
                        if (menudata.Delay <= 10)
                        {
                            Helpers.PrintInfoMessage("Casting charm on <font color=\"#EB54A2\"><b>" +
                                                     interruptabilespells.Sender.BaseSkinName +
                                                     "</b></font> to counter his interruptible spell.");
                            E.Cast(interruptabilespells.Sender);
                        }
                        else
                        {
                            Core.DelayAction(delegate
                            {
                                Helpers.PrintInfoMessage("Casting charm on <font color=\"#EB54A2\"><b>" +
                                                         interruptabilespells.Sender.BaseSkinName +
                                                         "</b></font> to counter his interruptible spell.");
                                E.Cast(interruptabilespells.Sender);
                            }, menudata.Delay);
                        }
                    }
                }
                if (Config.AutoHarass.Enabled && Config.AutoHarass.UseE)
                {
                    foreach (var enemy in EntityManager.Heroes.Enemies.Where(hero =>hero.IsValidTarget(E.Range - 100) && !hero.IsZombie && Config.CharmMenu.IsEEnabledFor(hero.BaseSkinName) &&
                            Q.GetPrediction(hero).HitChance == HitChance.Immobile).OrderByDescending(TargetSelector.GetPriority))
                    {
                        var prediction = E.GetPrediction(enemy);

                        Q.Cast(prediction.CastPosition);

                        Helpers.PrintInfoMessage("Casting E on immobile target : " +enemy.BaseSkinName);
                    }
                }
            }
            
            if (Config.AutoHarass.Enabled && Config.AutoHarass.UseQ && Q.IsReady() && Player.Instance.Position.CountEnemiesInRange(Q.Range - 250) >= 1 && Player.Instance.ManaPercent >= Config.AutoHarass.QMana)
            {
    //            foreach (var enemy in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(Q.Range - 250) && !hero.IsZombie && Config.AutoHarass.IsQEnabledFor(hero.BaseSkinName)).OrderByDescending(TargetSelector.GetPriority).ThenByDescending(hero => Q.GetPrediction(hero).HitChancePercent))
      //          {
      //              var time = Player.Instance.ServerPosition.Distance(enemy) / 1000;
        //            var prediction = Prediction.Position.PredictUnitPosition(enemy, 200 + Game.Ping / 2 + (int)time * 1000);
//
         //           if (prediction.IsInRange(Player.Instance, Q.Range - 250))
         //           {
         //               Q.Cast(prediction.To3D());
          //          }
        //        }
            }

            if (Config.MiscMenu.UseIgnite && Program.CurrentTarget != null && Program.Ignite != null && Program.Ignite.IsReady() && Program.Ignite.IsInRange(Program.CurrentTarget))
            {
                var damage = 50 + (20 * Player.Instance.Level);


                if (damage >= Program.CurrentTarget.Health + Program.CurrentTarget.PARRegenRate*0.4f*5)
                {
                    Program.Ignite.Cast(Program.CurrentTarget);
                }
            }
            if (W.IsReady())
            {
                var target = EntityManager.Heroes.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range) && x.Health <= x.GetWDamage());

                if (target != null)
                    W.Cast();
            }
        }
    }
}

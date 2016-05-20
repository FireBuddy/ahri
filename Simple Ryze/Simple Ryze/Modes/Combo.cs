using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using Settings = SimpleRyze.Config.Modes.Combo;

namespace SimpleRyze.Modes
{
    public sealed class Combo : ModeBase
    {
        // ReSharper disable once InconsistentNaming
        private static int LastTick;
        // ReSharper disable once InconsistentNaming
        private static Random Random;

        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo);
        }

        public override void Execute()
        {
            Random = new Random();

            if (Config.ExtrasMenu.EnableHumanizer && (LastTick + Random.Next(50, 150) > Game.Time*1000))
                return;

            var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            var passiveStacks = Helpers.GetPassiveStacks;

            if (Helpers.HasRyzeRBuff)
            {
                Q.AllowedCollisionCount = int.MaxValue;
            }

            if (R.IsReady() && Settings.UseR && wTarget != null && !Q.IsReady() && !E.IsReady() && !W.IsReady())
            {
                R.Cast();
            }
            if(R.IsReady() && Settings.UseR && wTarget != null && (passiveStacks == 3 || Helpers.IsPassiveCharged || (passiveStacks == 2 && !Q.IsReady() && !E.IsReady())))
            {
                R.Cast();
            }
            if (Q.IsReady() && Settings.UseQ && qTarget != null)
            {
                var qPredictionQ = Q.GetPrediction(qTarget);

                if (Helpers.HasRyzeRBuff && qPredictionQ.HitChancePercent >= 60 && qPredictionQ.CollisionObjects.Any(index => index.Distance(qTarget) < 400))
                {
                    Q.Cast(qPredictionQ.CastPosition);
                }
                else
                {
                    if (qPredictionQ.HitChancePercent >= 80)
                    {
                        Q.Cast(qPredictionQ.CastPosition);
                    }
                }
            }

            if (Q.IsReady() && Settings.UseQ && wTarget != null)
            {
                var qPredictionW = Q.GetPrediction(wTarget);

                if (Helpers.HasRyzeRBuff && qPredictionW.HitChancePercent >= 60 && qPredictionW.CollisionObjects.Any(index => index.Distance(wTarget) < 400))
                {
                    Q.Cast(qPredictionW.CastPosition);
                }
                else
                {
                    if (qPredictionW.HitChancePercent >= 80)
                    {
                        Q.Cast(qPredictionW.CastPosition);
                    }
                }
            }
            if (wTarget != null && Config.ExtrasMenu.UseWOn(wTarget.BaseSkinName) && W.IsReady() && passiveStacks == 4 && Settings.UseW)
            {
                W.Cast(wTarget);
            }
            if (wTarget != null && Config.ExtrasMenu.UseWOn(wTarget.BaseSkinName) && W.IsReady() && !Q.IsReady() && !E.IsReady() && !R.IsReady() && Settings.UseW)
            {
                W.Cast(wTarget);
            }

            if (E.IsReady() && Settings.UseE && wTarget != null)
            {
                E.Cast(wTarget);
            }
            
            LastTick = (int)Game.Time*1000;
        }
    }
}

using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using Settings = SimpleAhri.Config.Modes.Combo;

namespace SimpleAhri.Modes
{
    public sealed class Combo : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo);
        }

        public override void Execute()
        {
            var target = Program.CurrentTarget;

            if (target == null)
                return;

            if (Config.MiscMenu.UseIgnite && Program.Ignite != null &&
                Program.Ignite.IsReady() && Program.Ignite.IsInRange(Program.CurrentTarget))
            {
                var damage = 50 + (20*Player.Instance.Level);

                if (!Program.CurrentTarget.CanMove &&
                    Program.CurrentTarget.GetComboDamage() + damage >= Program.CurrentTarget.Health)
                {
                    Program.Ignite.Cast(Program.CurrentTarget);
                }
            }

            if (Settings.UseQ && Q.IsReady() && target.IsInRange(Player.Instance, Q.Range))
            {
                var prediction = Q.GetPrediction(target);
                if (prediction.HitChancePercent >= 70)
                {
                    Q.Cast(prediction.CastPosition);
                }
            }

            if (Settings.UseW && W.IsReady() && Player.Instance.IsInRange(target, W.Range-50))
            {
                W.Cast();
            }

            if (Settings.UseE && E.IsReady())
            {
                var prediction = E.GetPrediction(target);
                if (prediction.HitChancePercent >= 80 && (target.IsInRange(Player.Instance, 500)|| !target.CanMove)
                {
                    E.Cast(prediction.CastPosition);
                }

            }
            
            if (R.IsReady())
            {
                if (Settings.UseRAfterPlayer && !Player.HasBuff("AhriTumble"))
                    return;

                var rtarget = TargetSelector.GetTarget(E.Range + R.Range, DamageType.Magical);

                if (rtarget == null)
                    return;

                if (rtarget.IsUnderHisturret() && Settings.UseRToDive)
                {
                    if (rtarget.GetComboDamage() >= rtarget.Health*1.20f &&
                        Player.Instance.CountEnemiesInRange(1200) <= 1)
                    {
                        R.Cast(rtarget.Distance(Player.Instance) > 600
                            ? Player.Instance.Position.Extend(rtarget, 399).To3D()
                            : Player.Instance.Position.Extend(Game.CursorPos, 399).To3D());
                    }
                }
                else
                {
                    if (rtarget.GetComboDamage() >= rtarget.Health &&
                        Player.Instance.CountEnemiesInRange(1200) <= 1)
                    {
                        R.Cast(rtarget.Distance(Player.Instance) > 600
                            ? Player.Instance.Position.Extend(rtarget, 399).To3D()
                            : Player.Instance.Position.Extend(Game.CursorPos, 399).To3D());
                    }
                }


                if (Program.QReturnMissile != null && Player.HasBuff("AhriTumble") && Player.GetBuff("AhriTumble").Count <= 2)
                {
                    for (var i = -360; i < 360; i += 100)
                    {
                        var qReturnMissile = Program.QReturnMissile;

                        var qpolygon = new Geometry.Polygon.Rectangle(Player.Instance.Position,
                            qReturnMissile.StartPosition, 100);

                        if (qpolygon.IsInside(rtarget))
                            break;

                        var q =
                            Player.Instance.Position.Extend(qReturnMissile, i)
                                .RotateAroundPoint(qReturnMissile.Position.To2D(), i);

                        var polygonsahead = new Geometry.Polygon.Rectangle(Player.Instance.Position.Extend(q, i),
                            Player.Instance.Position.Extend(q, qReturnMissile.Distance(Player.Instance)), 100);
                        var polygonsbehind = new Geometry.Polygon.Rectangle(Player.Instance.Position.Extend(q, i),
                            Player.Instance.Position.Extend(q, -qReturnMissile.Distance(Player.Instance)), 100);

                        if (polygonsahead.IsInside(rtarget) || polygonsbehind.IsInside(rtarget))
                        {
                            var prediction = Prediction.Position.PredictLinearMissile(rtarget, SpellManager.Q.Range, 100,
                                250, 1500, int.MaxValue,
                                Player.Instance.Position.Extend(q, i).To3D());

                            if (prediction.HitChance == HitChance.High)
                                R.Cast(prediction.CastPosition);
                        }
                    }
                }
            }



        }
    }
}

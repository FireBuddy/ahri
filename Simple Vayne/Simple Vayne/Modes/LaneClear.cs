using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using Settings = Simple_Vayne.Config.Modes.Laneclear;

namespace Simple_Vayne.Modes
{
    public sealed class LaneClear : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) &&
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    Player.Instance.ServerPosition, Player.Instance.GetAutoAttackRange()).Any())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Execute()
        {
            if (Settings.UseQ && Q.IsReady() && Player.Instance.ManaPercent >= Settings.UseQMana)
            {
                Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            }
        }

        private void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            var unit = target as Obj_AI_Minion;

            if (unit == null || !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Orbwalker.OnPostAttack -= Orbwalker_OnPostAttack;
                return;
            }

            var qdamage = Player.Instance.GetAutoAttackDamage(unit) + Player.Instance.TotalAttackDamage * Helpers.QAdditionalDamage[Q.Level] + (unit.GetSilverStacks() == 2 ? unit.CalculateWDamage() : 0);

            var entities = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                Player.Instance.ServerPosition, Player.Instance.GetAutoAttackRange()).Where(t => t.Health < qdamage);

            var objAiMinions = entities as Obj_AI_Minion[] ?? entities.ToArray();

            if (objAiMinions.Length >= 2)
            {
                var sidePolygon = Helpers.GetSidePolygons();
                var positions = new List<Vector2>();

                for (var i = 0; i < 2; i++)
                {
                    positions.Add(sidePolygon[i].Points.Where(index =>
                    {
                        var objAiMinion = objAiMinions.LastOrDefault();

                        return objAiMinion != null && index.ToVector3().ExtendPlayerVector().IsInRange(objAiMinion.ServerPosition, Player.Instance.GetAutoAttackRange()) && index.ToVector3().ExtendPlayerVector().IsPositionSafe();
                    }).OrderByDescending(index => index.Distance(unit.ServerPosition)).FirstOrDefault());
                }
                Q.Cast(positions.OrderByDescending(index => index.Distance(unit)).FirstOrDefault(x =>
                {
                    var objAiMinion = objAiMinions.LastOrDefault();

                    return objAiMinion != null && x.ToVector3().ExtendPlayerVector().IsInRange(objAiMinion.ServerPosition, Player.Instance.GetAutoAttackRange());
                }).ToVector3().ExtendPlayerVector(200));
            }
            Orbwalker.OnPostAttack -= Orbwalker_OnPostAttack;
        }
    }
}
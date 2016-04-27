using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Spells;
using Settings = SimpleRyze.Config.ExtrasMenu;

namespace SimpleRyze.Utility
{
    public static class IncomingDamage
    {
        public static List<IncomingDamageArgs> IncomingDamages = new List<IncomingDamageArgs>();
        public static void Initializer()
        {
            Game.OnTick += Game_OnTick;

            Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.Instance.IsDead || !Settings.UseSeraph || !Program.Seraph.IsOwned() || !Program.Seraph.IsReady() || sender.IsMe)
                return;

            var heroSender = sender as AIHeroClient;
            var target = args.Target as AIHeroClient;

            if (heroSender == null || target == null || !target.IsMe)
                return;

            IncomingDamages.Add(new IncomingDamageArgs
            {
                Sender = heroSender,
                Target = target,
                Tick = (int) Game.Time*1000,
                Damage = heroSender.GetAutoAttackDamage(target, true),
                IsTargetted = true
            });
        }

        private static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.Instance.IsDead || !Settings.UseSeraph || !Program.Seraph.IsOwned() || !Program.Seraph.IsReady())
                return;

            var turret = sender as Obj_AI_Turret;
            var target = args.Target as AIHeroClient;

            if (turret == null || target == null || !target.IsMe)
                return;

            IncomingDamages.Add(new IncomingDamageArgs
            {
                Sender = turret,
                Target = target,
                IsTurretShot = true,
                Tick = (int)Game.Time * 1000,
                IsTargetted = false,
                IsSkillShot = false,
                Damage = turret.GetAutoAttackDamage(target)
            });
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.Instance.IsDead || !Settings.UseSeraph || !Program.Seraph.IsOwned() || !Program.Seraph.IsReady())
                return;

            if (sender.IsMe)
                return;
            
            var heroSender = sender as AIHeroClient;
            var target = args.Target as AIHeroClient;

           if (heroSender != null && target != null && target.IsMe)
            {
                IncomingDamages.Add(new IncomingDamageArgs
                {
                    Sender = heroSender,
                    Target = target,
                    Tick = (int)Game.Time * 1000,
                    Damage = heroSender.GetSpellDamage(target, args.Slot),
                    IsTargetted = true
                });
            }
            if (heroSender != null && target == null)
            {
                if (args.SData.TargettingType == SpellDataTargetType.LocationAoe)
                {
                    {
                        var polygon = new Geometry.Polygon.Circle(args.End, args.SData.CastRadius);
                        var polygon2 = new Geometry.Polygon.Circle(args.End, args.SData.CastRadiusSecondary);
                        if (polygon.IsInside(Player.Instance) || polygon2.IsInside(Player.Instance))
                        {
                            IncomingDamages.Add(new IncomingDamageArgs
                            {
                                Sender = heroSender,
                                Target = Player.Instance,
                                IsSkillShot = true,
                                Damage =
                                    heroSender.GetSpellDamage(Player.Instance,
                                        heroSender.GetSpellSlotFromName(args.SData.Name)),
                                Tick = (int) Game.Time*1000,
                                IsTargetted = false,
                                IsTurretShot = false
                            });
                        }
                    }
                }
                else if (args.SData.TargettingType == SpellDataTargetType.Location ||
                         args.SData.TargettingType == SpellDataTargetType.Location2 ||
                         args.SData.TargettingType == SpellDataTargetType.Location3 ||
                         args.SData.TargettingType == SpellDataTargetType.LocationVector ||
                         args.SData.TargettingType == SpellDataTargetType.LocationVector ||
                         args.SData.TargettingType == SpellDataTargetType.LocationVector)
                {
                    var range = SpellDatabase.GetSpellInfoList(heroSender).FirstOrDefault();
                    var polygon = new Geometry.Polygon.Rectangle(args.Start.To2D(),
                        args.Start.Extend(args.End, range?.Range ?? 1), args.SData.LineWidth);

                    if (polygon.IsInside(Player.Instance))
                    {
                        IncomingDamages.Add(new IncomingDamageArgs
                        {
                            Sender = heroSender,
                            Target = Player.Instance,
                            IsSkillShot = true,
                            Tick = (int) Game.Time*1000,
                            Damage =
                                heroSender.GetSpellDamage(Player.Instance,
                                    heroSender.GetSpellSlotFromName(args.SData.Name)),
                            IsTargetted = false,
                            IsTurretShot = false
                        });
                    }
                }
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            IncomingDamages.RemoveAll(match => Game.Time*1000 > match.Tick + 800);

            var damage = IncomingDamages.Sum(damages => damages.Damage);

            if (Player.Instance.IsDead || !Program.Seraph.IsReady())
                return;

            var heatlhPercentTaken = Math.Min(100, damage/(Player.Instance.Health + Player.Instance.Mana * 0.2f)*100);


            if (damage >= Player.Instance.Health-15)
            {
                Helpers.PrintInfoMessage("Casting Seraph's because something is going to kill you.");
                Program.Seraph.Cast();
            }
            else if (heatlhPercentTaken >= 40)
            {
                Helpers.PrintInfoMessage("Casting Seraph's because something is going to take more than 40% of your health.");
                Program.Seraph.Cast();
            }
        }
    }

    public class IncomingDamageArgs
    {
        public Obj_AI_Base Sender;
        public AIHeroClient Target;
        public int Tick;
        public float Damage;
        public bool IsTurretShot;
        public bool IsTargetted;
        public bool IsSkillShot;
    }
}
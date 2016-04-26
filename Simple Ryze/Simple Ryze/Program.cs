using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using SharpDX;
using SimpleRyze.Utility;
using Color = System.Drawing.Color;

namespace SimpleRyze
{
    public static class Program
    {
        public const string ChampName = "Ryze";

        public static AIHeroClient CurrentTarget;

        public static readonly List<ProcessSpellCastCache> CachedAntiGapclosers = new List<ProcessSpellCastCache>();

        private static Vector3 _flagPos;
        private static int _flagCreateTick;

        public static Text[] InfoText { get; set; }

        public static Item Rabadon, Seraph;

        public static Spell.Targeted Ignite;
        
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != ChampName)
            {
                return;
            }

            Config.Initialize();
            SpellManager.Initialize();
            ModeManager.Initialize();

            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnTick += Game_OnTick;
            HPBarIndicator.Initalize();
            PermaShow.Initalize();
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;

            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (enemy.Hero == Champion.Rengar)
                {
                    GameObject.OnCreate += GameObject_OnCreate;
                }
            }

            InfoText = new Text[3];
            InfoText[0] = new Text("", new Font("calibri", 18, FontStyle.Regular));

            var ignite = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("summonerdot"));
            if (ignite != null)
            {
                Ignite = new Spell.Targeted(ignite.Slot, 600);
            }

            if (Config.MiscMenu.SkinHackEnabled)
            {
                Player.SetSkin(Player.Instance.BaseSkinName, Config.MiscMenu.SkinId);
            }

            Rabadon = new Item(3089);
            Seraph = new Item(3040);

            TearStack.Initializer();
            IncomingDamage.Initializer();

            Helpers.PrintInfoMessage("Addon loaded !");
        }

        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || !SpellManager.Q.IsReady() || !SpellManager.W.IsReady() || !SpellManager.E.IsReady() || args.Target.Distance(Player.Instance) > 450)
                return;

            if (Config.ExtrasMenu.BlockAaInCombo)
                args.Process = false;
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Config.Drawings.Enabled)
                return;

            if (Config.Drawings.DrawRTime && SpellManager.R.IsLearned)
            {
                var rbuff = Player.Instance.GetBuff("RyzeR");

                if (rbuff != null)
                {
                    var percentage = 100 * Math.Max(0, rbuff.EndTime - Game.Time) / 6;

                    var g = Math.Max(0, 255f / 100f * percentage);
                    var r = Math.Max(0, 255 - g);

                    var color = Color.FromArgb((int)r, (int)g, 0);

                    InfoText[0].Color = color;
                    InfoText[0].X = (int)Drawing.WorldToScreen(Player.Instance.Position).X;
                    InfoText[0].Y = (int)Drawing.WorldToScreen(Player.Instance.Position).Y;
                    InfoText[0].TextValue = "\n\nR expiry time : " + Math.Max(0, rbuff.EndTime - Game.Time).ToString("F1") + "s";
                    InfoText[0].Draw();
                }
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_flagCreateTick != 0 && _flagCreateTick + 8500 < Game.Time * 1000)
            {
                _flagCreateTick = 0;
                _flagPos = Vector3.Zero;
            }

            CurrentTarget = TargetSelector.GetTarget(SpellManager.E.Range, DamageType.Magical);

            CachedAntiGapclosers.RemoveAll(x => Game.Time * 1000 > x.Tick + 850);
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                return;

            var enemy = sender as AIHeroClient;

            if (enemy == null)
                return;

            if (Config.GapcloserMenu.Enabled && Config.GapcloserMenu.GapclosersFound != 0)
            {
                var menudata = Config.AntiGapcloserMenuValues.FirstOrDefault(x => x.Champion == enemy.ChampionName);

                if (menudata == null)
                    return;

                if (enemy.Hero == Champion.Nidalee || enemy.Hero == Champion.Tristana || enemy.Hero == Champion.JarvanIV)
                {
                    if (enemy.Hero == Champion.JarvanIV && menudata.Enabled &&
                        args.SData.Name.ToLower() == "jarvanivdemacianstandard" &&
                        args.End.Distance(Player.Instance.Position) < 1000)
                    {
                        _flagPos.X = args.End.X;
                        _flagPos.Y = args.End.Y;
                        _flagPos.Z = NavMesh.GetHeightForPosition(args.End.X, args.End.Y);
                        _flagCreateTick = (int)Game.Time * 1000;
                    }

                    if (enemy.Hero == Champion.Nidalee && menudata.Enabled && args.SData.Name.ToLower() == "pounce" &&
                        args.End.Distance(Player.Instance.Position) < 350)
                    {
                        CachedAntiGapclosers.Add(new ProcessSpellCastCache
                        {
                            Sender = enemy,
                            NetworkId = enemy.NetworkId,
                            DangerLevel = menudata.DangerLevel,
                            Tick = (int)Game.Time * 1000
                        });
                    }

                    if (enemy.Hero == Champion.JarvanIV && menudata.Enabled &&
                        args.SData.Name.ToLower() == "jarvanivdragonstrike" &&
                        args.End.Distance(Player.Instance.Position) < 1000)
                    {
                        var flagpolygon = new Geometry.Polygon.Circle(_flagPos, 150);
                        var playerpolygon = new Geometry.Polygon.Circle(Player.Instance.Position, 150);

                        for (var i = 0; i < 1000; i += 25)
                        {
                            if (flagpolygon.IsInside(enemy.Position.Extend(args.End, i)) && playerpolygon.IsInside(enemy.ServerPosition.Extend(args.End, i)))
                            {
                                CachedAntiGapclosers.Add(new ProcessSpellCastCache
                                {
                                    Sender = enemy,
                                    NetworkId = enemy.NetworkId,
                                    DangerLevel = menudata.DangerLevel,
                                    Tick = (int)Game.Time * 1000
                                });
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (menudata.Enabled && args.Slot == menudata.SpellSlot &&
                        args.End.Distance(Player.Instance.Position) < 350)
                    {
                        CachedAntiGapclosers.Add(new ProcessSpellCastCache
                        {
                            Sender = enemy,
                            NetworkId = enemy.NetworkId,
                            DangerLevel = menudata.DangerLevel,
                            Tick = (int)Game.Time * 1000
                        });
                    }
                }
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name != "Rengar_LeapSound.troy")
                return;

            var gapcloserMenuInfo = Config.AntiGapcloserMenuValues.FirstOrDefault(x => x.Champion == "Rengar");

            if (gapcloserMenuInfo == null || !gapcloserMenuInfo.Enabled)
                return;

            foreach (var rengar in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(1000) && x.ChampionName == "Rengar").Where(rengar => rengar.Distance(Player.Instance.Position) < 1000))
            {
                CachedAntiGapclosers.Add(new ProcessSpellCastCache
                {
                    Sender = rengar,
                    NetworkId = rengar.NetworkId,
                    DangerLevel = gapcloserMenuInfo.DangerLevel,
                    Tick = (int)Game.Time * 1000
                });
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if(!Config.Drawings.Enabled)
                return;
            
            if (Config.Drawings.DrawW && SpellManager.W.IsLearned)
                Circle.Draw(SpellManager.W.IsReady() ? SharpDX.Color.GreenYellow : SharpDX.Color.Red, SpellManager.W.Range, Config.Drawings.DrawingBorderWidth, Player.Instance.Position);

            if (Config.Drawings.DrawQ && SpellManager.Q.IsLearned)
                Circle.Draw(SpellManager.Q.IsReady() ? SharpDX.Color.DeepSkyBlue : SharpDX.Color.Red, SpellManager.Q.Range, Config.Drawings.DrawingBorderWidth, Player.Instance.Position);
        }
    }
}
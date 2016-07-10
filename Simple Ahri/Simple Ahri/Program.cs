using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleAhri
{
    public static class Program
    {
        public const string ChampName = "Ahri";

        public static AIHeroClient CurrentTarget;
        
        public static readonly List<ProcessSpellCastCache> CachedAntiGapclosers = new List<ProcessSpellCastCache>();
        public static readonly List<ProcessSpellCastCache> CachedInterruptibleSpells = new List<ProcessSpellCastCache>();

        private static Vector3 _flagPos;
        private static int _flagCreateTick;

        public static Text[] InfoText { get; set; }

        public static Item Rabadon;

        public static Spell.Targeted Ignite;
        
        public static Spell.Targeted Flash;
        
        public static Obj_AI_Minion Minion;

        public static MissileClient QOrbMissile, QReturnMissile;

        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != ChampName)
            {
                //return;
            }

            Config.Initialize();
            SpellManager.Initialize();
            ModeManager.Initialize();
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast2;
            Game.OnTick += Game_OnTick;
            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            HPBarIndicator.Initalize();
            Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;

            if (Config.Drawings.DrawPermashow)
            {
               
            }

            InfoText = new Text[3];
            InfoText[0] = new Text("", new Font("calibri", 18, FontStyle.Regular));

            var ignite = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("summonerdot"));
            if (ignite != null)
            {
                Ignite = new Spell.Targeted(ignite.Slot, 600);
            }
            var flash = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("summonerflash"));
            if (flash != null)
            {
                Flash = new Spell.Targeted(flash.Slot, 425);
            }

            if (Config.MiscMenu.SkinHackEnabled)
            {
                Player.SetSkin(Player.Instance.BaseSkinName, Config.MiscMenu.SkinId);
            }

            Rabadon = new Item(3089);

            Helpers.PrintInfoMessage("Addon loaded !");
        }
        
        private static void Game_OnGameUpdate(EventArgs args)
        {
           if (Config.Modes.Combo.UseEFlash)
            {
            CastEFlash();
            }

        }


        
        private static void CastEFlash()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (Flash.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.EFlash.Range + 100, DamageType.Magical);
                    //var target = TargetSelector.SelectedTarget;
                if (target.IsValidTarget() && !target.IsInvulnerable)
                {
                    var pre = SpellManager.EFlash.GetPrediction(target);
                    var postion = EloBuddy.Player.Instance.ServerPosition.Extend(target.ServerPosition, Flash.Range);
                    int Delay = SpellManager.E.CastDelay + Game.Ping - 60;
                    
                        if (SpellManager.E.IsReady() && pre.HitChance >= HitChance.High)
                            if (SpellManager.EFlash.Cast(pre.CastPosition))
                                Core.DelayAction(delegate { Flash.Cast(postion.To3DWorld()); },Delay + 15); 
                    
                }    
            }
        }
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Config.Drawings.Enabled)
                return;

            if (Config.Drawings.DrawRTime && SpellManager.R.IsLearned)
            {
                var rbuff = Player.Instance.GetBuff("AhriTumble");

                if (rbuff != null)
                {
                    var percentage = 100 * Math.Max(0, rbuff.EndTime - Game.Time) / 10;

                    var g = Math.Max(0, 255f / 100f * percentage);
                    var r = Math.Max(0, 255 - g);

                    var color = Color.FromArgb((int)r, (int)g, 0);

                    InfoText[0].Color = color;
                    InfoText[0].X = (int)Drawing.WorldToScreen(Player.Instance.Position).X;
                    InfoText[0].Y = (int)Drawing.WorldToScreen(Player.Instance.Position).Y;
                    InfoText[0].TextValue = "\n\nR expiry time : " + Math.Max(0, rbuff.EndTime - Game.Time).ToString("F1") + "s | Stacks : "+ Player.Instance.GetBuff("AhriTumble").Count;
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
            CachedInterruptibleSpells.RemoveAll(x => Game.Time * 1000 > x.Tick + 8000);

            if (Config.InterrupterMenu.Enabled)
            {
                var processSpellCastCache = CachedInterruptibleSpells.FirstOrDefault();

                if (processSpellCastCache != null)
                {
                    var enemy = processSpellCastCache.Sender;

                    if (!enemy.Spellbook.IsCastingSpell && !enemy.Spellbook.IsCharging && !enemy.Spellbook.IsChanneling)
                        CachedInterruptibleSpells.Remove(processSpellCastCache);
                }
            }
        }

        private static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || !Config.AutoHarass.Enabled )
            {
               return;
            }
            CurrentTarget = TargetSelector.GetTarget(SpellManager.Q.Range, DamageType.Magical);
            if (!sender.IsInvulnerable && sender == CurrentTarget && sender.IsValidTarget(865) && SpellManager.Q.IsReady())
            {
                
                {
                    Chat.Print("basic Attack");
                    SpellManager.Q.Cast(sender.ServerPosition);
                }

            } 
        }
        private static void Obj_AI_Base_OnProcessSpellCast2(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            
            if (sender == null || !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
               return;
            }
            CurrentTarget = TargetSelector.GetTarget(SpellManager.Q.Range, DamageType.Magical);
            if (SpellManager.Q.IsReady() && sender.IsValidTarget(865) && !sender.IsInvulnerable && args.Target != CurrentTarget && !sender.IsDashing() && sender == CurrentTarget && !sender.IsDashing())
            {
                
                if (args.End.Distance(Player.Instance.Position) <= 100)
                {
                   Chat.Print("Receiving damage"+args.SData.Name);


                 }
                if (args.End.Distance(Player.Instance.Position) >= 100)
                {

                    Chat.Print("Not Receiving damage" +args.SData.Name);
                    SpellManager.Q.Cast(sender.ServerPosition);

                }                
                if (args.Target != null)
                {
                    Chat.Print("targetspell"+args.SData.Name);
                    SpellManager.Q.Cast(sender.ServerPosition);

                }

            } 
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

            if (Config.InterrupterMenu.Enabled && Config.InterrupterMenu.InterruptibleSpellsFound != 0)
            {
                var menudata = Config.InterrupterMenuValues.FirstOrDefault(info => info.Champion == enemy.Hero);

                if (menudata == null)
                    return;

                if (menudata.Enabled && args.Slot == menudata.SpellSlot)
                {
                    CachedInterruptibleSpells.Add(new ProcessSpellCastCache
                    {
                        Sender = enemy,
                        NetworkId = enemy.NetworkId,
                        DangerLevel = menudata.DangerLevel,
                        Tick = (int)Game.Time * 1000
                    });
                }
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var client = sender as MissileClient;
            if (client != null && client.SData.Name == "AhriOrbMissile")
            {
                QOrbMissile = client;
            }
            if (client != null && client.SData.Name == "AhriOrbReturn")
            {
                QReturnMissile = client;
            }

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



        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var client = sender as MissileClient;
            if (client != null && client.SData.Name == "AhriOrbMissile")
            {
                QOrbMissile = null;
            }
            if (client != null && client.SData.Name == "AhriOrbReturn")
            {
                QReturnMissile = null;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if(!Config.Drawings.Enabled)
                return;
            
            if (SpellManager.Q.IsLearned && SpellManager.Q.IsReady())
                Circle.Draw(SharpDX.Color.Aqua, SpellManager.Q.Range, Config.Drawings.DrawingBorderWidth, Player.Instance.Position);
            
            if (Config.Drawings.DrawW && SpellManager.W.IsLearned && SpellManager.W.IsReady())
                Circle.Draw(SharpDX.Color.GreenYellow, SpellManager.W.Range, Config.Drawings.DrawingBorderWidth, Player.Instance.Position);

            if (Config.Drawings.DrawE && SpellManager.E.IsLearned && SpellManager.E.IsReady())
                Circle.Draw(SharpDX.Color.DeepPink, SpellManager.E.Range, Config.Drawings.DrawingBorderWidth, Player.Instance.Position);

            if (!Config.Drawings.DrawQPosition)
                return;
            
            var end = new Vector3();
            var position = new Vector3();
            var start = new Vector3();

            if (QOrbMissile != null)
            {
                start = Player.Instance.ServerPosition;
                position = QOrbMissile.Position;
                end = QOrbMissile.EndPosition;
            }
            else if(QReturnMissile != null)
            {
                start = QReturnMissile.StartPosition;
                position = QReturnMissile.Position;
                end = Player.Instance.ServerPosition;
            }

            if (end == Vector3.Zero)
                return;
            
            var polygon1 = new Geometry.Polygon.Rectangle(start.To2D(), end.To2D(), 150);
            polygon1.Draw(Color.White);
            var polygon2 = new Geometry.Polygon.Rectangle(start.To2D(), end.To2D(), 100);
            polygon2.Draw(Color.GreenYellow);
            
            var direction = (end - start).Normalized().To2D();
            var x = Drawing.WorldToScreen((position.To2D() + 90 * direction.Perpendicular()).To3D());
            var y = Drawing.WorldToScreen((position.To2D() - 100 * direction.Perpendicular()).To3D());

            Drawing.DrawLine(x.X, x.Y, y.X, y.Y, 3, Color.DeepPink);
        }
    }
}

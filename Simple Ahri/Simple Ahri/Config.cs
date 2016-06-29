using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SimpleAhri.Utility;
using Interrupter = SimpleAhri.Utility.Interrupter;

namespace SimpleAhri
{
    public static class Config
    {
        private const string MenuName = "Simple Ahri";

        private static readonly Menu Menu;

        public static List<Interrupter.InterrupterMenuInfo> InterrupterMenuValues = new List<Interrupter.InterrupterMenuInfo>();
        public static List<Gapclosers.GapcloserMenuInfo> AntiGapcloserMenuValues = new List<Gapclosers.GapcloserMenuInfo>();


        static Config()
        {
            Menu = MainMenu.AddMenu(MenuName, MenuName.ToLower());

            Modes.InitializeModes();
            InterrupterMenu.Initializer();
            GapcloserMenu.Initializer();
            CharmMenu.Initializer();
            AutoHarass.Initializer();
            Drawings.Initializer();
        }

        public static void Initialize()
        {
        }

        /// <summary>
        /// InterrupterMenu
        /// </summary>
        public static class InterrupterMenu
        {
            /// <summary>
            /// Returns true if Interrupter is enabled.
            /// </summary>
            public static bool Enabled => interrupterMenu["InterrupterEnabled"].Cast<CheckBox>().CurrentValue;

            // ReSharper disable once InconsistentNaming
            private static readonly Menu interrupterMenu;


            public static int InterruptibleSpellsFound;

            /// <summary>
            /// Static initializer
            /// </summary>
            public static void Initializer()
            {
            }

            static InterrupterMenu()
            {
                interrupterMenu = Menu.AddSubMenu("Interrupter");
                interrupterMenu.Add("InterrupterEnabled", new CheckBox("Enable Interrupter"));
                interrupterMenu.AddSeparator();


                interrupterMenu.AddGroupLabel("Interruptible Spells !");
                interrupterMenu.AddSeparator(5);

                var inter = new Interrupter();

                foreach (
                    var data in
                        EntityManager.Heroes.Enemies.Where(x => Interrupter.Interruptible.ContainsKey(x.Hero))
                            .Select(x => Interrupter.Interruptible.FirstOrDefault(key => key.Key == x.Hero)))
                {
                    var dangerlevel = Interrupter.Interruptible.FirstOrDefault(pair => pair.Key == data.Key);

                    interrupterMenu.AddGroupLabel(data.Key + " " + data.Value.SpellSlot);

                    interrupterMenu.Add("Enabled." + data.Key, new CheckBox("Enabled")).OnValueChange +=
                        delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = InterrupterMenuValues.FirstOrDefault(i => i.Champion == data.Key);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Enabled = args.NewValue;
                        };

                    interrupterMenu.Add("DangerLevel." + data.Key, new ComboBox("Danger Level", new[] { "High", "Medium", "Low" }, (int)dangerlevel.Value.DangerLevel)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = InterrupterMenuValues.FirstOrDefault(i => i.Champion == data.Key);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.DangerLevel = (DangerLevel)args.NewValue;
                        };

                    interrupterMenu.Add("PercentHP." + data.Key, new Slider("Only if Im below of {0} % of my HP", 100)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = InterrupterMenuValues.FirstOrDefault(i => i.Champion == data.Key);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.PercentHp = args.NewValue;
                        };

                    interrupterMenu.Add("EnemiesNear." + data.Key, new Slider("Only if {0} or less enemies are near", 5, 1, 5)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = InterrupterMenuValues.FirstOrDefault(i => i.Champion == data.Key);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.EnemiesNear = args.NewValue;
                        };

                    var rand = new Random();

                    interrupterMenu.Add("Delay." + data.Key, new Slider("Humanizer delay", rand.Next(15, 50), 0, 500)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = InterrupterMenuValues.FirstOrDefault(i => i.Champion == data.Key);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Delay = args.NewValue;
                        };

                    interrupterMenu.AddSeparator(5);

                    InterrupterMenuValues.Add(new Interrupter.InterrupterMenuInfo
                    {
                        Champion = data.Key,
                        SpellSlot = data.Value.SpellSlot,
                        Delay = interrupterMenu["Delay." + data.Key].Cast<Slider>().CurrentValue,
                        DangerLevel = (DangerLevel)interrupterMenu["DangerLevel." + data.Key].Cast<ComboBox>().CurrentValue,
                        Enabled = interrupterMenu["Enabled." + data.Key].Cast<CheckBox>().CurrentValue,
                        EnemiesNear = interrupterMenu["EnemiesNear." + data.Key].Cast<Slider>().CurrentValue,
                        PercentHp = interrupterMenu["PercentHP." + data.Key].Cast<Slider>().CurrentValue
                    });

                    InterruptibleSpellsFound++;
                }
                if (InterruptibleSpellsFound == 0)
                {
                    interrupterMenu.AddGroupLabel("No interruptible spells found !");
                }

            }
        }

        /// <summary>
        /// Gapcloser menu
        /// </summary>
        public static class GapcloserMenu
        {
            /// <summary>
            /// Returns true if anti gapclosers is enabled.
            /// </summary>
            public static bool Enabled => gapcloserMenu["GapclosersEnabled"].Cast<CheckBox>().CurrentValue;

            // ReSharper disable once InconsistentNaming
            private static readonly Menu gapcloserMenu;

            public static int GapclosersFound;

            static GapcloserMenu()
            {
                gapcloserMenu = Menu.AddSubMenu("Anti-Gapcloser");
                gapcloserMenu.Add("GapclosersEnabled", new CheckBox("Enable Anti-Gapcloser"));
                gapcloserMenu.AddSeparator(5);

                gapcloserMenu.AddGroupLabel("Enemy Gapclosers : ");
                gapcloserMenu.AddSeparator(5);

                foreach (
                    var data in
                        EntityManager.Heroes.Enemies.Where(x => Gapcloser.GapCloserList.Exists(k => k.ChampName == x.ChampionName) && x.Hero != Champion.Ziggs)
                            .Select(x => Gapcloser.GapCloserList.FirstOrDefault(key => key.ChampName == x.ChampionName)))
                {
                    var dangerlevel = Gapclosers.GapcloserDangerLevel.FirstOrDefault(pair => pair.Key == data.ChampName);

                    gapcloserMenu.AddGroupLabel(data.ChampName + " " + data.SpellSlot + " [" + data.SpellName + "]");

                    gapcloserMenu.Add("Enabled." + data.ChampName, new CheckBox("Enabled")).OnValueChange +=
                        delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == data.ChampName);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Enabled = args.NewValue;
                        };

                    gapcloserMenu.Add("DangerLevel." + data.ChampName, new ComboBox("Danger Level", new[] { "High", "Medium", "Low" }, (int)dangerlevel.Value.DangerLevel)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == data.ChampName);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.DangerLevel = (DangerLevel)args.NewValue;
                        };

                    gapcloserMenu.AddSeparator(5);

                    gapcloserMenu.Add("PercentHP." + data.ChampName, new Slider("Only if Im below of {0} % of my HP", 100)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == data.ChampName);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.PercentHp = args.NewValue;
                        };

                    gapcloserMenu.Add("EnemiesNear." + data.ChampName, new Slider("Only if {0} or less enemies are near", dangerlevel.Value.EnemiesNear, 1, 5)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == data.ChampName);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.EnemiesNear = args.NewValue;
                        };

                    gapcloserMenu.Add("Delay." + data.ChampName, new Slider("Humanizer delay", dangerlevel.Value.Delay, 0, 500)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == data.ChampName);
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Delay = args.NewValue;
                        };

                    gapcloserMenu.AddSeparator(5);

                    AntiGapcloserMenuValues.Add(new Gapclosers.GapcloserMenuInfo
                    {
                        Champion = data.ChampName,
                        SpellSlot = data.SpellSlot,
                        SpellName = data.SpellName,
                        Delay = gapcloserMenu["Delay." + data.ChampName].Cast<Slider>().CurrentValue,
                        DangerLevel = (DangerLevel)gapcloserMenu["DangerLevel." + data.ChampName].Cast<ComboBox>().CurrentValue,
                        Enabled = gapcloserMenu["Enabled." + data.ChampName].Cast<CheckBox>().CurrentValue,
                        EnemiesNear = gapcloserMenu["EnemiesNear." + data.ChampName].Cast<Slider>().CurrentValue,
                        PercentHp = gapcloserMenu["PercentHP." + data.ChampName].Cast<Slider>().CurrentValue
                    });
                    GapclosersFound++;
                }

                if (EntityManager.Heroes.Enemies.FirstOrDefault(x => x.Hero == Champion.Nidalee) != null)
                {
                    gapcloserMenu.AddGroupLabel("Nidalee W [Pounce]");

                    gapcloserMenu.Add("Enabled.Nidalee", new CheckBox("Enabled")).OnValueChange +=
                        delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Nidalee");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Enabled = args.NewValue;
                        };

                    gapcloserMenu.Add("DangerLevel.Nidalee", new ComboBox("Danger Level", new[] { "High", "Medium", "Low" }, 1)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Nidalee");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.DangerLevel = (DangerLevel)args.NewValue;
                        };

                    gapcloserMenu.AddSeparator(5);

                    gapcloserMenu.Add("PercentHP.Nidalee", new Slider("Only if Im below of {0} % of my HP", 100)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Nidalee");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.PercentHp = args.NewValue;
                        };

                    gapcloserMenu.Add("EnemiesNear.Nidalee", new Slider("Only if {0} or less enemies are near", 5, 1, 5)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Nidalee");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.EnemiesNear = args.NewValue;
                        };

                    gapcloserMenu.Add("Delay.Nidalee", new Slider("Humanizer delay", 0, 0, 500)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Nidalee");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Delay = args.NewValue;
                        };

                    gapcloserMenu.AddSeparator(5);

                    AntiGapcloserMenuValues.Add(new Gapclosers.GapcloserMenuInfo
                    {
                        Champion = "Nidalee",
                        SpellName = "pounce",
                        Delay = gapcloserMenu["Delay.Nidalee"].Cast<Slider>().CurrentValue,
                        DangerLevel = (DangerLevel)gapcloserMenu["DangerLevel.Nidalee"].Cast<ComboBox>().CurrentValue,
                        Enabled = gapcloserMenu["Enabled.Nidalee"].Cast<CheckBox>().CurrentValue,
                        EnemiesNear = gapcloserMenu["EnemiesNear.Nidalee"].Cast<Slider>().CurrentValue,
                        PercentHp = gapcloserMenu["PercentHP.Nidalee"].Cast<Slider>().CurrentValue
                    });

                    GapclosersFound++;
                }

                if (EntityManager.Heroes.Enemies.FirstOrDefault(x => x.Hero == Champion.Rengar) != null)
                {
                    gapcloserMenu.AddGroupLabel("Rengar's passive [Leap]");

                    gapcloserMenu.Add("Enabled.Rengar", new CheckBox("Enabled")).OnValueChange +=
                        delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Rengar");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Enabled = args.NewValue;
                        };

                    gapcloserMenu.Add("DangerLevel.Rengar", new ComboBox("Danger Level", new[] { "High", "Medium", "Low" })).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Rengar");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.DangerLevel = (DangerLevel)args.NewValue;
                        };

                    gapcloserMenu.AddSeparator(5);

                    gapcloserMenu.Add("PercentHP.Rengar", new Slider("Only if Im below of {0} % of my HP", 100)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Rengar");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.PercentHp = args.NewValue;
                        };

                    gapcloserMenu.Add("EnemiesNear.Rengar", new Slider("Only if {0} or less enemies are near", 5, 1, 5)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Rengar");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.EnemiesNear = args.NewValue;
                        };

                    gapcloserMenu.Add("Delay.Rengar", new Slider("Humanizer delay", 0, 0, 500)).OnValueChange +=
                        delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            var interrupterMenuInfo = AntiGapcloserMenuValues.FirstOrDefault(i => i.Champion == "Rengar");
                            if (interrupterMenuInfo != null)
                                interrupterMenuInfo.Delay = args.NewValue;
                        };

                    gapcloserMenu.AddSeparator(5);

                    AntiGapcloserMenuValues.Add(new Gapclosers.GapcloserMenuInfo
                    {
                        Champion = "Rengar",
                        SpellName = "Rengar_LeapSound.troy",
                        Delay = gapcloserMenu["Delay.Rengar"].Cast<Slider>().CurrentValue,
                        DangerLevel = (DangerLevel)gapcloserMenu["DangerLevel.Rengar"].Cast<ComboBox>().CurrentValue,
                        Enabled = gapcloserMenu["Enabled.Rengar"].Cast<CheckBox>().CurrentValue,
                        EnemiesNear = gapcloserMenu["EnemiesNear.Rengar"].Cast<Slider>().CurrentValue,
                        PercentHp = gapcloserMenu["PercentHP.Rengar"].Cast<Slider>().CurrentValue
                    });

                    GapclosersFound++;
                }

                if (GapclosersFound == 0)
                {
                    gapcloserMenu.AddGroupLabel("No gapclosers found !");
                }

            }

            /// <summary>
            /// Static initializer
            /// </summary>
            public static void Initializer()
            {
            }
        }

        /// <summary>
        /// CharmMenu Menu
        /// </summary>
        public static class CharmMenu
        {
            /// <summary>
            /// Use E On 
            /// </summary>
            /// <param name="championName">Enemy champion name</param>
            /// <returns><c>True</c> if checkbox is true</returns>
            public static bool IsEEnabledFor(string championName)
            {
                return charmMenu["Enabled." + championName] != null && charmMenu["Enabled." + championName].Cast<CheckBox>().CurrentValue;
            }

            // ReSharper disable once InconsistentNaming
            private static readonly Menu charmMenu;

            /// <summary>
            /// Static initializer
            /// </summary>
            public static void Initializer()
            {
            }

            static CharmMenu()
            {
                charmMenu = Menu.AddSubMenu("Charm settings");
                charmMenu.AddGroupLabel("Charm settings !");
                charmMenu.AddLabel("Use E on : ");

                foreach (var hero in EntityManager.Heroes.Enemies)
                {
                    charmMenu.Add("Enabled." + hero.BaseSkinName, new CheckBox(hero.BaseSkinName));
                }
            }
        }

        /// <summary>
        /// AutoHarass Menu
        /// </summary>
        public static class AutoHarass
        {

            /// <summary>
            /// Returns true if Auto Harass is enabled.
            /// </summary>
            public static bool Enabled => AutoharassMenu["AutoHarass.Enabled"].Cast<KeyBind>().CurrentValue;

            /// <summary>
            /// Returns true if Use Q is enabled.
            /// </summary>
            public static bool UseQ => AutoharassMenu["AutoHarass.UseQ"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns minimal mana for Q value.
            /// </summary>
            public static int QMana => AutoharassMenu["AutoHarass.ManaQ"].Cast<Slider>().CurrentValue;

            /// <summary>
            /// Returns true if Use E is enabled.
            /// </summary>
            public static bool UseE => AutoharassMenu["AutoHarass.UseE"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Use E On 
            /// </summary>
            /// <param name="championName">Enemy champion name</param>
            /// <returns><c>True</c> if checkbox is true</returns>
            public static bool IsQEnabledFor(string championName)
            {
                return AutoharassMenu["Enabled." + championName] != null && AutoharassMenu["Enabled." + championName].Cast<CheckBox>().CurrentValue;
            }

            private static readonly Menu AutoharassMenu;

            /// <summary>
            /// Static initializer
            /// </summary>
            public static void Initializer()
            {
            }

            static AutoHarass()
            {
                AutoharassMenu = Menu.AddSubMenu("Auto Harass");
                AutoharassMenu.Add("AutoHarass.Enabled", new KeyBind("Enable Auto Harass", false, KeyBind.BindTypes.PressToggle, 'G')).OnValueChange +=
                    delegate(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                    {
                        Helpers.PrintInfoMessage(
                            args.NewValue
                                ? "Auto Harass : <font color =\"#32BA1A\"><b>Enabled</b></font>"
                                : "Auto Harass : <font color =\"#E02007\"><b>Disabled</b></font>", false);
                    };

                AutoharassMenu.Add("AutoHarass.UseQ", new CheckBox("Use Q"));
                AutoharassMenu.Add("AutoHarass.ManaQ", new Slider("Minimal mana ({0}%) to use Q", 80));
                AutoharassMenu.Add("AutoHarass.UseE", new CheckBox("Use E on immobile targets"));
                AutoharassMenu.AddSeparator();
                AutoharassMenu.AddLabel("Use Q on : ");
                foreach (var hero in EntityManager.Heroes.Enemies)
                {
                    AutoharassMenu.Add("Enabled." + hero.BaseSkinName, new CheckBox(hero.BaseSkinName));
                }
            }
        }


        /// <summary>
        /// CharmMenu Menu
        /// </summary>
        public static class Drawings
        {

            /// <summary>
            /// Returns true if Drawings are enabled.
            /// </summary>
            public static bool Enabled => DrawingsMenu["Enable"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns true if DrawQPosition is enabled.
            /// </summary>
            public static bool DrawQPosition => DrawingsMenu["DrawQPosition"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns true if DrawW is enabled.
            /// </summary>
            public static bool DrawW => DrawingsMenu["DrawW"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns true if DrawE is enabled.
            /// </summary>
            public static bool DrawE => DrawingsMenu["DrawE"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns DrawingBorderWidth value.
            /// </summary>
            public static int DrawingBorderWidth => DrawingsMenu["DrawingBorderWidth"].Cast<Slider>().CurrentValue;

            /// <summary>
            /// Returns true if DrawRTime is enabled.
            /// </summary>
            public static bool DrawRTime => DrawingsMenu["DrawRTime"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns true if DrawPermashow is enabled.
            /// </summary>
            public static bool DrawPermashow => DrawingsMenu["DrawPermashow"].Cast<CheckBox>().CurrentValue;
            // ReSharper disable once InconsistentNaming
            private static readonly Menu DrawingsMenu;

            /// <summary>
            /// Static initializer
            /// </summary>
            public static void Initializer()
            {
            }

            static Drawings()
            {
                DrawingsMenu = Menu.AddSubMenu("Drawings");
                DrawingsMenu.AddGroupLabel("Drawing settings !");
                DrawingsMenu.Add("Enable", new CheckBox("Enable drawings"));
                DrawingsMenu.Add("DrawQPosition", new CheckBox("Draw Q position"));
                DrawingsMenu.Add("DrawW", new CheckBox("Draw W range"));
                DrawingsMenu.Add("DrawE", new CheckBox("Draw E range"));
                DrawingsMenu.Add("DrawingBorderWidth", new Slider("Border Width", 2, 1, 10));
                DrawingsMenu.Add("DrawRTime", new CheckBox("Draw R expiry time"));
                DrawingsMenu.Add("DrawPermashow", new CheckBox("Enable PermaShow"));
            }
        }

        /// <summary>
        /// Misc Menu
        /// </summary>
        public static class MiscMenu
        {
            /// <summary>
            /// Returns Skin hack checkbox value
            /// </summary>
            public static bool SkinHackEnabled => miscMenu["SkinHack.Enabled"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns Skin hack checkbox value
            /// </summary>
            public static int SkinId => miscMenu["SkinID"].Cast<ComboBox>().CurrentValue;

            /// <summary>
            /// Returns Ignite checkbox value
            /// </summary>
            public static bool UseIgnite => miscMenu["Ignite.Enabled"].Cast<CheckBox>().CurrentValue;

            public static int PermashowX => miscMenu["Permashow.X"].Cast<Slider>().CurrentValue;
            public static int PermashowY => miscMenu["Permashow.Y"].Cast<Slider>().CurrentValue;

            // ReSharper disable once InconsistentNaming
            private static readonly Menu miscMenu;

            /// <summary>
            /// Static initializer
            /// </summary>
            public static void Initializer()
            {
            }

            static MiscMenu()
            {
                miscMenu = Menu.AddSubMenu("Misc");
                miscMenu.AddGroupLabel("Misc settings !");
                miscMenu.Add("SkinHack.Enabled", new CheckBox("Enable Skin hack", false)).OnValueChange +=
                    delegate
                    {
                        if (Player.Instance.SkinId != SkinId)
                        {
                            Player.SetSkin(Player.Instance.BaseSkinName, SkinId);
                        }
                    };

                miscMenu.Add("Ignite.Enabled", new CheckBox("Use Ignite"));

                miscMenu.Add("SkinID", new ComboBox("Skin : ", new[] {"Classic", "Dynasty", "Midnight", "Foxfire", "Popstar", "Challenger", "Academy"})).OnValueChange +=
                    delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                    {
                        if (SkinHackEnabled && Player.Instance.SkinId != SkinId)
                        {
                            Player.SetSkin(Player.Instance.BaseSkinName, args.NewValue);
                        }
                    };

                miscMenu.Add("Permashow.X", new Slider("Permashow position X: ", (int)(Drawing.Width*0.87f), 0, Drawing.Width)).OnValueChange +=
                    delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                    {
                        PermaShow.Position[0] = args.NewValue;
                    };
                miscMenu.Add("Permashow.Y", new Slider("Permashow position Y: ", (int)(Drawing.Height * 0.08f), 0, Drawing.Height)).OnValueChange +=
                    delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                    {
                        PermaShow.Position[1] = args.NewValue;
                    };
            }
        }


        public static class Modes
        {
            private static readonly Menu ModesMenu;

            static Modes()
            {
                ModesMenu = Menu.AddSubMenu("Modes");

                Combo.InitializeCombo();
                ModesMenu.AddSeparator();
                Harass.InitializeHarass();
                ModesMenu.AddSeparator();
                LaneClear.InitializeLaneClear();
                ModesMenu.AddSeparator();
                Flee.InitializeFlee();
            }

            public static void InitializeModes()
            {
            }

            public static class Combo
            {
                public static bool UseQ => ModesMenu["Combo.Q"].Cast<CheckBox>().CurrentValue;
                public static bool UseW => ModesMenu["Combo.W"].Cast<CheckBox>().CurrentValue;
                public static bool UseE => ModesMenu["Combo.E"].Cast<CheckBox>().CurrentValue;
                public static bool UseR => ModesMenu["Combo.R"].Cast<CheckBox>().CurrentValue;
                public static bool UseRToDive => ModesMenu["Combo.RDive"].Cast<KeyBind>().CurrentValue;
                public static bool UseRAfterPlayer => ModesMenu["Combo.ROnlyAfterPlayer"].Cast<CheckBox>().CurrentValue;

                static Combo()
                {
                    ModesMenu.AddGroupLabel("Combo");
                    ModesMenu.Add("Combo.Q", new CheckBox("Use Q"));
                    ModesMenu.Add("Combo.W", new CheckBox("Use W"));
                    ModesMenu.Add("Combo.E", new CheckBox("Use E"));
                    ModesMenu.Add("Combo.R", new CheckBox("Use R"));
                    ModesMenu.Add("Combo.RDive", new KeyBind("Use R to dive", false, KeyBind.BindTypes.PressToggle, 'H'));
                    ModesMenu.Add("Combo.ROnlyAfterPlayer", new CheckBox("Use R only if you used it before"));
                    ModesMenu.Add("Combo EFlash", new KeyBind("Use E + Flash", false, KeyBind.BindTypes.HoldActive, 'Z'));
                }

                public static void InitializeCombo()
                {
                }
            }

            public static class Harass
            {
                public static bool UseQ => ModesMenu["Harass.Q"].Cast<CheckBox>().CurrentValue;
                public static int QMana => ModesMenu["Harass.Q.Mana"].Cast<Slider>().CurrentValue;
                public static bool UseW => ModesMenu["Harass.W"].Cast<CheckBox>().CurrentValue;
                public static int WMana => ModesMenu["Harass.W.Mana"].Cast<Slider>().CurrentValue;

                static Harass()
                {
                    ModesMenu.AddGroupLabel("Harass");
                    ModesMenu.Add("Harass.Q", new CheckBox("Use Q"));
                    ModesMenu.Add("Harass.Q.Mana", new Slider("Minimal mana ({0}%) to use Q", 60));
                    ModesMenu.Add("Harass.W", new CheckBox("Use W"));
                    ModesMenu.Add("Harass.W.Mana", new Slider("Minimal mana ({0}%) to use W", 40));
                }

                public static void InitializeHarass()
                {
                }
            }

            public static class LaneClear
            {
                public static bool UseQ => ModesMenu["LaneClear.Q"].Cast<CheckBox>().CurrentValue;
                public static bool UseQUnderTower => ModesMenu["LaneClear.QTower"].Cast<CheckBox>().CurrentValue;
                public static int QMana => ModesMenu["LaneClear.Q.Mana"].Cast<Slider>().CurrentValue;
                public static int HitMin => ModesMenu["LaneClear.Q.HitMin"].Cast<Slider>().CurrentValue;

                static LaneClear()
                {
                    ModesMenu.AddGroupLabel("Lane clear / Jungle clear");
                    ModesMenu.Add("LaneClear.Q", new CheckBox("Use Q"));
                    ModesMenu.Add("LaneClear.QTower", new CheckBox("Dont use Q under tower"));
                    ModesMenu.Add("LaneClear.Q.Mana", new Slider("Minimal mana ({0}%) to use Q", 60));
                    ModesMenu.Add("LaneClear.Q.HitMin", new Slider("Minimal minions hit to use Q : {0}", 3, 1, 12));
                }

                public static void InitializeLaneClear()
                {
                }
            }

            public static class Flee
            {
                public static bool UseQ => ModesMenu["Flee.Q"].Cast<CheckBox>().CurrentValue;

                static Flee()
                {
                    ModesMenu.AddGroupLabel("Flee");
                    ModesMenu.Add("Flee.Q", new CheckBox("Use Q"));
                }

                public static void InitializeFlee()
                {
                }
            }
        }
    }
}

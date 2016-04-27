using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SimpleRyze.Utility;

namespace SimpleRyze
{
    public static class Config
    {
        private const string MenuName = "Simple Ryze";

        private static readonly Menu Menu;
        
        public static List<Gapclosers.GapcloserMenuInfo> AntiGapcloserMenuValues = new List<Gapclosers.GapcloserMenuInfo>();


        static Config()
        {
            Menu = MainMenu.AddMenu(MenuName, MenuName.ToLower());

            Modes.InitializeModes();
            GapcloserMenu.Initializer();
            AutoHarass.Initializer();
            Drawings.Initializer();
            ExtrasMenu.Initializer();
            MiscMenu.Initializer();
        }

        public static void Initialize()
        {
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
            /// Returns true if Use W is enabled.
            /// </summary>
            public static bool UseW => AutoharassMenu["AutoHarass.UseW"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns minimal range to use W.
            /// </summary>
            public static int WRange => AutoharassMenu["AutoHarass.UseWRange"].Cast<Slider>().CurrentValue;

            /// <summary>
            /// Use W On 
            /// </summary>
            /// <param name="championName">Enemy champion name</param>
            /// <returns><c>True</c> if checkbox is true</returns>
            public static bool AutoHarassEnabledFor(string championName)
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
                AutoharassMenu.AddLabel("Uses Q on enemy only if you're not in LaneClear/Harass/Combo mode");
                AutoharassMenu.AddSeparator(1);
                AutoharassMenu.Add("AutoHarass.ManaQ", new Slider("Minimal mana ({0}%) to use Q", 80));
                AutoharassMenu.Add("AutoHarass.UseW", new CheckBox("Use W as soon as enemy is in range", false));
                AutoharassMenu.Add("AutoHarass.UseWRange", new Slider("Range {0} units from player", 400, 0, (int)SpellManager.W.Range));
                AutoharassMenu.AddSeparator();
                AutoharassMenu.AddLabel("Auto harass enabled for : ");

                foreach (var hero in EntityManager.Heroes.Enemies)
                {
                    AutoharassMenu.Add("Enabled." + hero.BaseSkinName, new CheckBox(hero.BaseSkinName));
                }
            }
        }

        /// <summary>
        /// AutoHarass Menu
        /// </summary>
        public static class ExtrasMenu
        {
            /// <summary>
            /// Returns Extras.FarmQ value.
            /// </summary>
            public static bool UseQToFarm => extrasMenu["Extras.FarmQ"].Cast<KeyBind>().CurrentValue;

            /// <summary>
            /// Returns minimal mana to use Q.
            /// </summary>
            public static int ManaForQ => extrasMenu["Extras.FarmQMana"].Cast<Slider>().CurrentValue;

            /// <summary>
            /// Returns Extras.FarmQStacks value.
            /// </summary>
            public static int MaxPassiveStacksForQFarm => extrasMenu["Extras.FarmQStacks"].Cast<Slider>().CurrentValue;

            /// <summary>
            /// Returns Extras.StackTear value.
            /// </summary>
            public static bool StackTear => extrasMenu["Extras.StackTear"].Cast<KeyBind>().CurrentValue;

            /// <summary>
            /// Returns Extras.StackOnlyInFountain value.
            /// </summary>
            public static bool StackOnlyInFountain => extrasMenu["Extras.StackOnlyInFountain"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns Extras.StackTill value.
            /// </summary>
            public static int MaxPassiveStacksForTearStacking => extrasMenu["Extras.StackTill"].Cast<Slider>().CurrentValue;

            /// <summary>
            /// Returns Extras.StackMana value.
            /// </summary>
            public static int StackTearMinMana => extrasMenu["Extras.StackMana"].Cast<Slider>().CurrentValue;


            /// <summary>
            /// Returns Extras.UseSeraph value.
            /// </summary>
            public static bool UseSeraph => extrasMenu["Extras.UseSeraph"].Cast<CheckBox>().CurrentValue;


            /// <summary>
            /// Returns Extras.BlockAAs value.
            /// </summary>
            public static bool BlockAaInCombo => extrasMenu["Extras.BlockAAs"].Cast<CheckBox>().CurrentValue;


            /// <summary>
            /// Returns Extras.RandomizeDelays value.
            /// </summary>
            public static bool EnableHumanizer => extrasMenu["Extras.RandomizeDelays"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Use W On 
            /// </summary>
            /// <param name="championName">Enemy champion name</param>
            /// <returns><c>True</c> if checkbox is true</returns>
            public static bool UseWOn(string championName)
            {
                return extrasMenu["Extras.UseWOn." + championName] != null && extrasMenu["Extras.UseWOn." + championName].Cast<CheckBox>().CurrentValue;
            }

            // ReSharper disable once InconsistentNaming
            private static readonly Menu extrasMenu;

            /// <summary>
            /// Static initializer
            /// </summary>
            public static void Initializer()
            {
            }

            static ExtrasMenu()
            {
                extrasMenu = Menu.AddSubMenu("Extras");

                extrasMenu.Add("Extras.FarmQ", new KeyBind("Auto farm with Q", true, KeyBind.BindTypes.PressToggle, 'H')).OnValueChange +=
                    delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                    {
                        Helpers.PrintInfoMessage(
                            args.NewValue
                                ? "Q auto farm : <font color =\"#32BA1A\"><b>Enabled</b></font>"
                                : "Q auto farm : <font color =\"#E02007\"><b>Disabled</b></font>", false);
                    };

                extrasMenu.AddLabel("Uses Q on killable minion only if you're not in LaneClear/Harass/Combo mode");
                extrasMenu.AddSeparator(1);
                extrasMenu.Add("Extras.FarmQMana", new Slider("Minimal mana ({0}%) to use Q", 80));
                extrasMenu.Add("Extras.FarmQStacks", new Slider("Farm with Q till {0} passive stacks are reached", 3, 1, 4));
                extrasMenu.Add("Extras.StackTear", new KeyBind("Stack Tear / Archangels", true, KeyBind.BindTypes.PressToggle, 'U')).OnValueChange +=
                    delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                    {
                        Helpers.PrintInfoMessage(
                            args.NewValue
                                ? "Stack tear : <font color =\"#32BA1A\"><b>Enabled</b></font>"
                                : "Stack tear : <font color =\"#E02007\"><b>Disabled</b></font>", false);
                    };
                extrasMenu.Add("Extras.StackOnlyInFountain", new CheckBox("Only in fountain"));
                extrasMenu.Add("Extras.StackTill", new Slider("Stack tear with Q till {0} passive stacks are reached", 3, 1, 4));
                extrasMenu.Add("Extras.StackMana", new Slider("Minimal mana ({0}%) to use Q for Tear / Archangels", 80));
                extrasMenu.Add("Extras.UseSeraph", new CheckBox("Use Seraphs when in danger"));
                extrasMenu.Add("Extras.BlockAAs", new CheckBox("Block autoattack while in combo"));
                extrasMenu.Add("Extras.RandomizeDelays", new CheckBox("Randomize delays between spells", false));
                extrasMenu.AddSeparator();

                extrasMenu.AddLabel("Use W on : ");
                foreach (var hero in EntityManager.Heroes.Enemies)
                {
                    extrasMenu.Add("Extras.UseWOn." + hero.BaseSkinName, new CheckBox(hero.BaseSkinName));
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
            /// Returns true if DrawE is enabled.
            /// </summary>
            public static bool DrawQ => DrawingsMenu["DrawQ"].Cast<CheckBox>().CurrentValue;

            /// <summary>
            /// Returns true if DrawW is enabled.
            /// </summary>
            public static bool DrawW => DrawingsMenu["DrawW"].Cast<CheckBox>().CurrentValue;
            
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

            /// <summary>
            /// Returns true if HPBarIndicator is enabled.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            public static bool EnableHPBarIndicator => DrawingsMenu["HPBarIndicator"].Cast<CheckBox>().CurrentValue;

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
                DrawingsMenu.Add("DrawQ", new CheckBox("Draw Q range"));
                DrawingsMenu.Add("DrawW", new CheckBox("Draw W range"));
                DrawingsMenu.Add("DrawingBorderWidth", new Slider("Border Width", 2, 1, 10));
                DrawingsMenu.AddSeparator(5);
                DrawingsMenu.Add("DrawRTime", new CheckBox("Draw R expiry time"));
                DrawingsMenu.Add("DrawPermashow", new CheckBox("Enable PermaShow"));
                DrawingsMenu.Add("HPBarIndicator", new CheckBox("Enable HPBarIndicator"));
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

            public static bool KillstealQ => miscMenu["KillstealQ"].Cast<CheckBox>().CurrentValue;
            public static bool KillstealW => miscMenu["KillstealW"].Cast<CheckBox>().CurrentValue;
            public static bool KillstealE => miscMenu["KillstealE"].Cast<CheckBox>().CurrentValue;

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

                miscMenu.Add("SkinID", new ComboBox("Skin : ", new[] {"Classic", "Human", "Tribal", "Uncle", "Triumphant", "Professor", "Zombie", "Dark Crystal", "Pirate" })).OnValueChange +=
                    delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                    {
                        if (SkinHackEnabled && Player.Instance.SkinId != SkinId)
                        {
                            Player.SetSkin(Player.Instance.BaseSkinName, args.NewValue);
                        }
                    };

                miscMenu.Add("Permashow.X", new Slider("Permashow position X: ", (int)(Drawing.Width*0.86f), 0, Drawing.Width)).OnValueChange +=
                    delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                    {
                        PermaShow.Position[0] = args.NewValue;
                    };
                miscMenu.Add("Permashow.Y", new Slider("Permashow position Y: ", (int)(Drawing.Height * 0.08f), 0, Drawing.Height)).OnValueChange +=
                    delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                    {
                        PermaShow.Position[1] = args.NewValue;
                    };
                miscMenu.AddSeparator(5);
                miscMenu.AddGroupLabel("Killsteal settings : ");
                miscMenu.AddSeparator(5);
                miscMenu.Add("KillstealQ", new CheckBox("Use Q to secure a kill"));
                miscMenu.Add("KillstealW", new CheckBox("Use W to secure a kill"));
                miscMenu.Add("KillstealE", new CheckBox("Use E to secure a kill"));
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

                static Combo()
                {
                    ModesMenu.AddGroupLabel("Combo");
                    ModesMenu.Add("Combo.Q", new CheckBox("Use Q"));
                    ModesMenu.Add("Combo.W", new CheckBox("Use W"));
                    ModesMenu.Add("Combo.E", new CheckBox("Use E"));
                    ModesMenu.Add("Combo.R", new CheckBox("Use R"));
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
                public static bool UseE => ModesMenu["Harass.E"].Cast<CheckBox>().CurrentValue;
                public static int EMana => ModesMenu["Harass.E.Mana"].Cast<Slider>().CurrentValue;

                static Harass()
                {
                    ModesMenu.AddGroupLabel("Harass");
                    ModesMenu.Add("Harass.Q", new CheckBox("Use Q"));
                    ModesMenu.Add("Harass.Q.Mana", new Slider("Minimal mana ({0}%) to use Q", 60));
                    ModesMenu.Add("Harass.W", new CheckBox("Use W"));
                    ModesMenu.Add("Harass.W.Mana", new Slider("Minimal mana ({0}%) to use W", 40));
                    ModesMenu.Add("Harass.E", new CheckBox("Use E"));
                    ModesMenu.Add("Harass.E.Mana", new Slider("Minimal mana ({0}%) to use E", 40));
                }

                public static void InitializeHarass()
                {
                }
            }

            public static class LaneClear
            {
                public static bool UseQ => ModesMenu["LaneClear.Q"].Cast<CheckBox>().CurrentValue;
                public static bool UnderTowerFarm => ModesMenu["LaneClear.Tower"].Cast<CheckBox>().CurrentValue;
                public static int QMana => ModesMenu["LaneClear.Q.Mana"].Cast<Slider>().CurrentValue;
                public static int EMana => ModesMenu["LaneClear.E.Mana"].Cast<Slider>().CurrentValue;
                public static bool UseE => ModesMenu["LaneClear.E"].Cast<CheckBox>().CurrentValue;
                public static bool UseR => ModesMenu["LaneClear.R"].Cast<CheckBox>().CurrentValue;
                public static bool ROnlyIfNoEnemiesNear => ModesMenu["LaneClear.REnemies"].Cast<CheckBox>().CurrentValue;

                static LaneClear()
                {
                    ModesMenu.AddGroupLabel("Lane clear / Jungle clear");
                    ModesMenu.Add("LaneClear.Q", new CheckBox("Use Q"));
                    ModesMenu.Add("LaneClear.Q.Mana", new Slider("Minimal mana ({0}%) to use Q", 60));
                    ModesMenu.Add("LaneClear.E", new CheckBox("Use E"));
                    ModesMenu.Add("LaneClear.E.Mana", new Slider("Minimal mana ({0}%) to use E", 60));
                    ModesMenu.Add("LaneClear.R", new CheckBox("Use R"));
                    ModesMenu.Add("LaneClear.REnemies", new CheckBox("Use R only if no enemies are near"));
                    ModesMenu.Add("LaneClear.Tower", new CheckBox("Dont use any skills under tower"));
                }

                public static void InitializeLaneClear()
                {
                }
            }

            public static class Flee
            {
                public static bool UseW => ModesMenu["Flee.W"].Cast<CheckBox>().CurrentValue;

                static Flee()
                {
                    ModesMenu.AddGroupLabel("Flee");
                    ModesMenu.Add("Flee.W", new CheckBox("Use W"));
                }

                public static void InitializeFlee()
                {
                }
            }
        }
    }
}
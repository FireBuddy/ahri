using System;
using EloBuddy;
using EloBuddy.SDK;
using Settings = SimpleRyze.Config.ExtrasMenu;

namespace SimpleRyze.Utility
{
    public static class TearStack
    {
        // ReSharper disable once InconsistentNaming
        private static int LastTick;
        // ReSharper disable once InconsistentNaming
        private static Random Random;

        public static void Initializer()
        {
            Random = new Random();
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Player.Instance.IsDead || !Settings.StackTear || LastTick + Random.Next(200, 250) > Game.Time*1000 || !SpellManager.Q.IsReady() || !Helpers.IsTearReady ||
                Helpers.GetTearSpellSlot == SpellSlot.Unknown ||
                (Settings.StackOnlyInFountain && !Player.Instance.IsInShopRange()) ||
                Player.Instance.ManaPercent < Settings.StackTearMinMana || Helpers.GetPassiveStacks >= Settings.MaxPassiveStacksForTearStacking)
                return;

            
            if (Player.Instance.CountEnemiesInRange(3000) == 0)
                Core.DelayAction(() => SpellManager.Q.Cast(Player.Instance.ServerPosition.Extend(Game.CursorPos, Random.Next(200, 400)).To3D()), Random.Next(100, 300));

            LastTick = (int)Game.Time*1000;
        }
    }
}
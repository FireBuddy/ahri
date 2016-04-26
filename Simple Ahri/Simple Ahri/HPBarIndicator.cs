using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleAhri
{
    class HPBarIndicator
    {
        public static void Initalize()
        {
            Drawing.OnEndScene += DrawingOnEndScene;
        }

        private static void DrawingOnEndScene(EventArgs args)
        {
            foreach (var unit in
                EntityManager.Heroes.Enemies.Where(
                    index => index.IsHPBarRendered && index.IsValidTarget(1200)))
            {
                var damage = unit.GetComboDamage();

                if (damage <= 0)
                {
                    return;
                }

                const int height = 9;
                const int width = 104;
                var xOffset = unit.Hero == Champion.Jhin ? -9 : 2;
                var yOffset = unit.Hero == Champion.Jhin ? -5 : 9;

                var damageAfter = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                var currentHealth = unit.Health/unit.MaxHealth;

                var start = new Vector2(unit.HPBarPosition.X + xOffset + damageAfter * width, unit.HPBarPosition.Y + yOffset);
                var end = new Vector2(unit.HPBarPosition.X + currentHealth * width, unit.HPBarPosition.Y + yOffset);
                
                Line.DrawLine(Color.FromArgb(215, 255, 215, 0), height, start, end);
            }
        }
    }
}
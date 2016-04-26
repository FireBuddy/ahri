using System;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK.Rendering;
using SharpDX;
using SimpleRyze.Properties;

namespace SimpleRyze
{
    public class PermaShow
    {
        public static readonly TextureLoader TextureLoader = new TextureLoader();

        public static Vector2 Position;
        private static Sprite Sprite { get; set; }
        private static Text Text { get; set; }

        public static void Initalize()
        {
            TextureLoader.Load("sprite", Resources.sprite);

            Sprite = new Sprite(TextureLoader["sprite"]);

            Text = new Text("", new Font("Tahoma", 9, FontStyle.Regular));

            Position = new Vector2(Config.MiscMenu.PermashowX, Config.MiscMenu.PermashowY);
            
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Config.Drawings.DrawPermashow)
                return;
            

            Sprite.Draw(Position);
            Text.Draw("Simple Ryze : Permashow", System.Drawing.Color.GreenYellow, (int)Position.X + 15, (int)Position.Y + 5);
            Text.Draw("Simple Ryze : Auto harass : ", System.Drawing.Color.White, (int)Position.X + 15, (int)Position.Y + 33);
            Text.Draw(Config.AutoHarass.Enabled ? "Enabled" : "Disabled", Config.AutoHarass.Enabled ? System.Drawing.Color.GreenYellow : System.Drawing.Color.Red, (int)Position.X + 20 + 149, (int)Position.Y + 33);
            Text.Draw("\nSimple Ryze : Q auto farm : ", System.Drawing.Color.White, (int)Position.X + 15, (int)Position.Y + 33);
            Text.Draw(Config.ExtrasMenu.UseQToFarm ? "\nEnabled" : "\nDisabled", Config.ExtrasMenu.UseQToFarm ? System.Drawing.Color.GreenYellow : System.Drawing.Color.Red, (int)Position.X + 20 + 149, (int)Position.Y + 33);
            Text.Draw("\n\nSimple Ryze : Stack tear : ", System.Drawing.Color.White, (int)Position.X + 15, (int)Position.Y + 33);
            Text.Draw(Config.ExtrasMenu.StackTear ? "\n\nEnabled" : "\n\nDisabled", Config.ExtrasMenu.StackTear ? System.Drawing.Color.GreenYellow : System.Drawing.Color.Red, (int)Position.X + 20 + 149, (int)Position.Y + 33);
        }
    }
}
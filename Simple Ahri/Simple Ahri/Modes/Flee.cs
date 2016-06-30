using EloBuddy;
using EloBuddy.SDK;

namespace SimpleAhri.Modes
{
    public sealed class Flee : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee);
        }

        public override void Execute()
        {
            var target = Program.CurrentTarget;
            if (Q.IsReady() && Config.Modes.Flee.UseQ && target.IsInRange(Player.Instance, Q.Range + 150))
            {
                Q.Cast(target);
            }
            
            else if (Q.IsReady() && Config.Modes.Flee.UseQ)
            {
                Q.Cast(Player.Instance.Position.Extend(Game.CursorPos, -300).To3D());
            }
        }
    }
}

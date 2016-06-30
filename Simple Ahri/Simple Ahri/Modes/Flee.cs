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
            
            
            {
                var target = Program.CurrentTarget;
                if (Q.IsReady() && Config.Modes.Flee.UseQ && target.IsInRange(Player.Instance, Q.Range + 300))
                Q.Cast(target);
            }
            
            
            

            
        }
    }
}

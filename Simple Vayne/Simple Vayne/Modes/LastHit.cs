using EloBuddy.SDK;

namespace Simple_Vayne.Modes
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LastHit : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit);
        }

        public override void Execute()
        {
        }
    }
}
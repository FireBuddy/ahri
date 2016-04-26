using EloBuddy.SDK;

namespace SimpleRyze.Modes
{
    public abstract class ModeBase
    {
        protected Spell.Skillshot Q => SpellManager.Q;
        
        protected Spell.Targeted W => SpellManager.W;

        protected Spell.Targeted E => SpellManager.E;

        protected Spell.Active R => SpellManager.R;

        public abstract bool ShouldBeExecuted();

        public abstract void Execute();
    }
}
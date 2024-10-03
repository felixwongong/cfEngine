using System;

namespace cfEngine.Util
{
    public abstract class State<TStateId>
    {
        public abstract TStateId id { get; }
        protected internal abstract void StartContext(StateMachine<TStateId> sm, StateParam param);

        protected internal virtual void OnEndContext()
        {
        }
    }
}
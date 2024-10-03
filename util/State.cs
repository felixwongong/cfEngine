using System;

namespace cfEngine.Util
{
    public abstract class State<TStateId> where TStateId : Enum
    {
        public abstract TStateId id { get; }
        protected internal abstract void StartContext(StateMachine<TStateId> sm, StateParam param);

        protected internal virtual void OnEndContext()
        {
        }
    }
}
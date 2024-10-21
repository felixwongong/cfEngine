using System;
using System.Collections.Generic;

namespace cfEngine.Util
{
    public abstract class State<TStateId, TState, TStateMachine>: IDisposable
        where TStateMachine: StateMachine<TStateId, TState, TStateMachine> 
        where TState : State<TStateId, TState, TStateMachine>
    {
        public abstract TStateId Id { get; }
        public virtual HashSet<TStateId> Whitelist { get; } = new();
        protected internal abstract void StartContext(TStateMachine gsm, StateParam param);

        protected internal virtual void OnEndContext()
        {
        }

        public virtual void Dispose(){}
    }
}
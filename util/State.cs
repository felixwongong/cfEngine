using System;

namespace cfEngine.Util
{
    public abstract class State<TStateId, TState, TStateMachine>: IDisposable
        where TStateMachine: StateMachine<TStateId, TState, TStateMachine> 
        where TState : State<TStateId, TState, TStateMachine>
    {
        public abstract TStateId Id { get; }

        public TStateMachine StateMachine { get; internal set; }

        public virtual bool IsReady()
        {
            return true;
        }

        public abstract void StartContext(StateParam param);

        protected internal virtual void OnEndContext()
        {
        }

        public virtual void Dispose(){}
    }
}
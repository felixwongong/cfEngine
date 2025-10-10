using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Util
{
    public class SequentialStateMachine<TStateId, TState, TStateMachine>: StateMachine<TStateId, TState, TStateMachine> 
        where TState : SequentialState<TStateId, TState, TStateMachine>
        where TStateMachine : SequentialStateMachine<TStateId, TState, TStateMachine>
    {
        private readonly SortedList<int, TStateId> _sortedStateId;

        public SequentialStateMachine() : base()
        {
            _sortedStateId = new SortedList<int, TStateId>();
        }

        public override void RegisterState(TState state)
        {
            base.RegisterState(state);
            _sortedStateId.Add(state.order, state.id);
        }

        public virtual void Start()
        {
            if (_sortedStateId.Count == 0)
            {
                Log.LogError("No state availabled yet, cannot start");
                return;
            }
            
            ForceGoToState(_sortedStateId[0]);
        }

        public Res<TStateId, Exception> GetNextState()
        {
            var currentIndex = _sortedStateId.IndexOfValue(currentStateId);
            if (currentIndex < 0)
                return new KeyNotFoundException("Current state not found in the sequence.");

            if (currentIndex >= _sortedStateId.Count - 1)
                return new Exception("Already at the last state, no next state available.");
            
            return _sortedStateId[currentIndex + 1];
        }
        
        public bool IsLastState()
        {
            var currentIndex = _sortedStateId.IndexOfValue(currentStateId);
            if (currentIndex < 0)
                throw new KeyNotFoundException("Current state not found in the sequence.");

            return currentIndex >= _sortedStateId.Count - 1;
        }
    }

    public abstract class SequentialState<TStateId, TState, TStateMachine> : State<TStateId, TState, TStateMachine>
        where TState : SequentialState<TStateId, TState, TStateMachine>
        where TStateMachine : SequentialStateMachine<TStateId, TState, TStateMachine>
    {
        public abstract int order { get; }
        
        public bool IsLastState()
        {
            return stateMachine.IsLastState();
        }
    }
}
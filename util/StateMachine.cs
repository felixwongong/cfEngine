using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using cfEngine.Logging;

namespace cfEngine.Util
{
    public class StateParam
    {
    }

    public class StateMachine<TStateId>
    {
        public struct StateChangeRecord<TStateId>
        {
            public State<TStateId> LastState;
            public State<TStateId> NewState;
        }

        private State<TStateId> _lastState;
        private State<TStateId> _currentState;
        public State<TStateId> LastState => _lastState;
        public State<TStateId> CurrentState => _currentState;

        private readonly Dictionary<TStateId, State<TStateId>> _stateDictionary = new();

        public event Action<StateChangeRecord<TStateId>> onBeforeStateChange;
        public event Action<StateChangeRecord<TStateId>> onAfterStateChange;

        public StateMachine()
        {
        }

        public void RegisterState([NotNull] State<TStateId> state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (_stateDictionary.ContainsKey(state.id))
            {
                throw new Exception($"State {state.GetType()} already registered");
            }

            _stateDictionary[state.id] = state;
        }

        public void GoToState(TStateId id, in StateParam param = null)
        {
            try
            {
                if (!_stateDictionary.TryGetValue(id, out var currentState))
                    throw new KeyNotFoundException($"State {id} not registered");

                onBeforeStateChange?.Invoke(new StateChangeRecord<TStateId>()
                    { LastState = _currentState, NewState = currentState });

                if (_currentState != null)
                {
                    _currentState.OnEndContext();
                    _lastState = _currentState;
                }

                _currentState = currentState;
                _currentState.StartContext(this, param);

                onAfterStateChange?.Invoke(new StateChangeRecord<TStateId>()
                    { LastState = _lastState, NewState = _currentState });

            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        public void GoToStateNoRepeat(TStateId id, in StateParam param = null)
        {
            if (_currentState.id.Equals(id))
                GoToState(id, param);
        }

        public T GetState<T>(TStateId id) where T : State<TStateId>
        {
            if (!_stateDictionary.TryGetValue(id, out var state))
            {
                throw new Exception($"State {typeof(T)} not registered");
            }

            return (T)state;
        }
    }
}
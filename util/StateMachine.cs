using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using cfEngine.Logging;

namespace cfEngine.Util
{
    public struct StateChangeRecord<TStateId>
    {
        public TStateId LastState;
        public TStateId NewState;
    }

    public interface IStateMachine<TStateId>
    {
        public TStateId LastStateId { get; }
        public TStateId CurrentStateId { get; }
        public event Action<StateChangeRecord<TStateId>> OnBeforeStateChange;
        public event Action<StateChangeRecord<TStateId>> OnAfterStateChange;
        public bool CanGoToState(TStateId id);
        public void GoToState(TStateId nextStateId, in StateParam param = null, bool checkWhitelist = true);
    }
    
    public class StateExecutionException<TStateId> : Exception
    {
        public StateExecutionException(TStateId stateId, Exception innerException): base($"State {stateId} execution failed", innerException)
        {
        }
    }
    
    public class StateParam
    {
    }

    public class StateMachine<TStateId, TState, TStateMachine>: IStateMachine<TStateId>, IDisposable
        where TStateMachine: StateMachine<TStateId, TState, TStateMachine>
        where TState: State<TStateId, TState, TStateMachine>
    {
        private TStateId _lastStateId;
        private TStateId _currentStateId;
        public TStateId LastStateId => _lastStateId;
        public TStateId CurrentStateId => _currentStateId;
        
        private readonly Dictionary<TStateId, TState> _stateDictionary = new();

        public event Action<StateChangeRecord<TStateId>> OnBeforeStateChange;
        public event Action<StateChangeRecord<TStateId>> OnAfterStateChange;

        public StateMachine()
        {
        }

        public void RegisterState([NotNull] TState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!_stateDictionary.TryAdd(state.Id, state))
            {
                throw new Exception($"State {state.GetType()} already registered");
            }
        }
        
        public bool CanGoToState(TStateId id)
        {
            return TryGetState(id, out _) && GetState(_currentStateId).Whitelist.Contains(id);
        }

        public void GoToState(TStateId nextStateId, in StateParam param = null, bool checkWhitelist = true)
        {
            if (!TryGetState(nextStateId, out var nextState))
            {
                Log.LogException(new KeyNotFoundException($"State {nextStateId} not registered"));
                return;
            }

            if (TryGetState(_currentStateId, out var currentState))
            {
                if (checkWhitelist && !CanGoToState(nextState.Id))
                {
                    Log.LogException(new ArgumentException(
                        $"Cannot go to state {nextState.Id}, not in current state {currentState.Id} whitelist"));
                    return;
                }

                OnBeforeStateChange?.Invoke(new StateChangeRecord<TStateId>
                    { LastState = currentState.Id, NewState = nextState.Id });

                currentState.OnEndContext();
                _lastStateId = currentState.Id;
            }

            currentState.StartContext((TStateMachine)this, param);
            _currentStateId = nextState.Id;

            OnAfterStateChange?.Invoke(new StateChangeRecord<TStateId>
                { LastState = currentState.Id, NewState = nextState.Id });
        }

        public void GoToStateNoRepeat(TStateId id, in StateParam param = null)
        {
            if (_currentStateId.Equals(id))
                GoToState(id, param);
        }

        public TState GetState(TStateId id)
        {
            if (!_stateDictionary.TryGetValue(id, out var state))
            {
                throw new Exception($"State {id} not registered");
            }

            return state;
        }

        public T GetState<T>(TStateId id) where T : TState
        {
            return (T)GetState(id);
        }
        
        public bool TryGetState(TStateId id, out TState state)
        {
            if (!_stateDictionary.TryGetValue(id, out state))
            {
                Log.LogException(new Exception($"State {id} not registered"));
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            foreach (var state in _stateDictionary.Values)
            {
                state.Dispose();
            }
            
            _stateDictionary.Clear();
        }
    }
}
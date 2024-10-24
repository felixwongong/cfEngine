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
        private TState _lastState;
        private TState _currentState;
        public TStateId LastStateId => _lastState.Id;
        public TStateId CurrentStateId => _currentState.Id;
        
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
            return TryGetState(id, out _) && (_currentState == null || _currentState.Whitelist.Contains(id));
        }

        public void GoToState(TStateId nextStateId, in StateParam param = null, bool checkWhitelist = true)
        {
            if (!TryGetState(nextStateId, out var nextState))
            {
                Log.LogException(new KeyNotFoundException($"State {nextStateId} not registered"));
                return;
            }

            if (checkWhitelist && !CanGoToState(nextState.Id))
            {
                Log.LogException(new ArgumentException(
                    $"Cannot go to state {nextState.Id}, not in current state {_currentState.Id} whitelist"));
                return;
            }

            if (_currentState != null)
            {
                OnBeforeStateChange?.Invoke(new StateChangeRecord<TStateId>
                    { LastState = _currentState.Id, NewState = nextState.Id });
                
                _currentState.OnEndContext();
                _lastState = _currentState;
            }
            
            nextState.StartContext((TStateMachine)this, param);
            _currentState = nextState;

            if (_lastState != null)
            {
                OnAfterStateChange?.Invoke(new StateChangeRecord<TStateId>
                    { LastState = _lastState.Id, NewState = _currentState.Id });
            }
        }

        public void GoToStateNoRepeat(TStateId id, in StateParam param = null)
        {
            if (_currentState.Equals(id))
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
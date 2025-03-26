using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using cfEngine.Logging;
using cfEngine.Rx;

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
        public bool TryGoToState(TStateId nextStateId, in StateParam param = null);
        public void ForceGoToState(TStateId nextStateId, in StateParam param = null);
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

        #region Relay & Events (OnBeforeStateChange[Once], OnAfterStateChange[Once]);

        private Relay<StateChangeRecord<TStateId>> _beforeStateChangeRelay;
        private Relay<StateChangeRecord<TStateId>> _afterStateChangeRelay;
        
        public event Action<StateChangeRecord<TStateId>> OnBeforeStateChange
        {
            add
            {
                _beforeStateChangeRelay ??= new Relay<StateChangeRecord<TStateId>>(this);
                _beforeStateChangeRelay.AddListener(value);
            }
            remove => _beforeStateChangeRelay.RemoveListener(value);
        }

        public event Action<StateChangeRecord<TStateId>> OnAfterStateChange
        {
            add
            {
                _afterStateChangeRelay ??= new Relay<StateChangeRecord<TStateId>>(this);
                _afterStateChangeRelay.AddListener(value);
            }
            remove => _afterStateChangeRelay.RemoveListener(value);
        }

        #endregion

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

            state.StateMachine = (TStateMachine)this;
        }
        
        public bool CanGoToState(TStateId id)
        {
            return TryGetState(id, out var nextState) && nextState.IsReady() && (_currentState == null || _currentState.Whitelist.Contains(id));
        }

        public bool TryGoToState(TStateId nextStateId, in StateParam param = null)
        {
            try
            {
                if (!TryGetState(nextStateId, out var nextState))
                {
                    Log.LogException(new KeyNotFoundException($"State {nextStateId} not registered"));
                    return false;
                }

                if (!CanGoToState(nextState.Id))
                {
                    Log.LogException(new ArgumentException(
                        $"Cannot go to state {nextState.Id}, not in current state {_currentState.Id} whitelist"));
                    return false;
                }

                if (_currentState != null)
                {
                    _beforeStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _currentState.Id, NewState = nextState.Id });
                
                    _currentState.OnEndContext();
                    _lastState = _currentState;
                }
            
                _currentState = nextState;
                if (_lastState != null)
                {
                    _afterStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _lastState.Id, NewState = _currentState.Id });
                }
                _currentState.StartContext(param);

                return true;
            }
            catch (Exception e)
            {
                Log.LogException(e);
                return false;
            }
        }

        public void ForceGoToState(TStateId nextStateId, in StateParam param = null)
        {
            try
            {
                if (!TryGetState(nextStateId, out var nextState))
                {
                    Log.LogException(new KeyNotFoundException($"State {nextStateId} not registered"));
                    return;
                }

                if (_currentState != null)
                {
                    _beforeStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _currentState.Id, NewState = nextState.Id });
                
                    _currentState.OnEndContext();
                    _lastState = _currentState;
                }
            
                _currentState = nextState;
                if (_lastState != null)
                {
                    _afterStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _lastState.Id, NewState = _currentState.Id });
                }
                _currentState.StartContext(param);
            }
            catch (Exception e)
            {
                Log.LogException(e);
            }
        }

        public TState GetStateUnsafe(TStateId id)
        {
            if (!_stateDictionary.TryGetValue(id, out var state))
            {
                throw new Exception($"State {id} not registered");
            }

            return state;
        }

        public T GetStateUnsafe<T>(TStateId id) where T : TState
        {
            return (T)GetStateUnsafe(id);
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace IIMLib.Loop
{
    public class GameLoopServiceIIM<T> : IGameLoopService<GameLoopType> where T : GameManagerIIMAbstract
    {
        private struct Subscriber : IEquatable<Subscriber>
        {
            public Action<float> Action;
            public bool IsActive;

            public bool SetActive(in bool value) => IsActive = value;

            public bool Equals(Subscriber other)
            {
                return Equals(Action, other.Action) && IsActive == other.IsActive;
            }

            public override bool Equals(object obj)
            {
                return obj is Subscriber other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Action, IsActive);
            }
        }

        private readonly Dictionary<object, HashSet<Subscriber>> _UpdateSubscribers = new();
        private readonly Dictionary<object, HashSet<Subscriber>> _FixedUpdateSubscribers = new();
        private readonly Dictionary<object, HashSet<Subscriber>> _LateUpdateSubscribers = new();

        private bool _Update_IsDirty = true, _FixedUpdate_IsDirty = true, _LateUpdate_IsDirty = true;

        private IEnumerable<Subscriber> _UpdateSubscribers_Cache;
        private IEnumerable<Subscriber> _FixedUpdateSubscribers_Cache;
        private IEnumerable<Subscriber> _LateUpdateSubscribers_Cache;

        private IEnumerable<Subscriber> UpdateSubscribers
        {
            get
            {
                if (!_Update_IsDirty) return _UpdateSubscribers_Cache;

                _UpdateSubscribers_Cache = LayOutActiveSubscribers(_UpdateSubscribers);
                _Update_IsDirty = false;

                return _UpdateSubscribers_Cache;
            }
        }

        private IEnumerable<Subscriber> FixedUpdateSubscribers
        {
            get
            {
                if (!_FixedUpdate_IsDirty) return _FixedUpdateSubscribers_Cache;

                _FixedUpdateSubscribers_Cache = LayOutActiveSubscribers(_FixedUpdateSubscribers);
                _FixedUpdate_IsDirty = false;

                return _FixedUpdateSubscribers_Cache;
            }
        }

        private IEnumerable<Subscriber> LateUpdateSubscribers
        {
            get
            {
                if (!_LateUpdate_IsDirty) return _LateUpdateSubscribers_Cache;

                _LateUpdateSubscribers_Cache = LayOutActiveSubscribers(_LateUpdateSubscribers);
                _LateUpdate_IsDirty = false;

                return _LateUpdateSubscribers_Cache;
            }
        }

        public void Initialize() { }

        public void Subscribe(object subscriber, Action<float> action, GameLoopType type)
        {
            if (action == null) return;

            switch (type)
            {
                case GameLoopType.UPDATE:
                    if (_UpdateSubscribers.TryGetValue(subscriber, out var update))
                        update.Add(new Subscriber {Action = action, IsActive = true});
                    else
                        _UpdateSubscribers[subscriber] = new HashSet<Subscriber>
                            {new Subscriber {Action = action, IsActive = true}};
                    break;
                case GameLoopType.FIXED_UPDATE:
                    if (_FixedUpdateSubscribers.TryGetValue(subscriber, out var fixedUpdate))
                        fixedUpdate.Add(new Subscriber {Action = action, IsActive = true});
                    else
                        _FixedUpdateSubscribers[subscriber] = new HashSet<Subscriber>
                            {new Subscriber {Action = action, IsActive = true}};
                    break;
                case GameLoopType.LATE_UPDATE:
                    if (_LateUpdateSubscribers.TryGetValue(subscriber, out var lateUpdate))
                        lateUpdate.Add(new Subscriber {Action = action, IsActive = true});
                    else
                        _LateUpdateSubscribers[subscriber] = new HashSet<Subscriber>
                            {new Subscriber {Action = action, IsActive = true}};
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void UnSubscribe(object subscriber, GameLoopType type)
        {
            switch (type)
            {
                case GameLoopType.UPDATE:
                    _UpdateSubscribers.Remove(subscriber);
                    break;
                case GameLoopType.FIXED_UPDATE:
                    _FixedUpdateSubscribers.Remove(subscriber);
                    break;
                case GameLoopType.LATE_UPDATE:
                    _LateUpdateSubscribers.Remove(subscriber);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void UnSubscribe(object subscriber, Action<float> action, GameLoopType type)
        {
            switch (type)
            {
                case GameLoopType.UPDATE:
                    RemoveSubscription(subscriber, action, _UpdateSubscribers);
                    break;
                case GameLoopType.FIXED_UPDATE:
                    RemoveSubscription(subscriber, action, _FixedUpdateSubscribers);
                    break;
                case GameLoopType.LATE_UPDATE:
                    RemoveSubscription(subscriber, action, _LateUpdateSubscribers);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void UnSubscribe(object subscriber)
        {
            _UpdateSubscribers.Remove(subscriber);
            _FixedUpdateSubscribers.Remove(subscriber);
            _LateUpdateSubscribers.Remove(subscriber);
        }

        public void UnSubscribe(object subscriber, Action<float> action)
        {
            RemoveSubscription(subscriber, action, _UpdateSubscribers);
            RemoveSubscription(subscriber, action, _FixedUpdateSubscribers);
            RemoveSubscription(subscriber, action, _LateUpdateSubscribers);

            _Update_IsDirty = _FixedUpdate_IsDirty = _LateUpdate_IsDirty = true;
        }

        private static void RemoveSubscription(object subscriber, Action<float> action,
            Dictionary<object, HashSet<Subscriber>> dictionary)
        {
            if (!dictionary.TryGetValue(subscriber, out var update)) return;

            foreach (var sub0 in update)
            {
                if (sub0.Action != action) continue;
                update.Remove(sub0);
                break;
            }
        }

        public void PauseUpdate(object subscriber) => SetActiveState(subscriber, false);
        public void ResumeUpdate(object subscriber) => SetActiveState(subscriber, true);

        public void PauseUpdate(object subscriber, GameLoopType type) => SetActiveState(subscriber, type, false);
        public void ResumeUpdate(object subscriber, GameLoopType type) => SetActiveState(subscriber, type, true);

        private void SetActiveState(object subscriber, bool isActive)
        {
            SetSubscriberActive(subscriber, isActive, _UpdateSubscribers);
            SetSubscriberActive(subscriber, isActive, _FixedUpdateSubscribers);
            SetSubscriberActive(subscriber, isActive, _LateUpdateSubscribers);
            _Update_IsDirty = _FixedUpdate_IsDirty = _LateUpdate_IsDirty = true;
        }

        private void SetActiveState(object subscriber, GameLoopType type, bool isActive)
        {
            switch (type)
            {
                case GameLoopType.UPDATE:
                    SetSubscriberActive(subscriber, isActive, _UpdateSubscribers);
                    _Update_IsDirty = true;
                    break;
                case GameLoopType.FIXED_UPDATE:
                    SetSubscriberActive(subscriber, isActive, _FixedUpdateSubscribers);
                    _FixedUpdate_IsDirty = true;
                    break;
                case GameLoopType.LATE_UPDATE:
                    SetSubscriberActive(subscriber, isActive, _LateUpdateSubscribers);
                    _LateUpdate_IsDirty = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void SetActiveState(object subscriber, Action<float> action, bool isActive)
        {
            SetSubscriberActionActive(subscriber, action, isActive, _UpdateSubscribers);
            SetSubscriberActionActive(subscriber, action, isActive, _FixedUpdateSubscribers);
            SetSubscriberActionActive(subscriber, action, isActive, _LateUpdateSubscribers);
            _Update_IsDirty = _FixedUpdate_IsDirty = _LateUpdate_IsDirty = true;
        }

        private void SetActiveState(object subscriber, Action<float> action, bool isActive, GameLoopType type)
        {
            switch (type)
            {
                case GameLoopType.UPDATE:
                    SetSubscriberActionActive(subscriber, action, isActive, _UpdateSubscribers);
                    _Update_IsDirty = true;
                    break;
                case GameLoopType.FIXED_UPDATE:
                    SetSubscriberActionActive(subscriber, action, isActive, _FixedUpdateSubscribers);
                    _FixedUpdate_IsDirty = true;
                    break;
                case GameLoopType.LATE_UPDATE:
                    SetSubscriberActionActive(subscriber, action, isActive, _LateUpdateSubscribers);
                    _LateUpdate_IsDirty = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static void SetSubscriberActive(object subscriber, bool isActive,
            Dictionary<object, HashSet<Subscriber>> dictionary)
        {
            if (!dictionary.TryGetValue(subscriber, out var update)) return;
            foreach (var sub in update)
            {
                sub.SetActive(isActive);
            }
        }

        private static void SetSubscriberActionActive(object subscriber, Action<float> action, bool isActive,
            Dictionary<object, HashSet<Subscriber>> dictionary)
        {
            if (!dictionary.TryGetValue(subscriber, out var update)) return;

            foreach (var sub in update)
            {
                if (sub.Action != action) continue;
                sub.SetActive(isActive);
                break;
            }
        }

        public void Update(object updater, float deltaTime)
        {
            if (updater is not T) return;

            foreach (var subscriber in UpdateSubscribers)
            {
                subscriber.Action?.Invoke(deltaTime);
            }
        }

        public void FixedUpdate(object updater, float deltaTime)
        {
            if (updater is not T) return;

            foreach (var subscriber in FixedUpdateSubscribers)
            {
                subscriber.Action?.Invoke(deltaTime);
            }
        }

        public void LateUpdate(object updater, float deltaTime)
        {
            if (updater is not T) return;

            foreach (var subscriber in LateUpdateSubscribers)
            {
                subscriber.Action?.Invoke(deltaTime);
            }
        }

        private static IEnumerable<Subscriber> LayOutActiveSubscribers(
            Dictionary<object, HashSet<Subscriber>> dictionary)
        {
            var output = new List<Subscriber>();

            foreach (var value in dictionary.Values)
            {
                output.AddRange(value.Where(subscriber => subscriber.IsActive));
            }

            return output;
        }
    }
}
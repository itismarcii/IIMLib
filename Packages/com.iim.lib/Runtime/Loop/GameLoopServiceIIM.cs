using System;
using System.Collections.Generic;

namespace IIMLib.Loop
{
    public sealed class GameLoopServiceIIM<T> : IGameLoopService<T>
    {
        private sealed class Subscriber : IEquatable<Subscriber>
        {
            public readonly Action<float> Action;
            public bool IsActive;

            public Subscriber(Action<float> action)
            {
                Action = action ?? throw new ArgumentNullException(nameof(action));
                IsActive = true;
            }

            public bool Equals(Subscriber other)
                => other != null && Equals(Action, other.Action);

            public override bool Equals(object obj)
                => obj is Subscriber other && Equals(other);

            public override int GetHashCode() => Action.GetHashCode();
        }

        public string IdentifierName => "GameManager";

        private readonly Dictionary<object, HashSet<Subscriber>> _updateSubscribers = new();
        private readonly Dictionary<object, HashSet<Subscriber>> _fixedUpdateSubscribers = new();
        private readonly Dictionary<object, HashSet<Subscriber>> _lateUpdateSubscribers = new();

        private Subscriber[] _updateCache = Array.Empty<Subscriber>();
        private Subscriber[] _fixedUpdateCache = Array.Empty<Subscriber>();
        private Subscriber[] _lateUpdateCache = Array.Empty<Subscriber>();

        private bool _updateDirty = true;
        private bool _fixedUpdateDirty = true;
        private bool _lateUpdateDirty = true;
        private string _ValidName;

        public void Initialize()
        {
        }

        public void Subscribe(object subscriber, Action<float> action, GameLoopType gameLoopType)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dictionary = GetDictionary(gameLoopType);

            if (!dictionary.TryGetValue(subscriber, out var subscriptions))
            {
                subscriptions = new HashSet<Subscriber>();
                dictionary.Add(subscriber, subscriptions);
            }

            if (subscriptions.Add(new Subscriber(action)))
                MarkDirty(gameLoopType);
        }

        public void UnSubscribe(object subscriber, GameLoopType gameLoopType)
        {
            if (subscriber == null)
                return;

            if (GetDictionary(gameLoopType).Remove(subscriber))
                MarkDirty(gameLoopType);
        }

        public void UnSubscribe(object subscriber)
        {
            if (subscriber == null)
                return;

            UnSubscribe(subscriber, GameLoopType.UPDATE);
            UnSubscribe(subscriber, GameLoopType.FIXED_UPDATE);
            UnSubscribe(subscriber, GameLoopType.LATE_UPDATE);
        }

        public void UnSubscribe(object subscriber, Action<float> action, GameLoopType gameLoopType)
        {
            if (subscriber == null || action == null)
                return;

            var dictionary = GetDictionary(gameLoopType);
            if (!dictionary.TryGetValue(subscriber, out var subscriptions))
                return;

            if (!subscriptions.Remove(new Subscriber(action)))
                return;

            if (subscriptions.Count == 0)
                dictionary.Remove(subscriber);

            MarkDirty(gameLoopType);
        }

        public void UnSubscribe(object subscriber, Action<float> action)
        {
            UnSubscribe(subscriber, action, GameLoopType.UPDATE);
            UnSubscribe(subscriber, action, GameLoopType.FIXED_UPDATE);
            UnSubscribe(subscriber, action, GameLoopType.LATE_UPDATE);
        }

        public void Pause(object subscriber)
            => SetActiveState(subscriber, false);

        public void Resume(object subscriber)
            => SetActiveState(subscriber, true);

        public void Update(T updater, float deltaTime)
        {
            if (_updateDirty)
            {
                _updateCache = BuildCache(_updateSubscribers);
                _updateDirty = false;
            }

            Invoke(_updateCache, deltaTime);
        }

        public void FixedUpdate(T updater, float fixedDeltaTime)
        {
            if (_fixedUpdateDirty)
            {
                _fixedUpdateCache = BuildCache(_fixedUpdateSubscribers);
                _fixedUpdateDirty = false;
            }

            Invoke(_fixedUpdateCache, fixedDeltaTime);
        }

        public void LateUpdate(T updater, float deltaTime)
        {
            if (_lateUpdateDirty)
            {
                _lateUpdateCache = BuildCache(_lateUpdateSubscribers);
                _lateUpdateDirty = false;
            }

            Invoke(_lateUpdateCache, deltaTime);
        }

        private void SetActiveState(object subscriber, bool active)
        {
            if (subscriber == null)
                return;

            SetSubscriberActive(_updateSubscribers, subscriber, active, GameLoopType.UPDATE);
            SetSubscriberActive(_fixedUpdateSubscribers, subscriber, active, GameLoopType.FIXED_UPDATE);
            SetSubscriberActive(_lateUpdateSubscribers, subscriber, active, GameLoopType.LATE_UPDATE);
        }

        private void SetSubscriberActive(
            Dictionary<object, HashSet<Subscriber>> dictionary,
            object subscriber,
            bool active,
            GameLoopType gameLoopType)
        {
            if (!dictionary.TryGetValue(subscriber, out var subscriptions))
                return;

            var changed = false;
            foreach (var subscription in subscriptions)
            {
                if (subscription.IsActive == active)
                    continue;

                subscription.IsActive = active;
                changed = true;
            }

            if (changed)
                MarkDirty(gameLoopType);
        }

        private Dictionary<object, HashSet<Subscriber>> GetDictionary(GameLoopType gameLoopType)
        {
            return gameLoopType switch
            {
                GameLoopType.UPDATE => _updateSubscribers,
                GameLoopType.FIXED_UPDATE => _fixedUpdateSubscribers,
                GameLoopType.LATE_UPDATE => _lateUpdateSubscribers,
                _ => throw new ArgumentOutOfRangeException(nameof(gameLoopType), gameLoopType, null)
            };
        }

        private void MarkDirty(GameLoopType gameLoopType)
        {
            switch (gameLoopType)
            {
                case GameLoopType.UPDATE:
                    _updateDirty = true;
                    break;
                case GameLoopType.FIXED_UPDATE:
                    _fixedUpdateDirty = true;
                    break;
                case GameLoopType.LATE_UPDATE:
                    _lateUpdateDirty = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameLoopType), gameLoopType, null);
            }
        }

        private static Subscriber[] BuildCache(Dictionary<object, HashSet<Subscriber>> dictionary)
        {
            var count = 0;
            foreach (var pair in dictionary)
            {
                foreach (var subscriber in pair.Value)
                {
                    if (subscriber.IsActive)
                        count++;
                }
            }

            if (count == 0)
                return Array.Empty<Subscriber>();

            var cache = new Subscriber[count];
            var index = 0;

            foreach (var pair in dictionary)
            {
                foreach (var subscriber in pair.Value)
                {
                    if (subscriber.IsActive)
                        cache[index++] = subscriber;
                }
            }

            return cache;
        }

        private static void Invoke(Subscriber[] subscribers, float deltaTime)
        {
            for (var i = 0; i < subscribers.Length; i++)
                subscribers[i].Action.Invoke(deltaTime);
        }
    }
}

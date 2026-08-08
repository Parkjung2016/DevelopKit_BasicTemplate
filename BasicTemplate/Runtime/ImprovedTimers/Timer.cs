using System;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>PlayerLoop에서 갱신되는 타이머의 공통 수명주기를 제공합니다.</summary>
    public abstract class Timer : IDisposable
    {
        private bool isRegistered;
        private bool isDisposed;

        protected Timer(float duration)
        {
            SetDuration(duration);
            Reset();
        }

        public float CurrentTime { get; protected set; }
        public float Duration { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public float Progress => Duration > 0f ? Mathf.Clamp01(CurrentTime / Duration) : 0f;

        public event Action Started;
        public event Action Stopped;

        public void Start()
        {
            ThrowIfDisposed();
            Reset();

            if (!isRegistered)
            {
                TimerManager.Instance.Register(this);
                isRegistered = true;
            }

            IsRunning = true;
            IsPaused = false;
            Started?.Invoke();
        }

        public void Stop()
        {
            if (!isRegistered)
                return;

            TimerManager.Instance.Unregister(this);
            isRegistered = false;
            IsRunning = false;
            IsPaused = false;
            Stopped?.Invoke();
        }

        public void Pause()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            IsPaused = true;
        }

        public void Resume()
        {
            ThrowIfDisposed();
            if (!IsPaused)
                return;

            IsRunning = true;
            IsPaused = false;
        }

        public virtual void Reset()
        {
            CurrentTime = Duration;
        }

        public virtual void Reset(float duration)
        {
            SetDuration(duration);
            Reset();
        }

        internal void Update(float deltaTime)
        {
            if (IsRunning)
                OnTick(Mathf.Max(0f, deltaTime));
        }

        protected abstract void OnTick(float deltaTime);

        protected void Complete()
        {
            Stop();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            if (isRegistered)
                TimerManager.Instance.Unregister(this);

            isRegistered = false;
            IsRunning = false;
            IsPaused = false;
            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        private void SetDuration(float duration)
        {
            if (duration < 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration cannot be negative.");

            Duration = duration;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
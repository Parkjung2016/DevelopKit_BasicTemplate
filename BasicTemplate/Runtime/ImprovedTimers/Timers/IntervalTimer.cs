using System;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>지정한 시간 동안 일정 간격으로 이벤트를 실행합니다.</summary>
    public sealed class IntervalTimer : Timer
    {
        private readonly float interval;
        private float nextInterval;

        public IntervalTimer(float duration, float interval) : base(duration)
        {
            if (interval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");

            this.interval = interval;
            Reset();
        }

        public bool IsFinished => CurrentTime <= 0f;
        public event Action Elapsed;

        protected override void OnTick(float deltaTime)
        {
            CurrentTime -= deltaTime;

            while (CurrentTime <= nextInterval && nextInterval >= 0f)
            {
                Elapsed?.Invoke();
                nextInterval -= interval;
            }

            if (CurrentTime > 0f)
                return;

            CurrentTime = 0f;
            Complete();
        }

        public override void Reset()
        {
            base.Reset();
            nextInterval = Duration - interval;
        }
    }
}
using System;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>초당 지정한 횟수만큼 이벤트를 실행합니다.</summary>
    public sealed class FrequencyTimer : Timer
    {
        private float interval;

        public FrequencyTimer(int ticksPerSecond) : base(0f)
        {
            SetFrequency(ticksPerSecond);
        }

        public int TicksPerSecond { get; private set; }
        public bool IsFinished => !IsRunning && !IsPaused;
        public event Action Ticked;

        protected override void OnTick(float deltaTime)
        {
            CurrentTime += deltaTime;
            while (CurrentTime >= interval)
            {
                CurrentTime -= interval;
                Ticked?.Invoke();
            }
        }

        public override void Reset()
        {
            CurrentTime = 0f;
        }

        public void SetFrequency(int ticksPerSecond)
        {
            if (ticksPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticksPerSecond), "Ticks per second must be greater than zero.");

            TicksPerSecond = ticksPerSecond;
            interval = 1f / ticksPerSecond;
            Reset();
        }
    }
}
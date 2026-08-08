namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>경과 시간을 누적하는 타이머입니다.</summary>
    public sealed class StopwatchTimer : Timer
    {
        public StopwatchTimer() : base(0f)
        {
        }

        public bool IsFinished => false;

        protected override void OnTick(float deltaTime)
        {
            CurrentTime += deltaTime;
        }

        public override void Reset()
        {
            CurrentTime = 0f;
        }
    }
}
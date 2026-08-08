namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>지정한 시간부터 0까지 감소하는 타이머입니다.</summary>
    public sealed class CountdownTimer : Timer
    {
        public CountdownTimer(float duration) : base(duration)
        {
        }

        public bool IsFinished => CurrentTime <= 0f;

        protected override void OnTick(float deltaTime)
        {
            CurrentTime -= deltaTime;
            if (CurrentTime > 0f)
                return;

            CurrentTime = 0f;
            Complete();
        }
    }
}
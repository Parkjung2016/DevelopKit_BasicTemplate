using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime {
    /// <summary>
    /// 특정 값에서 0까지 카운트다운하는 타이머.
    /// </summary>
    public class CountdownTimer : Timer {
        public CountdownTimer(float value) : base(value) { }

        public override void Tick() {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= Time.deltaTime;
            }

            if (IsRunning && CurrentTime <= 0) {
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;
    }
}
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime {
    /// <summary>
    /// 경과 시간 측정을 위한 증가형 타이머.
    /// </summary>
    public class StopwatchTimer : Timer {
        public StopwatchTimer() : base(0) { }

        public override void Tick() {
            if (IsRunning) {
                CurrentTime += Time.deltaTime;
            }
        }

        public override bool IsFinished => false;
    }
}
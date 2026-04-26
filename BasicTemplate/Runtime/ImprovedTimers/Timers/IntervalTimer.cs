using System;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime {
    /// <summary>
    /// 완료될 때까지 일정 간격마다 이벤트를 발생시키는 카운트다운 타이머.
    /// </summary>
    public class IntervalTimer : Timer {
        readonly float interval;
        float nextInterval;

        public Action OnInterval = delegate { };

        public IntervalTimer(float totalTime, float intervalSeconds) : base(totalTime) {
            interval = intervalSeconds;
            nextInterval = totalTime - interval;
        }

        public override void Tick() {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= Time.deltaTime;

                // Fire interval events as long as thresholds are crossed
                while (CurrentTime <= nextInterval && nextInterval >= 0) {
                    OnInterval.Invoke();
                    nextInterval -= interval;
                }
            }

            if (IsRunning && CurrentTime <= 0) {
                CurrentTime = 0;
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;

        public override void Reset() {
            base.Reset();
            nextInterval = initialTime - interval;
        }

        public override void Reset(float newTime) {
            base.Reset(newTime);
            nextInterval = initialTime - interval;
        }
    }
}
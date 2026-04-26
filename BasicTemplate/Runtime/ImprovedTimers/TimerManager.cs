using System.Collections.Generic;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public class TimerManager : Singleton<TimerManager>
    {
        private readonly List<Timer> timers = new();
        private readonly List<Timer> sweep = new();

        public void RegisterTimer(Timer timer) => timers.Add(timer);
        public void DeregisterTimer(Timer timer) => timers.Remove(timer);

        public void UpdateTimers()
        {
            if (timers.Count == 0) return;

            sweep.RefreshWith(timers);
            foreach (var timer in sweep)
            {
                timer.Tick();
            }
        }

        public void Clear()
        {
            sweep.RefreshWith(timers);
            foreach (var timer in sweep)
            {
                timer.Dispose();
            }

            timers.Clear();
            sweep.Clear();
        }
    }
}
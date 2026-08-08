using System.Collections.Generic;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    internal sealed class TimerManager : Singleton<TimerManager>
    {
        private readonly List<Timer> timers = new();
        private readonly List<Timer> updateBuffer = new();

        public TimerManager()
        {
        }

        internal void Register(Timer timer)
        {
            if (timer != null && !timers.Contains(timer))
                timers.Add(timer);
        }

        internal void Unregister(Timer timer)
        {
            if (timer != null)
                timers.Remove(timer);
        }

        internal void UpdateTimers()
        {
            if (timers.Count == 0)
                return;

            updateBuffer.Clear();
            updateBuffer.AddRange(timers);

            float deltaTime = Time.deltaTime;
            for (int i = 0; i < updateBuffer.Count; i++)
                updateBuffer[i].Update(deltaTime);
        }

        internal void Clear()
        {
            updateBuffer.Clear();
            updateBuffer.AddRange(timers);

            for (int i = 0; i < updateBuffer.Count; i++)
                updateBuffer[i].Dispose();

            timers.Clear();
            updateBuffer.Clear();
        }
    }
}
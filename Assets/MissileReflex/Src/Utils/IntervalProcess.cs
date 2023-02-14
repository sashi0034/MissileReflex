using System;

namespace MissileReflex.Src.Utils
{
    public class IntervalProcess
    {
        private readonly float interval = 0;
        private readonly Action process;

        private float _timeCount = 0;

        public IntervalProcess()
        {
            process = () => { };
        }

        public IntervalProcess(Action process, float interval)
        {
            this.interval = interval;
            this.process = process;
        }

        public void Clear()
        {
            _timeCount = 0;
        }

        public void Update(float deltaTime)
        {
            _timeCount += deltaTime;
            while (_timeCount > interval)
            {
                _timeCount -= interval;
                process.Invoke();
            }
        }
    }
}
using System;
using System.Threading;

namespace UptimeWarner
{
    public class Timer : IDisposable
    {
        private System.Threading.Timer _timer;
        private bool _isRunning;
        private TimeSpan _deltaTime;
        private Action<Timer> _callback;

        public TimeSpan DeltaTime
        {
            private get => _deltaTime;
            set
            {
                if (_isRunning) throw new TimerException();
                _deltaTime = value;
            }
        }

        public Action<Timer> Callback
        {
            private get => _callback;
            set
            {
                if (_isRunning) throw new TimerException();
                _callback = value;
            }
        }

        public bool IsRunning => _isRunning;

        public Timer() { }
        public Timer(Action<Timer> callback, TimeSpan deltaTime, bool shouldRunInstantly)
        {
            DeltaTime = deltaTime;
            Callback = callback;
            if(shouldRunInstantly) Start();
        }
        public void Start()
        {
            _timer = new System.Threading.Timer((_) => Callback(this), new object(), 0, (int)DeltaTime.TotalMilliseconds);
            _isRunning = true;
        }

        public void Pause()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _isRunning = false;
        }

        public void Resume()
        {
            _timer.Change(0, (int)DeltaTime.TotalMilliseconds);
            _isRunning = true;
        }

        public void Destroy()
        {
            _timer.Dispose();
            _timer = null;
            _isRunning = false;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

    public class TimerException : Exception
    {
        public override string Message { get; }
        public TimerException()
        {
            Message = "Cannot change delta time or callback for running task!";
        }
    }
}
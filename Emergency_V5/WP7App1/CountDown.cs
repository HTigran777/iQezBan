///Countdown timer class created by Tigran Hakobyan
///EmmanuelPro @2012
///http://www.EmmanuelPro.netne.net

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WP7App1
{
    public class CountDown
    {
        private double _reserveTimer;
        private double _timer;
        private DispatcherTimer _localTimer = new DispatcherTimer();
        private bool _isActive;
        private bool _alarmed;

        /// <summary>
        /// Alarms when timer ends
        /// </summary>
        public event System.EventHandler Alarm;

        /// <summary>
        /// Initializes empty Timer
        /// </summary>
        public CountDown()
        {
            _alarmed = false;
            _localTimer.Interval = TimeSpan.FromMilliseconds(0.1);
            _localTimer.Start();
            _localTimer.Tick += new EventHandler(_localTimer_Tick);
        }

        /// <summary>
        /// Initializes a timer and sets its count in milliseconds
        /// </summary>
        /// <param name="milliseconds">Countdown Timer</param>
        public CountDown(double milliseconds)
        {
            CountDownTimer = milliseconds;
            _alarmed = false;
            _localTimer.Interval = TimeSpan.FromMilliseconds(0.1);
            _localTimer.Start();
            _localTimer.Tick += new EventHandler(_localTimer_Tick);
        }

        /// <summary>
        /// Initializes a timer and sets its count in milliseconds
        /// </summary>
        /// <param name="timer">Countdown Timer</param>
        public CountDown(TimeSpan timer)
        {
            CountDownTimer = timer.TotalMilliseconds;
            _alarmed = false;
            _localTimer.Interval = TimeSpan.FromMilliseconds(0.1);
            _localTimer.Start();
            _localTimer.Tick += new EventHandler(_localTimer_Tick);
        }

        /// <summary>
        /// Initializes countdown timer valuse Hours, Minutes, Seconds, Milliseconds
        /// </summary>
        /// <param name="timeSpan"></param>
          public void setCountDown(TimeSpan timeSpan)
        {
            CountDownTimer = timeSpan.TotalMilliseconds;
            _alarmed = false;
        }

        void _localTimer_Tick(object sender, EventArgs e)
        {
            if (_isActive && !_alarmed)
            {
                if (_timer > 0)
                    _timer--;
                else
                {
                    Stop();
                    _alarmed = true;
                    Alarm(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Set countdown timer by Milliseconds
        /// </summary>
        public double CountDownTimer
        {
            get { return _timer; }
            set 
            { 
                _timer = value;
                _reserveTimer = value;
                _alarmed = false;
            }
        }

        /// <summary>
        /// Shows if timer active or not
        /// </summary>
        public bool IsActive { get { return _isActive; } }

        /// <summary>
        /// Starts to countdown
        /// </summary>
        public void Start()
        {
            if (!_isActive && !_alarmed)
                _isActive = true;
        }

        /// <summary>
        /// Stops timer and resets to start position
        /// </summary>
        public void Stop()
        {
            if (_isActive)
            {
                _isActive = false;
                _timer = _reserveTimer;
            }
        }

        /// <summary>
        /// Pauses Timer
        /// </summary>
        public void Pause()
        {
            if(_isActive) _isActive = false;
        }

        /// <summary>
        /// Reset timer to start position without stoping it
        /// </summary>
        public void ResetTimer()
        {
            _timer = _reserveTimer;
            _alarmed = false;
        }

        /// <summary>
        /// Returns the estimated time to the end of timer
        /// </summary>
        public double EstimatedTime
        {
            get { return _timer; }
        }

        /// <summary>
        /// Indicates if the last timer alarmed
        /// </summary>
        public bool Alarmed
        {
            get { return _alarmed; }
        }
    }
}

using System;

namespace GraphingMeter
{
    /// <summary>
    /// Sample counter. Tracks number of packets received and packet rate.
    /// </summary>
    internal class SampleCounter
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly System.Windows.Forms.Timer _timer;
        private int _prevSamplesReceived;
        private int _samplesReceived;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SampleCounter()
        {
            // Initialise variables
            _prevSamplesReceived = 0;
            _samplesReceived = 0;

            // Setup timer
            _timer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
        
        /// <summary>
        /// Sample receive rate as packets per second.
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Increments packet counter.
        /// </summary>
        public void Increment()
        {
            _samplesReceived++;
        }

        /// <summary>
        /// Reset packet rate counter
        /// </summary>
        public void Reset()
        {
            _prevSamplesReceived = 0;
            _samplesReceived = 0;
            SampleRate = 0;
        }

        /// <summary>
        /// timer Tick event to calculate packet rate.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            SampleRate = _samplesReceived - _prevSamplesReceived;
            _prevSamplesReceived = _samplesReceived;
        }
    }
}
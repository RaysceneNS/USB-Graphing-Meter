using System;

namespace Foresight.GraphingMeter
{
    /// <summary>
    /// Sample counter. Tracks number of packets received and packet rate.
    /// </summary>
    internal class SampleCounter
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly System.Windows.Forms.Timer _timer;
        private int _prevSamplesReceived;
        private int _sampleRate;
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
        /// Number of packets received.
        /// </summary>
        public int SamplesReceived
        {
            get
            {
                return _samplesReceived;
            }
        }

        /// <summary>
        /// Sample receive rate as packets per second.
        /// </summary>
        public int SampleRate
        {
            get
            {
                return _sampleRate;
            }
        }

        /// <summary>
        /// Increments packet counter.
        /// </summary>
        public void Increment()
        {
            _samplesReceived++;
        }

        // Zeros packet counter.
        public void Reset()
        {
            _prevSamplesReceived = 0;
            _samplesReceived = 0;
            _sampleRate = 0;
        }

        /// <summary>
        /// timer Tick event to calculate packet rate.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            _sampleRate = SamplesReceived - _prevSamplesReceived;
            _prevSamplesReceived = SamplesReceived;
        }
    }
}
using System;
using System.Threading;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Timer = System.Windows.Forms.Timer;

namespace Foresight.GraphingMeter
{
    public partial class FormMain : Form
    {
        private const int Vid = 0x16c0;
        private const int Pid = 0x05df;

        private readonly Timer _formUpdateTimer = new Timer();
        private Thread _backThread;
        private static UsbDevice _myUsbDevice;
        private readonly SampleCounter _sampleCounter = new SampleCounter();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
        }

        #region Form 

        /// <summary>
        /// From load event.
        /// </summary>
        private void FormTerminal_Load(object sender, EventArgs e)
        {
            // Attempt to find the usb device
            FindUsbDevice();
            
            // Setup form update timer
            _formUpdateTimer.Interval = 40;
            _formUpdateTimer.Tick += FormUpdateTimer_Tick;
            _formUpdateTimer.Start();

            //fill in the x axis
            for (int i = 0; i < 1000; i++)
            {
                chart1.Series[0].Points.Add(0);
            }
        }

        /// <summary>
        /// Form close event.
        /// </summary>
        private void FormTerminal_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseUsbDevice();
        }
        
        /// <summary>
        /// formUpdateTimer Tick event to update terminal textbox.
        /// </summary>
        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Update sample counter values
            toolStripStatusLabelSampleRate.Text = "Sample Rate: " + _sampleCounter.SampleRate;
        }

        #endregion

        #region Usb Device Acccess

        /// <summary>
        /// Locates the usb device 
        /// </summary>
        private void FindUsbDevice()
        {
            // Find and open the usb device.
            UsbDeviceFinder usbFinder = new UsbDeviceFinder(Vid, Pid);
            _myUsbDevice = UsbDevice.OpenUsbDevice(usbFinder);
            if (_myUsbDevice == null)
            {
                return;
            }

            if (_myUsbDevice is IUsbDevice wholeUsbDevice)
            {
                // This is a "whole" USB device. Before it can be used, 
                // the desired configuration and interface must be selected.
                // Select config #1
                wholeUsbDevice.SetConfiguration(1);

                // Claim interface #0.
                wholeUsbDevice.ClaimInterface(0);
            }

            try
            {
                _backThread = new Thread(Start)
                {
                    IsBackground = true
                };
                _backThread.Start();

                _sampleCounter.Reset();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void Start()
        {
            byte requestType = (0x01 << 5) | 0x80;
            const byte request = 0x01;
            short val = 0x03 << 8;
            byte[] readBuffer = new byte[512];
            var packet = new UsbSetupPacket(requestType, request, val, 0, 1);

            while (true)
            {
                if (_myUsbDevice == null)
                {
                    return;
                }

                _myUsbDevice.ControlTransfer(ref packet, readBuffer, 512, out var transferred);

                if (transferred != 0)
                {
                    this.Invoke((MethodInvoker)delegate 
                    {
                        OnReceiveEndPointData(readBuffer, transferred);
                    });
                }
            }
        }
        
        /// <summary>
        /// Event is fired each time samples are received from USB device
        /// </summary>
        private void OnReceiveEndPointData(byte[] buff, int count)
        {
            var series = chart1.Series[0];
            // Process each byte
            for (var index = 0; index < count; index++)
            {
                byte b = buff[index];
                float volts = b / 51.0f;
                series.Points.AddY(volts);
                series.Points.RemoveAt(0);

                _sampleCounter.Increment();
            }
        }

        /// <summary>
        /// Closes serial port.
        /// </summary>
        private void CloseUsbDevice()
        {
            _backThread.Abort();
            try
            {
                if (_myUsbDevice != null)
                {
                    if (_myUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        var wholeUsbDevice = _myUsbDevice as IUsbDevice;
                        // Release interface #0.
                        wholeUsbDevice?.ReleaseInterface(0);
                        _myUsbDevice.Close();
                    }
                }

                _myUsbDevice = null;
            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }
}

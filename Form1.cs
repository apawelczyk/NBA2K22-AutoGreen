using System;
using System.Linq;
using System.Windows.Forms;
using AutoGreen.Hooks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutoGreen
{
    public partial class Form1 : Form
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        delegate void SetTextCallback(string text, Label label);

        public bool IsRunning { get; set; } = false;
        public Timer Clock { get; set; }
        public MicroTimer MicroTimer { get; set; }
        public long Ticks { get; set; }
        public KeyHelper KeyHelper { get; set; }
        public double MicroShotTimeRelease { get; set; } = 446;
        public double ShotTimeRelease { get; set; } = 28;
        public long TicksAfter { get; set; }
        public bool WasShotTaken { get; set; } = false;
        public Process GameProcess { get; set; }
        public string Exception { get; set; }
        public Input[] Inputs = new Input[]
        {
            new Input
            {
                type = 1, // Keyboard
                io = new InputOffset
                {
                    ki = new KeyboardInput
                    {
                        wVk = 0,
                        wScan = (ushort) 0x4C, // NUMPAD 5
                        dwFlags = (uint)(0x0000 | 0x0008), // KEYDOWN | SCANCODE
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            }
        };

        public Form1()
        {
            try
            {
                InitializeComponent();

                this.BackgroundImage = Properties.Resources.Bulls;
                this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
                this.BackgroundImageLayout = ImageLayout.Stretch;

                //Clock = new Timer();
                //Clock.Tick += TimerTick;
                //Clock.Interval = 1;
                //Clock.Start();

                MicroTimer = new MicroTimer();
                MicroTimer.Interval = 1;//10000;
                MicroTimer.Enabled = true;
                MicroTimer.MicroTimerElapsed += new MicroTimer.MicroTimerElapsedEventHandler(MicroTimerTick);

                KeyHelper = new KeyHelper();

                KeyHelper.KeyDown += KeyDown;
                GameProcess = Process.GetProcessesByName("NBA2K22").FirstOrDefault();
                if (GameProcess != null)
                {
                    IntPtr h = GameProcess.MainWindowHandle;
                    SetForegroundWindow(h);
                    label8.Text = $"Game Process {GameProcess.Id.ToString()} attached!";
                }
                else
                {
                    label8.Text = "No Game Process attached!";
                }
            }
            catch (Exception e)
            {
                Exception = e.Message;
                label7.Text = Exception;
            }
        }

        private void MicroTimerTick(object sender, MicroTimerEventArgs timerEventArgs)
        {
            try
            {
                if (IsRunning)
                {
                    SetText(MicroTimer.ElapsedTime.ToString(), label5);
                    SetText(MicroShotTimeRelease.ToString(), label3);
                    
                    if (WasShotTaken)
                    {
                        SendInput((uint)Inputs.Length, Inputs, Marshal.SizeOf(typeof(Input)));
                        if (MicroTimer.ElapsedTime >= MicroShotTimeRelease)
                        {
                            SendInput((uint)Inputs.Length, Inputs, Marshal.SizeOf(typeof(Input)));
                            WasShotTaken = false;
                            return;
                        }
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Exception = ex.Message;
                SetText(Exception, label7);
            }
        }

        public void KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (IsRunning)
            {
                if (e.KeyCode == Keys.E)
                {
                    WasShotTaken = true;
                    MicroTimer.Stop();
                    MicroTimer.Start();
                    return;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IsRunning = !IsRunning;
            button1.Text = IsRunning ? "Stop" : "Start";
        }

        private void SetText(string text, Label label)
        {
            if (label.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text, label });
            }
            else
            {
                label.Text = text;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MicroShotTimeRelease = Convert.ToDouble(textBox1.Text);
                }
            }
            catch (Exception exception)
            {

            }
        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }

    public struct Input
    {
        public int type;
        public InputOffset io;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputOffset
    {
        [FieldOffset(0)] 
        public KeyboardInput ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}

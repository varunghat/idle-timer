using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IdleTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String filename = "alert";
        String resume_filename = "resume";
        int h, m, s;
        int H = 0, M = 0, S = 0;
        private static System.Timers.Timer aTimer;
        bool initset = false;
        bool inBreak = false;

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }

        [DllImport("User32.dll")]
		public static extern bool LockWorkStation();

		[DllImport("User32.dll")]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);		

		[DllImport("Kernel32.dll")]
		private static extern uint GetLastError();
	
		public static uint GetIdleTime()
		{
			LASTINPUTINFO lastInPut = new LASTINPUTINFO();
			lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
			GetLastInputInfo(ref lastInPut);

			return ( (uint)Environment.TickCount - lastInPut.dwTime);
		}

		public static long GetTickCount()
		{
			return Environment.TickCount;
		}

		public static long GetLastInputTime()
		{
			LASTINPUTINFO lastInPut = new LASTINPUTINFO();
			lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
			if (!GetLastInputInfo(ref lastInPut))
			{
				throw new Exception(GetLastError().ToString());
			}
							
			return lastInPut.dwTime;
		}

        public MainWindow()
        {
            InitializeComponent();
        }

       

        private void timerText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(hText.Visibility==Visibility.Hidden)
            {
                Dispatcher.Invoke(() => {
                    hText.Text = mText.Text = sText.Text = "";
                    hText.Visibility = mText.Visibility = sText.Visibility = Visibility.Visible;
                });
                if (initset == true)
                {
                    aTimer.Stop();
                    aTimer.Dispose();
                }
            }
        }

        private void setTimerText(int hour,int min,int sec)
        {
            Dispatcher.Invoke(() => { timerText.Content = hour.ToString("D2") + ":" + min.ToString("D2") + ":" + sec.ToString("D2"); });
            h = H;
            m = M;
            s = S;

        }

        private void startTimer()
        {
            initset = true;
            inBreak = false;
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent; 
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void breakButton_Click(object sender, RoutedEventArgs e)
        {
            if (hText.Visibility == Visibility.Visible)
                return;
            if (inBreak==false)
                timerExpired();
        }

        async private void timerExpired()
        {
            inBreak = true;
            if(aTimer!=null)
            {
                aTimer.Stop();
                aTimer.Dispose();
            }
            
            Dispatcher.Invoke(() => { messageLabel.Content = "Get up"; });

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + filename + ".wav");
            player.Play();

            await Task.Delay(60000);
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += IdleTimer;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (s == 0)
            {
                if (m == 0)
                {
                    if (h == 0)
                    {
                        timerExpired();
                    }
                    else
                    {
                        h--;
                        m = 59;
                        s = 59;
                    }
                }
                else
                {
                    m--;
                    s = 59;
                }
            }
            else
            {
                s--;
            }
            Dispatcher.Invoke(() => { timerText.Content = h.ToString("D2") + ":" + m.ToString("D2") + ":" + s.ToString("D2"); });
            
            

        }

        private void IdleTimer(Object source, ElapsedEventArgs e)
        {
            if (GetIdleTime() < 900)
            {
                Dispatcher.Invoke(() => { messageLabel.Content = "Work"; });
                aTimer.Stop();
                aTimer.Dispose();
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + resume_filename + ".wav");
                player.Play();
                setTimerText(H, M, S);
                startTimer();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(hText.Visibility == Visibility.Visible)
            {                
                Int32.TryParse(hText.Text, out H);
                Int32.TryParse(mText.Text, out M);
                Int32.TryParse(sText.Text, out S);

                h = H;
                m = M;
                s = S;
                setTimerText(H,M,S);
                hText.Visibility = mText.Visibility = sText.Visibility = Visibility.Hidden;
                startTimer();

            }
        }
    }
}

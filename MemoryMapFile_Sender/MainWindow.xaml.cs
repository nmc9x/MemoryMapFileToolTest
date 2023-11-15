using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MemoryMapFile_Sender
{
    public partial class MainWindow : Window
    {
        private MemoryMapHelper _mmf_1;
        private MemoryMapHelper _mmf_2;
        private bool mmf1Alive;
        private bool mmf2Alive;
        public MainWindow()
        {
            RunReceiver();
            InitializeComponent();
            InitControlValue();
            Button_Send.Click += Button_Send_ClickAsync;
            _ = CheckStatusMmf(1000);
        }
        private void RunReceiver()
        {
            Process process = new Process();
            process.StartInfo.FileName = "MemoryMapFile_Receiver.exe";
            try
            {
                process.Start();
            }
            catch
            {
                return;
            }
            
        }
        private async void Button_Send_ClickAsync(object sender, RoutedEventArgs e)
        {
            await SendAsync();
        }

        async Task CheckStatusMmf(int interval)
        {
            while (true)
            {
                CheckMmf();
                await Task.Delay(interval);
            }
        }

        private void InitControlValue()
        {
            TextBoxData.Text = "abcd";
            TextBoxData1.Text = "1234";
            TextBoxLength.Text = "4";
            TextBoxLength1.Text = "4";
        }
        public void CheckMmf()
        {
            if (_mmf_1 != null && !_mmf_1._isDisposed)
            {
                mmf1Alive = true;
            }
            else
            {
                mmf1Alive = false;
            }
            if (_mmf_2 != null && !_mmf_2._isDisposed)
            {
                mmf2Alive = true;
            }
            else
            {
                mmf2Alive = false;
            }
            TextBoxStatusMmf1.Text = mmf1Alive.ToString();
            TextBoxStatusMmf2.Text = mmf2Alive.ToString();
        }

        private async Task SendAsync()
        {
            int cap1 = int.Parse(TextBoxLength.Text);
            int cap2 = int.Parse(TextBoxLength1.Text);
            
            int length1 = TextBoxData.Text.Length;
            int length2 = TextBoxData1.Text.Length;
            bool isValid1 = int.TryParse(TextBoxLength.Text, out _);
            bool isValid2 = int.TryParse(TextBoxLength1.Text, out _);

            try
            {
                if (length1 > 0 && isValid1 && length2 > 0 && isValid2 && length1 <= cap1 && length2 <= cap2)
                {
                    using (_mmf_1 = new MemoryMapHelper("mmf_name1", cap1))
                    {
                        await _mmf_1.WriteDataAsync(Encoding.ASCII.GetBytes(TextBoxData.Text), 0, cap1);
                    }
                    using (_mmf_2 = new MemoryMapHelper("mmf_name2", cap2))
                    {
                        await _mmf_2.WriteDataAsync(Encoding.ASCII.GetBytes(TextBoxData1.Text), 0, cap2);
                    }
                }
                else
                {
                    MessageBox.Show("Fail value, please re-enter value !", "", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}

using MemoryMapFile_Sender;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MemoryMapFile_Receiver
{
    public partial class MainWindow : Window
    {
        private MemoryMapHelper _mmf_1;
        private MemoryMapHelper _mmf_2;
        private bool mmf1Alive;
        private bool mmf2Alive;

        public MainWindow()
        {
            InitializeComponent();
            TextBoxLength.Text = "4";
            TextBoxLength1.Text = "4";
            _ = CheckStatusMmf(1000);
        }
        async Task CheckStatusMmf(int interval)
        {
            while (true)
            {
                using (_mmf_1 = new MemoryMapHelper("mmf_name1", int.Parse(TextBoxLength.Text)))
                {
                    var res = await _mmf_1.ReadDataAsync(0, int.Parse(TextBoxLength.Text));
                    TextBoxData.Text = Encoding.ASCII.GetString(res);

                }
                using (_mmf_2 = new MemoryMapHelper("mmf_name2", int.Parse(TextBoxLength1.Text)))
                {
                    var res1 = await _mmf_2.ReadDataAsync(0, int.Parse(TextBoxLength1.Text));
                    TextBoxData1.Text = Encoding.ASCII.GetString(res1);
                }
                CheckMmf();
                await Task.Delay(interval);
            }
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
    }
}

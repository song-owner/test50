using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace problemSolver
{
    public interface IDeviceService
    {
        void Exit();
        string GetUuid();
        bool IsUpperVersion(int major, int minor);
        string GetDeviceVersion();
        string GetManufacturerName();
        string GetModelName();
        string GetCpuType();
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            IsEnable = false;
            IsBusy = true;
            //IsEnable = true;
            //IsBusy = false;

        }

        private void startSearch(object sender, EventArgs e)
        {
            var abc = new ResultPage(seartchWord.Text);
            //Navigation.PushAsync(abc,true);
            Application.Current.MainPage = abc;
            //Navigation.PushAsync(new ResultPage(seartchWord.Text));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public Command OverlayClicked { get; }
        public Command TBCommand { get; set; }

        public bool IsEnable { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
                }
            }
        }
    }
}
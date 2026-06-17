using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class SplashView : UserControl
    {
        private bool _hasFinished;

        public event Action? Finished;

        public SplashView()
        {
            InitializeComponent();
            Loaded += SplashView_Loaded;
        }

        private async void SplashView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_hasFinished)
            {
                return;
            }

            _hasFinished = true;
            await Task.Delay(1450);
            Finished?.Invoke();
        }
    }
}

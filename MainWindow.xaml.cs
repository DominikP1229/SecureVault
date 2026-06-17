using SecureVault.Views;
using System.Windows;

namespace SecureVault
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowSplash();
        }

        public void SwitchView(UIElement newView)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(newView);
        }

        private void ShowSplash()
        {
            var splashView = new SplashView();
            splashView.Finished += HandleSplashFinished;
            MainGrid.Children.Add(splashView);
        }

        private void HandleSplashFinished()
        {
            if (MainGrid.Children.Count > 0 && MainGrid.Children[0] is SplashView splashView)
            {
                splashView.Finished -= HandleSplashFinished;
            }

            SwitchView(new LoginView());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SecureVault.Model.Services;

namespace SecureVault.Views
{
    /// <summary>
    /// Interaction logic for RegisterView.xaml
    /// </summary>
    public partial class RegisterView : UserControl
    {

        public RegisterView()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Login jest wymagany.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Hasło jest wymagane.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AccountStore.Exists(login))
            {
                MessageBox.Show("Konto o takim loginie już istnieje.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AccountStore.Add(login, password);

            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new LoginView());
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new LoginView());
            }
        }
    }
}

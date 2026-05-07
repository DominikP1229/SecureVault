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
using SecureVault.Model;
using SecureVault.Model.Services;

namespace SecureVault.Views
{
    /// <summary>
    /// Logika interakcji dla klasy LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {

        public LoginView()
        {
            InitializeComponent();
            LoginBox.ItemsSource = AccountStore.Accounts;
            LoginBox.DisplayMemberPath = nameof(Account.Name);

            if (AccountStore.Accounts.Count > 0)
            {
                LoginBox.SelectedIndex = 0;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (LoginBox.SelectedItem is not Account selectedAccount)
            {
                MessageBox.Show("Wybierz konto.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password != selectedAccount.Password)
            {
                MessageBox.Show("Niepoprawne hasło.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new MainView());
            }
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new RegisterView());
            }
        }
    }
}

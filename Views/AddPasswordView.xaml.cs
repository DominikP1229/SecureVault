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
using SecureVault.ViewModel;

namespace SecureVault.Views
{
    /// <summary>
    /// Interaction logic for PasswordMenuView.xaml
    /// </summary>
    public partial class AddPasswordView: UserControl
    {
        private readonly MainViewModel _viewModel;
        private readonly bool _isEditMode;

        public AddPasswordView()
            : this(new MainViewModel())
        {
        }

        public AddPasswordView(MainViewModel viewModel)
            : this(viewModel, false)
        {
        }

        public AddPasswordView(MainViewModel viewModel, bool isEditMode)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _isEditMode = isEditMode;
            DataContext = _viewModel;
        }

        private void OpenCategories(object sender, RoutedEventArgs e)
        {
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                if (_viewModel.EditCommand.CanExecute(null))
                {
                    _viewModel.EditCommand.Execute(null);

                    if (string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
                    {
                        ReturnToMainView();
                    }
                }

                return;
            }

            var countBeforeSave = _viewModel.Credentials.Count;

            if (_viewModel.AddCommand.CanExecute(null))
            {
                _viewModel.AddCommand.Execute(null);
            }

            if (_viewModel.Credentials.Count > countBeforeSave)
            {
                ReturnToMainView();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ReturnToMainView();
        }

        private void AddPasswordView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ReturnToMainView();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Save_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ReturnToMainView()
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new MainView(_viewModel));
            }
        }
    }
}

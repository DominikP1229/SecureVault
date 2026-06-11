using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class CategoriesView : UserControl
    {
        private readonly CategoriesViewModel _viewModel;

        public CategoriesView()
            : this(new MainViewModel())
        {
        }

        public CategoriesView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _viewModel = new CategoriesViewModel(mainViewModel);
            DataContext = _viewModel;
            _viewModel.CloseRequested += CloseSubView;
        }

        private void CategoriesView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.AddCategoryCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.CloseCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void CloseSubView()
        {
            _viewModel.CloseRequested -= CloseSubView;

            var parent = this.Parent as FrameworkElement;

            while (parent != null && parent.Name != "SubViewContainer")
            {
                parent = parent.Parent as FrameworkElement;
            }

            if (parent is Border border)
            {
                border.Visibility = Visibility.Collapsed;

                if (border.Child is ContentControl content)
                {
                    content.Content = null;
                }
            }
        }
    }
}

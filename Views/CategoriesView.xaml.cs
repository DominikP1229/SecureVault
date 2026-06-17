using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;

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

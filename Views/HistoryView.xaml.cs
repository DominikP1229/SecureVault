using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class HistoryView : UserControl
    {
        private readonly HistoryViewModel _viewModel = new();

        public HistoryView()
        {
            InitializeComponent();
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

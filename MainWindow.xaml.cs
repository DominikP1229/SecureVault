using SecureVault.Model.Services;
using SecureVault.Views;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SecureVault
{
    public partial class MainWindow : Window
    {
        private readonly Queue<AppNotification> _pendingNotifications = new();
        private AppNotification? _currentNotification;

        public MainWindow()
        {
            InitializeComponent();
            NotificationService.NotificationRequested += HandleNotificationRequested;
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

        private void HandleNotificationRequested(AppNotification notification)
        {
            Dispatcher.Invoke(() =>
            {
                _pendingNotifications.Enqueue(notification);

                if (_currentNotification == null)
                {
                    ShowNextNotification();
                }
            });
        }

        private void ShowNextNotification()
        {
            if (_pendingNotifications.Count == 0)
            {
                NotificationOverlay.Visibility = Visibility.Collapsed;
                _currentNotification = null;
                return;
            }

            _currentNotification = _pendingNotifications.Dequeue();
            NotificationTitleText.Text = _currentNotification.Title;
            NotificationMessageText.Text = _currentNotification.Message;
            NotificationTitleText.Foreground = GetNotificationBrush(_currentNotification.Kind);
            NotificationOkButtons.Visibility = _currentNotification.RequiresConfirmation ? Visibility.Collapsed : Visibility.Visible;
            NotificationConfirmButtons.Visibility = _currentNotification.RequiresConfirmation ? Visibility.Visible : Visibility.Collapsed;
            NotificationOverlay.Visibility = Visibility.Visible;
        }

        private Brush GetNotificationBrush(NotificationKind kind)
        {
            var resourceKey = kind switch
            {
                NotificationKind.Warning => "WarningBrush",
                NotificationKind.Error => "DangerBrush",
                NotificationKind.Question => "AccentBrush",
                _ => "TextPrimaryBrush"
            };

            return TryFindResource(resourceKey) as Brush ?? Brushes.White;
        }

        private void CompleteNotification(NotificationResult result)
        {
            _currentNotification?.Complete(result);
            _currentNotification = null;
            ShowNextNotification();
        }

        private void NotificationOk_Click(object sender, RoutedEventArgs e)
        {
            CompleteNotification(NotificationResult.Ok);
        }

        private void NotificationYes_Click(object sender, RoutedEventArgs e)
        {
            CompleteNotification(NotificationResult.Yes);
        }

        private void NotificationNo_Click(object sender, RoutedEventArgs e)
        {
            CompleteNotification(NotificationResult.No);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            NotificationService.NotificationRequested -= HandleNotificationRequested;
            base.OnClosed(e);
        }
    }
}

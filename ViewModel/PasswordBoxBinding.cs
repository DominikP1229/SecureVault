using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SecureVault.ViewModel
{
    public static class PasswordBoxBinding
    {
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxBinding),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxBinding),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(PasswordBoxBinding));

        public static string GetBoundPassword(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(BoundPasswordProperty)?.ToString() ?? string.Empty;
        }

        public static void SetBoundPassword(DependencyObject dependencyObject, string value)
        {
            dependencyObject.SetValue(BoundPasswordProperty, value);
        }

        public static bool GetBindPassword(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(BindPasswordProperty);
        }

        public static void SetBindPassword(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(BindPasswordProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsUpdatingProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not PasswordBox passwordBox)
            {
                return;
            }

            if (!GetIsUpdating(passwordBox))
            {
                passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnBindPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not PasswordBox passwordBox)
            {
                return;
            }

            passwordBox.PasswordChanged -= HandlePasswordChanged;

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not PasswordBox passwordBox)
            {
                return;
            }

            SetIsUpdating(passwordBox, true);
            passwordBox.SetCurrentValue(BoundPasswordProperty, passwordBox.Password);
            BindingOperations.GetBindingExpression(passwordBox, BoundPasswordProperty)?.UpdateSource();
            SetIsUpdating(passwordBox, false);
        }
    }
}

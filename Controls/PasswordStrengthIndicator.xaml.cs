using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SecureVault.Controls
{
    public partial class PasswordStrengthIndicator : UserControl
    {
        public static readonly DependencyProperty StrengthProperty =
            DependencyProperty.Register(
                nameof(Strength),
                typeof(int),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata(0, OnStrengthChanged));

        public static readonly DependencyProperty StrengthLabelProperty =
            DependencyProperty.Register(
                nameof(StrengthLabel),
                typeof(string),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata("Empty"));

        public static readonly DependencyProperty StrengthBrushProperty =
            DependencyProperty.Register(
                nameof(StrengthBrush),
                typeof(Brush),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register(
                nameof(BarWidth),
                typeof(double),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata(0d));

        public PasswordStrengthIndicator()
        {
            InitializeComponent();
            UpdateState();
        }

        public int Strength
        {
            get => (int)GetValue(StrengthProperty);
            set => SetValue(StrengthProperty, value);
        }

        public string StrengthLabel
        {
            get => (string)GetValue(StrengthLabelProperty);
            private set => SetValue(StrengthLabelProperty, value);
        }

        public Brush StrengthBrush
        {
            get => (Brush)GetValue(StrengthBrushProperty);
            private set => SetValue(StrengthBrushProperty, value);
        }

        public double BarWidth
        {
            get => (double)GetValue(BarWidthProperty);
            private set => SetValue(BarWidthProperty, value);
        }

        private static void OnStrengthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is PasswordStrengthIndicator indicator)
            {
                indicator.UpdateState();
            }
        }

        private void UpdateState()
        {
            var clampedStrength = Strength switch
            {
                < 0 => 0,
                > 100 => 100,
                _ => Strength
            };

            StrengthLabel = clampedStrength switch
            {
                0 => "Empty",
                < 50 => "Weak",
                < 83 => "Medium",
                _ => "Strong"
            };

            StrengthBrush = clampedStrength switch
            {
                0 => FindBrush("DangerBrush"),
                < 50 => FindBrush("DangerBrush"),
                < 83 => FindBrush("WarningBrush"),
                _ => FindBrush("SuccessBrush")
            };

            BarWidth = 260d * clampedStrength / 100d;
        }

        private Brush FindBrush(string resourceKey)
        {
            return TryFindResource(resourceKey) as Brush ?? Brushes.Transparent;
        }
    }
}

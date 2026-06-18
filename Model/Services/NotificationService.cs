using System;
using System.Media;
using System.Threading.Tasks;

namespace SecureVault.Model.Services
{
    public enum NotificationKind
    {
        Information,
        Warning,
        Error,
        Question
    }

    public enum NotificationResult
    {
        Ok,
        Yes,
        No
    }

    public class AppNotification
    {
        private readonly TaskCompletionSource<NotificationResult> _completionSource = new();

        public AppNotification(string title, string message, NotificationKind kind, bool requiresConfirmation)
        {
            Title = title;
            Message = message;
            Kind = kind;
            RequiresConfirmation = requiresConfirmation;
        }

        public string Title { get; }
        public string Message { get; }
        public NotificationKind Kind { get; }
        public bool RequiresConfirmation { get; }
        public Task<NotificationResult> Completion => _completionSource.Task;

        public void Complete(NotificationResult result)
        {
            _completionSource.TrySetResult(result);
        }
    }

    public static class NotificationService
    {
        public static event Action<AppNotification>? NotificationRequested;

        public static Task<NotificationResult> ShowInformationAsync(string title, string message)
            => ShowAsync(title, message, NotificationKind.Information);

        public static Task<NotificationResult> ShowWarningAsync(string title, string message)
            => ShowAsync(title, message, NotificationKind.Warning);

        public static Task<NotificationResult> ShowErrorAsync(string title, string message)
            => ShowAsync(title, message, NotificationKind.Error);

        public static Task<NotificationResult> ConfirmWarningAsync(string title, string message)
            => ShowAsync(title, message, NotificationKind.Warning, requiresConfirmation: true);

        private static Task<NotificationResult> ShowAsync(
            string title,
            string message,
            NotificationKind kind,
            bool requiresConfirmation = false)
        {
            PlaySound(kind);

            var notification = new AppNotification(title, message, kind, requiresConfirmation);
            var handler = NotificationRequested;
            if (handler == null)
            {
                return Task.FromResult(requiresConfirmation ? NotificationResult.No : NotificationResult.Ok);
            }

            handler(notification);
            return notification.Completion;
        }

        private static void PlaySound(NotificationKind kind)
        {
            switch (kind)
            {
                case NotificationKind.Warning:
                    SystemSounds.Exclamation.Play();
                    break;
                case NotificationKind.Error:
                    SystemSounds.Hand.Play();
                    break;
                case NotificationKind.Question:
                    SystemSounds.Question.Play();
                    break;
                default:
                    SystemSounds.Asterisk.Play();
                    break;
            }
        }
    }
}

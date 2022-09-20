using Avalonia.Notification;
using Avalonia.Threading;
using System;

namespace EDVTrader.Common
{
    public class Notifications
    {
        private INotificationMessageManager _manager;

        public Notifications(INotificationMessageManager manager)
        {
            _manager = manager;
        }

        public void ShowError(string header, string message, string buttonText = "Hide", Action<INotificationMessageButton>? action = null)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() => ShowError(header, message, buttonText, action));
                return;
            }
            
            _manager
                .CreateMessage()
                .Accent("#5e0825").Background("#850E35")
                .HasHeader(header)
                .HasMessage(message)
                .Animates(true)
                .Dismiss().WithDelay(TimeSpan.FromSeconds(5))
                .Dismiss().WithButton(buttonText, action ?? new Action<INotificationMessageButton>((button) => { }))
                .Queue();
        }

        public void ShowWarning(string header, string message, string buttonText = "Hide", Action<INotificationMessageButton>? action = null)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() => ShowWarning(header, message, buttonText, action));
                return;
            }

            _manager
                .CreateMessage()
                .Accent("#AC4425").Background("#D07000")
                .HasHeader(header)
                .HasMessage(message)
                .Animates(true)
                .Dismiss().WithDelay(TimeSpan.FromSeconds(5))
                .Dismiss().WithButton(buttonText, action ?? new Action<INotificationMessageButton>((button) => { }))
                .Queue();
        }
    }
}

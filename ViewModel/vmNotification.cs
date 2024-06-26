using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using VisualHFT.Commons.Helpers;
using VisualHFT.Commons.Model;
using VisualHFT.Helpers;
using System.Windows;

namespace VisualHFT.ViewModels
{
    public class vmNotifications : BindableBase
    {
        public ObservableCollection<ErrorNotification> Notifications { get; }
        public ObservableCollection<ErrorNotification> GroupedNotifications { get; }

        public ICommand MarkAllAsReadCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand ToggleNotificationsCommand { get; }

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            private set => SetProperty(ref _unreadCount, value);
        }
        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            private set => SetProperty(ref _totalCount, value);
        }

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => SetProperty(ref _isPopupOpen, value);
        }

        public vmNotifications()
        {
            Notifications = new ObservableCollection<ErrorNotification>(HelperNotificationManager.Instance.GetAllNotifications());
            GroupedNotifications = new ObservableCollection<ErrorNotification>();

            MarkAllAsReadCommand = new RelayCommand<object>(param => MarkAllAsRead());
            ClearAllCommand = new RelayCommand<object>(param => ClearAll());
            ToggleNotificationsCommand = new RelayCommand<object>(param => ToggleNotifications());

            UpdateUnreadCount();
            UpdateGroupedNotifications();
            Notifications.CollectionChanged += (s, e) =>
            {
                UpdateUnreadCount();
                UpdateGroupedNotifications();
            };

            // Subscribe to the NotificationAdded event
            HelperNotificationManager.Instance.NotificationAdded += OnNotificationAdded;
        }

        private void OnNotificationAdded(object sender, ErrorNotificationEventArgs e)
        {
            // Ensure the collection is updated on the UI thread
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Notifications.Add(e.Notification);
                UpdateGroupedNotifications();
            });
        }

        private void MarkAllAsRead()
        {
            foreach (var notification in Notifications)
            {
                notification.IsRead = true;
            }
            UpdateUnreadCount();
        }

        private void ClearAll()
        {
            Notifications.Clear();
            GroupedNotifications.Clear();
            HelperNotificationManager.Instance.ClearNotifications();
            UpdateUnreadCount();
            _totalCount = 0;
        }

        private void ToggleNotifications()
        {
            IsPopupOpen = !IsPopupOpen;
            if (IsPopupOpen)
            {
                foreach (var notification in Notifications)
                {
                    notification.IsRead = true;
                }
                UpdateUnreadCount();
            }
        }

        private void UpdateUnreadCount()
        {
            UnreadCount = Notifications.Count(n => !n.IsRead);
            TotalCount = Notifications.Count;
        }

        private void UpdateGroupedNotifications()
        {
            var grouped = Notifications
                .GroupBy(n => new { n.Title, n.NotificationType })
                .Select(g => new ErrorNotification
                {
                    Title = g.Key.Title + (g.Count() > 1 ? $" ({g.Count()})" : ""),
                    NotificationType = g.Key.NotificationType,
                    Message = g.Last().Message,
                    Timestamp = g.Last().Timestamp
                });

            GroupedNotifications.Clear();
            foreach (var notification in grouped)
            {
                GroupedNotifications.Add(notification);
            }
        }
    }
}

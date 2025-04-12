using TVOnline.Models;

namespace TVOnline.ViewModels.Notification
{
    public class NotificationsViewModel
    {
        public IEnumerable<Models.Notification> Notifications { get; set; }
        public int UnreadCount { get; set; }
    }
}

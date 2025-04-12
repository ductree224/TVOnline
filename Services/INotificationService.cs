namespace TVOnline.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string? link = null);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);
    }
}

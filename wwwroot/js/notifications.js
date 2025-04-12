$(document).ready(function() {
    // Load notifications when dropdown is shown
    $('#notificationDropdown').on('shown.bs.dropdown', function() {
        loadNotifications();
    });

    // Function to load notifications
    function loadNotifications() {
        $.get('/Account/GetNotifications', function(data) {
            var container = $('.notifications-list');
            container.empty();

            if (data.length === 0) {
                container.html('<div class="p-3 text-center text-muted">Không có thông báo mới</div>');
                return;
            }

            data.forEach(function(notification) {
                var notificationClass = notification.isRead ? '' : 'bg-light';
                var notificationHtml = `
                    <div class="notification-item ${notificationClass} p-2 border-bottom" data-id="${notification.id}">
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="notification-content">
                                <h6 class="mb-1">${notification.title}</h6>
                                <p class="mb-1 small">${notification.message}</p>
                                <small class="text-muted">${new Date(notification.createdAt).toLocaleString()}</small>
                            </div>
                            ${!notification.isRead ? '<span class="badge bg-primary">Mới</span>' : ''}
                        </div>
                        ${notification.link ? `<a href="${notification.link}" class="stretched-link"></a>` : ''}
                    </div>
                `;
                container.append(notificationHtml);
            });
        });
    }

    // Mark notification as read when clicked
    $(document).on('click', '.notification-item', function() {
        var notificationId = $(this).data('id');
        if (notificationId) {
            $.post('/Account/MarkNotificationAsRead', { id: notificationId }, function() {
                loadNotifications();
                updateNotificationCount();
            });
        }
    });

    // Function to update notification count
    function updateNotificationCount() {
        $.get('/Account/GetUnreadNotificationCount', function(count) {
            var badge = $('#notificationBadge');
            if (count > 0) {
                badge.text(count > 99 ? '99+' : count);
                badge.show();
            } else {
                badge.hide();
            }
        });
    }

    // Update notification count periodically
    setInterval(updateNotificationCount, 30000); // Update every 30 seconds

    // Make updateNotificationCount function globally available
    window.updateNotificationCount = updateNotificationCount;
}); 
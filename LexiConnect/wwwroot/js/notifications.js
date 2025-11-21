// Global notification system
(function() {
    'use strict';

    let notificationConnection = null;
    let notificationContainer = null;

    // Initialize notification system
    function initNotifications() {
        // Create notification container if it doesn't exist
        if (!notificationContainer) {
            notificationContainer = document.createElement('div');
            notificationContainer.className = 'notification-container';
            notificationContainer.id = 'notificationContainer';
            document.body.appendChild(notificationContainer);
        }

        // Initialize SignalR connection for notifications
        if (typeof signalR !== 'undefined') {
            notificationConnection = new signalR.HubConnectionBuilder()
                .withUrl("/ChatHub")
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .build();

            // Listen for notifications
            notificationConnection.on("ReceiveNotification", function(notification) {
                console.log('Received notification:', notification);
                if (notification && notification.type) {
                    showNotification(notification);
                } else {
                    console.warn('Invalid notification format:', notification);
                }
            });
            
            // Listen for connection errors
            notificationConnection.onclose(function(error) {
                if (error) {
                    console.error('Notification connection closed with error:', error);
                } else {
                    console.log('Notification connection closed');
                }
            });

            // Start connection
            notificationConnection.start()
                .then(function() {
                    console.log('Notification connection started successfully');
                })
                .catch(function(err) {
                    console.error('Error starting notification connection:', err);
                    // Try to reconnect after a delay if it's an authentication error
                    if (err.message && err.message.includes('401')) {
                        console.log('Authentication error. User may need to log in.');
                    }
                });

            // Handle reconnection
            notificationConnection.onreconnecting(function() {
                console.log('Notification connection reconnecting...');
            });

            notificationConnection.onreconnected(function() {
                console.log('Notification connection reconnected');
            });
        }
    }

    // Show a notification
    function showNotification(data) {
        if (!notificationContainer) {
            initNotifications();
        }

        const notification = document.createElement('div');
        notification.className = `notification ${data.type || ''}`;
        
        // Determine icon based on type
        let icon = '🔔';
        if (data.type === 'new_message') {
            icon = '💬';
        } else if (data.type === 'points_granted') {
            icon = '⭐';
        }

        notification.innerHTML = `
            <div class="notification-icon">${icon}</div>
            <div class="notification-content">
                <div class="notification-title">${escapeHtml(data.title || 'Notification')}</div>
                <div class="notification-message">${escapeHtml(data.message || '')}</div>
            </div>
            <button class="notification-close" onclick="this.closest('.notification').remove()">&times;</button>
        `;

        // Add click handler for redirection
        if (data.redirectUrl) {
            notification.addEventListener('click', function(e) {
                // Don't redirect if clicking the close button
                if (!e.target.classList.contains('notification-close')) {
                    window.location.href = data.redirectUrl;
                }
            });
        }

        // Add to container
        notificationContainer.appendChild(notification);

        // Auto-remove after 5 seconds
        setTimeout(function() {
            if (notification.parentNode) {
                notification.classList.add('removing');
                setTimeout(function() {
                    if (notification.parentNode) {
                        notification.remove();
                    }
                }, 300);
            }
        }, 5000);
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Expose showNotification globally for manual use
    window.showNotification = showNotification;

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initNotifications);
    } else {
        initNotifications();
    }
})();


// User Management JavaScript
document.addEventListener('DOMContentLoaded', function () {
    initializeSearchAndFilters();
    initializeModals();
    highlightSearchTerms();
});

// Initialize search and filter functionality
function initializeSearchAndFilters() {
    const searchInput = document.querySelector('input[name="search"]');
    const filterSelects = document.querySelectorAll('.select-input');

    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                this.form.submit();
            }
        });
    }

    filterSelects.forEach(select => {
        select.addEventListener('change', function () {
            if (this.name === 'subscription' || this.name === 'university') {
                this.form.submit();
            }
        });
    });
}

// Initialize modals
function initializeModals() {
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('modal')) {
            closeAllModals();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllModals();
        }
    });
}

function closeAllModals() {
    closePointsModal();
    closeSuspendModal();
    closeUnsuspendModal();
    closeDeleteModal();
}

// Points adjustment functionality
let currentUserId = null;

function showPointsModal(userId, userName, currentPoints) {
    const modal = document.getElementById('pointsModal');
    const userNameSpan = document.getElementById('pointsUserName');
    const currentPointsSpan = document.getElementById('currentPoints');
    const pointsChangeInput = document.getElementById('pointsChange');
    const pointsReasonInput = document.getElementById('pointsReason');
    const confirmBtn = document.getElementById('confirmPointsBtn');

    if (!modal) return;

    currentUserId = userId;
    userNameSpan.textContent = userName;
    currentPointsSpan.textContent = currentPoints;
    pointsChangeInput.value = '';
    pointsReasonInput.value = '';
    modal.style.display = 'block';

    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

    newConfirmBtn.addEventListener('click', function () {
        const pointsChange = parseInt(pointsChangeInput.value);
        const reason = pointsReasonInput.value;

        if (isNaN(pointsChange) || pointsChange === 0) {
            showNotification('Please enter a valid points amount', 'error');
            return;
        }

        performPointsAdjustment(userId, pointsChange, reason);
    });
}

function performPointsAdjustment(userId, pointsChange, reason) {
    const confirmBtn = document.getElementById('confirmPointsBtn');
    const originalText = confirmBtn.textContent;
    confirmBtn.textContent = 'Processing...';
    confirmBtn.disabled = true;

    fetch('/Admin/AdjustUserPoints', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({
            id: userId,
            pointsChange: pointsChange,
            reason: reason
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(`Points ${pointsChange > 0 ? 'added' : 'deducted'} successfully`, 'success');
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                showNotification('Error: ' + data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('An error occurred while adjusting points', 'error');
        })
        .finally(() => {
            confirmBtn.textContent = originalText;
            confirmBtn.disabled = false;
            closePointsModal();
        });
}

function closePointsModal() {
    const modal = document.getElementById('pointsModal');
    if (modal) {
        modal.style.display = 'none';
    }
    currentUserId = null;
}

// Suspend user functionality
function suspendUser(userId, userName) {
    const modal = document.getElementById('suspendModal');
    const userNameSpan = document.getElementById('suspendUserName');
    const confirmBtn = document.getElementById('confirmSuspendBtn');

    if (!modal) return;

    userNameSpan.textContent = userName;
    modal.style.display = 'block';

    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

    newConfirmBtn.addEventListener('click', function () {
        performSuspendUser(userId);
    });
}

function performSuspendUser(userId) {
    const confirmBtn = document.getElementById('confirmSuspendBtn');
    const originalText = confirmBtn.textContent;
    confirmBtn.textContent = 'Suspending...';
    confirmBtn.disabled = true;

    fetch('/Admin/SuspendUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({ id: userId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('User suspended successfully', 'success');
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                showNotification('Error: ' + data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('An error occurred while suspending user', 'error');
        })
        .finally(() => {
            confirmBtn.textContent = originalText;
            confirmBtn.disabled = false;
            closeSuspendModal();
        });
}

function closeSuspendModal() {
    const modal = document.getElementById('suspendModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Unsuspend user functionality
function unsuspendUser(userId, userName) {
    const modal = document.getElementById('unsuspendModal');
    const userNameSpan = document.getElementById('unsuspendUserName');
    const confirmBtn = document.getElementById('confirmUnsuspendBtn');

    if (!modal) return;

    userNameSpan.textContent = userName;
    modal.style.display = 'block';

    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

    newConfirmBtn.addEventListener('click', function () {
        performUnsuspendUser(userId);
    });
}

function performUnsuspendUser(userId) {
    const confirmBtn = document.getElementById('confirmUnsuspendBtn');
    const originalText = confirmBtn.textContent;
    confirmBtn.textContent = 'Unsuspending...';
    confirmBtn.disabled = true;

    fetch('/Admin/UnsuspendUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({ id: userId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('User unsuspended successfully', 'success');
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                showNotification('Error: ' + data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('An error occurred while unsuspending user', 'error');
        })
        .finally(() => {
            confirmBtn.textContent = originalText;
            confirmBtn.disabled = false;
            closeUnsuspendModal();
        });
}

function closeUnsuspendModal() {
    const modal = document.getElementById('unsuspendModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Delete user functionality
function deleteUser(userId, userName) {
    const modal = document.getElementById('deleteModal');
    const userNameSpan = document.getElementById('deleteUserName');
    const confirmBtn = document.getElementById('confirmDeleteBtn');

    if (!modal) return;

    userNameSpan.textContent = userName;
    modal.style.display = 'block';

    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

    newConfirmBtn.addEventListener('click', function () {
        performDeleteUser(userId);
    });
}

function performDeleteUser(userId) {
    const confirmBtn = document.getElementById('confirmDeleteBtn');
    const originalText = confirmBtn.textContent;
    confirmBtn.textContent = 'Deleting...';
    confirmBtn.disabled = true;

    fetch('/Admin/DeleteUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({ id: userId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('User deleted successfully', 'success');
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                showNotification('Error: ' + data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('An error occurred while deleting user', 'error');
        })
        .finally(() => {
            confirmBtn.textContent = originalText;
            confirmBtn.disabled = false;
            closeDeleteModal();
        });
}

function closeDeleteModal() {
    const modal = document.getElementById('deleteModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Highlight search terms
function highlightSearchTerms() {
    const searchTerm = getUrlParameter('search');
    if (searchTerm && searchTerm.length > 2) {
        const regex = new RegExp(`(${escapeRegExp(searchTerm)})`, 'gi');
        document.querySelectorAll('.user-details h4, .user-details p, .user-details small').forEach(element => {
            if (element.innerHTML && !element.querySelector('mark')) {
                element.innerHTML = element.innerHTML.replace(regex, '<mark>$1</mark>');
            }
        });
    }
}

// Utility functions
function getUrlParameter(name) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(name);
}

function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) {
        return tokenInput.value;
    }

    const tokenMeta = document.querySelector('meta[name="__RequestVerificationToken"]');
    if (tokenMeta) {
        return tokenMeta.getAttribute('content');
    }

    return '';
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <span>${message}</span>
        <button class="notification-close" onclick="this.parentElement.remove()">&times;</button>
    `;

    let container = document.querySelector('.notification-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'notification-container';
        document.body.appendChild(container);
    }

    container.appendChild(notification);

    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

// Add notification styles
if (!document.querySelector('#notification-styles')) {
    const style = document.createElement('style');
    style.id = 'notification-styles';
    style.textContent = `
        .notification-container {
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 10000;
        }
        
        .notification {
            background: #fff;
            border-radius: 6px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            padding: 16px 20px;
            margin-bottom: 10px;
            max-width: 400px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            border-left: 4px solid #007bff;
            animation: slideIn 0.3s ease-out;
        }
        
        .notification-success { border-left-color: #28a745; }
        .notification-error { border-left-color: #dc3545; }
        .notification-warning { border-left-color: #ffc107; }
        
        .notification-close {
            background: none;
            border: none;
            font-size: 18px;
            cursor: pointer;
            margin-left: 10px;
            opacity: 0.7;
        }
        
        .notification-close:hover {
            opacity: 1;
        }
        
        @keyframes slideIn {
            from { transform: translateX(100%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
        
        mark {
            background-color: #fff3cd;
            padding: 2px 4px;
            border-radius: 3px;
        }
    `;
    document.head.appendChild(style);
}
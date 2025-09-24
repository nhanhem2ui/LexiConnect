// Document Management JavaScript
document.addEventListener('DOMContentLoaded', function () {
    // Initialize functionality
    initializeSearchAndFilters();
    initializeModals();
    highlightSearchTerms();
});

// Initialize search and filter functionality
function initializeSearchAndFilters() {
    const searchInput = document.querySelector('input[name="search"]');
    const filterSelects = document.querySelectorAll('.select-input');

    // Auto-submit on Enter key for search
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                this.form.submit();
            }
        });

        // Optional: Auto-submit after typing stops (debounced)
        let searchTimeout;
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                // Uncomment the line below if you want auto-submit on typing
                // this.form.submit();
            }, 500);
        });
    }

    // Auto-submit form when filters change (optional)
    filterSelects.forEach(select => {
        select.addEventListener('change', function () {
            // Only auto-submit for filter selects, not sort selects
            if (this.name === 'status' || this.name === 'type' || this.name === 'course' || this.name === 'uploader') {
                this.form.submit();
            }
        });
    });
}

// Initialize modals
function initializeModals() {
    // Close modals when clicking outside
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('modal')) {
            closeDeleteModal();
            closeRejectModal();
        }
    });

    // Close modals with Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeDeleteModal();
            closeRejectModal();
        }
    });
}

// Delete document functionality
function deleteDocument(documentId, documentTitle) {
    const modal = document.getElementById('deleteModal');
    const titleSpan = document.getElementById('documentTitle');
    const confirmBtn = document.getElementById('confirmDeleteBtn');

    if (!modal || !titleSpan || !confirmBtn) {
        console.error('Delete modal elements not found');
        return;
    }

    titleSpan.textContent = documentTitle;
    modal.style.display = 'block';

    // Remove any existing event listeners
    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

    // Add new event listener
    newConfirmBtn.addEventListener('click', function () {
        performDeleteDocument(documentId);
    });
}

function performDeleteDocument(documentId) {
    // Show loading state
    const confirmBtn = document.getElementById('confirmDeleteBtn');
    const originalText = confirmBtn.textContent;
    confirmBtn.textContent = 'Deleting...';
    confirmBtn.disabled = true;

    fetch('/Admin/DeleteDocument', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({ id: documentId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Show success message
                showNotification('Document deleted successfully', 'success');
                // Refresh the page or remove the row
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                showNotification('Error: ' + data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('An error occurred while deleting the document', 'error');
        })
        .finally(() => {
            // Reset button state
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

// Reject document functionality
function showRejectModal(documentId, documentTitle) {
    const modal = document.getElementById('rejectModal');
    const titleSpan = document.getElementById('rejectDocumentTitle');
    const confirmBtn = document.getElementById('confirmRejectBtn');
    const reasonTextarea = document.getElementById('rejectionReason');

    if (!modal || !titleSpan || !confirmBtn) {
        console.error('Reject modal elements not found');
        return;
    }

    titleSpan.textContent = documentTitle;
    reasonTextarea.value = '';
    modal.style.display = 'block';

    // Remove any existing event listeners
    const newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);

    // Add new event listener
    newConfirmBtn.addEventListener('click', function () {
        performRejectDocument(documentId, reasonTextarea.value);
    });
}

function performRejectDocument(documentId, rejectionReason) {
    // Show loading state
    const confirmBtn = document.getElementById('confirmRejectBtn');
    const originalText = confirmBtn.textContent;
    confirmBtn.textContent = 'Rejecting...';
    confirmBtn.disabled = true;

    fetch('/Admin/RejectDocument', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({
            id: documentId,
            rejectionReason: rejectionReason
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('Document rejected successfully', 'success');
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                showNotification('Error: ' + data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('An error occurred while rejecting the document', 'error');
        })
        .finally(() => {
            // Reset button state
            confirmBtn.textContent = originalText;
            confirmBtn.disabled = false;
            closeRejectModal();
        });
}

function closeRejectModal() {
    const modal = document.getElementById('rejectModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Highlight search terms in results
function highlightSearchTerms() {
    const searchTerm = getUrlParameter('search');
    if (searchTerm && searchTerm.length > 2) {
        const regex = new RegExp(`(${escapeRegExp(searchTerm)})`, 'gi');
        document.querySelectorAll('.title-text h4, .title-text p, .uploader-info strong, .course-info strong').forEach(element => {
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
    // Try to get the token from a hidden input field
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) {
        return tokenInput.value;
    }

    // Try to get from a meta tag
    const tokenMeta = document.querySelector('meta[name="__RequestVerificationToken"]');
    if (tokenMeta) {
        return tokenMeta.getAttribute('content');
    }

    // Return empty string if not found (you might want to handle this differently)
    return '';
}

function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <span>${message}</span>
        <button class="notification-close" onclick="this.parentElement.remove()">&times;</button>
    `;

    // Add to page
    let container = document.querySelector('.notification-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'notification-container';
        document.body.appendChild(container);
    }

    container.appendChild(notification);

    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

// Table row actions
function toggleRowDetails(button) {
    const row = button.closest('tr');
    const detailsRow = row.nextElementSibling;

    if (detailsRow && detailsRow.classList.contains('details-row')) {
        detailsRow.style.display = detailsRow.style.display === 'none' ? 'table-row' : 'none';
        button.textContent = detailsRow.style.display === 'none' ? '▼' : '▲';
    }
}

// Export functionality (if needed)
function exportDocuments() {
    const currentUrl = new URL(window.location);
    currentUrl.pathname = '/Admin/ExportDocuments';
    window.open(currentUrl.toString(), '_blank');
}

// Bulk actions (if needed)
function selectAllDocuments(checkbox) {
    const checkboxes = document.querySelectorAll('input[name="selectedDocuments"]');
    checkboxes.forEach(cb => cb.checked = checkbox.checked);
    updateBulkActions();
}

function updateBulkActions() {
    const selectedCheckboxes = document.querySelectorAll('input[name="selectedDocuments"]:checked');
    const bulkActionsDiv = document.querySelector('.bulk-actions');

    if (bulkActionsDiv) {
        bulkActionsDiv.style.display = selectedCheckboxes.length > 0 ? 'block' : 'none';
    }
}

// Performance optimization: Debounce function
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Add CSS for notifications if not already present
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
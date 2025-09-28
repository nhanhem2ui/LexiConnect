// Modal functions
function showConfirmModal(title, message, onConfirm) {
    document.getElementById('modalTitle').textContent = title;
    document.getElementById('modalMessage').textContent = message;
    document.getElementById('confirmModal').style.display = 'flex';

    document.getElementById('confirmBtn').onclick = () => {
        onConfirm();
        hideConfirmModal();
    };
}

function hideConfirmModal() {
    document.getElementById('confirmModal').style.display = 'none';
}

document.getElementById('cancelBtn').onclick = hideConfirmModal;

// Close modal when clicking outside
document.getElementById('confirmModal').onclick = function (e) {
    if (e.target === this) {
        hideConfirmModal();
    }
};
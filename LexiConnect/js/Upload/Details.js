
// Add this to your Details.js or create a new cleanup script


function removeDocument(element, documentId) {
    if (confirm('Are you sure you want to remove this document?')) {
        // Show loading state
        const section = element.closest('.doc-section');
        if (section) {
            section.style.opacity = '0.5';
            section.style.pointerEvents = 'none';
        }

        fetch('/Upload/DeleteDocument', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: 'id=' + encodeURIComponent(documentId) + '&__RequestVerificationToken=' + encodeURIComponent(document.querySelector('input[name="__RequestVerificationToken"]').value)
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    console.log("Document deleted successfully");

                    // Remove the document section from DOM
                    if (section) {
                        section.remove();
                        console.log("Document section removed from DOM");

                        // Check if there are any remaining documents in DOM
                        const remainingDocs = document.querySelectorAll('.doc-section');
                        console.log(`Remaining documents in DOM: ${remainingDocs.length}`);

                        // Only redirect if there are truly no documents left
                        setTimeout(() => {
                            const finalCheck = document.querySelectorAll('.doc-section');
                            if (finalCheck.length === 0) {
                                const container = document.querySelector('.doc-container');
                                container.innerHTML = `
                                     <div class="doc-empty">
                                         <p>All documents have been deleted.</p>
                                         <a href="/Upload/Index" class="doc-next-btn">Upload More Documents</a>
                                     </div>
                                 `;
                            }
                        }, 100);

                    } else {
                        console.error("Could not find document section to remove");
                        // Reload the page to sync with server state
                        window.location.reload();
                    }
                } else {
                    console.error("Failed to delete document:", data.error);
                    alert('Failed to delete document: ' + (data.error || 'Unknown error'));

                    // Restore section state
                    if (section) {
                        section.style.opacity = '1';
                        section.style.pointerEvents = 'auto';
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while deleting the document.');

                // Restore section state
                if (section) {
                    section.style.opacity = '1';
                    section.style.pointerEvents = 'auto';
                }
            });
    }
}

// Auto-resize textareas
document.addEventListener('DOMContentLoaded', function () {
    const textareas = document.querySelectorAll('textarea');
    textareas.forEach(textarea => {
        textarea.addEventListener('input', function () {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        });
    });
});

// Enhanced form validation that checks for remaining documents
document.querySelector('form').addEventListener('submit', function (e) {
    // First check if there are any documents left
    const docSections = document.querySelectorAll('.doc-section');
    if (docSections.length === 0) {
        e.preventDefault();
        alert('You need at least one document to continue. Redirecting to upload page...');
        window.location.href = '/Upload/Index';
        return;
    }

    const requiredFields = this.querySelectorAll('input[required], select[required]');
    let isValid = true;

    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });

    if (!isValid) {
        e.preventDefault();
        alert('Please fill in all required fields.');
    }
});

// Auto-resize textareas
document.addEventListener('DOMContentLoaded', function () {
    const textareas = document.querySelectorAll('textarea');
    textareas.forEach(textarea => {
        textarea.addEventListener('input', function () {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        });
    });
});

// Form validation
document.querySelector('form').addEventListener('submit', function (e) {
    const requiredFields = this.querySelectorAll('input[required], select[required]');
    let isValid = true;

    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });

    if (!isValid) {
        e.preventDefault();
        alert('Please fill in all required fields.');
    }
});
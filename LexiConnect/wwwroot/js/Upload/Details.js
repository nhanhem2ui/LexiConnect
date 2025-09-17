function removeDocument(element, documentId) {
    if (confirm('Are you sure you want to remove this document?')) {
        // Sửa lỗi: thiếu dấu phẩy giữa headers và body
        fetch('/Upload/DeleteDocument?id=' + documentId, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },  // Thêm dấu phẩy này
            body: '__RequestVerificationToken=' + encodeURIComponent(document.querySelector('input[name="__RequestVerificationToken"]').value)
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    console.log("data success");

                    // Debug: Kiểm tra element và parent elements
                    console.log("Element clicked:", element);
                    console.log("Parent element:", element.parentElement);
                    console.log("Searching for .doc-section...");

                    // Tìm phần tử cha chứa document section
                    const section = element.closest('.doc-section');
                    console.log("Found section: ", section);

                    if (section) {
                        section.remove();
                        console.log("Document section removed successfully");
                    } else {
                        // Fallback: thử tìm bằng cách khác
                        console.log("Không tìm thấy .doc-section, thử cách khác...");
                        let currentElement = element;
                        while (currentElement && currentElement !== document.body) {
                            console.log("Checking element:", currentElement.className);
                            if (currentElement.classList && currentElement.classList.contains('doc-section')) {
                                currentElement.remove();
                                console.log("Document section removed using fallback method");
                                break;
                            }
                            currentElement = currentElement.parentElement;
                        }

                        if (!currentElement || currentElement === document.body) {
                            console.error("Could not find doc-section to remove");
                            alert("Could not remove document from the page. Please refresh and try again.");
                        }
                    }

                    // Kiểm tra xem còn document nào không
                    const remainingDocs = document.querySelectorAll('.doc-section');
                    if (remainingDocs.length === 0) {
                        alert('You need at least one document to continue.');
                        window.location.href = '/Upload/Index';
                    }
                } else {
                    console.log("Failed to delete document");
                    alert('Failed to delete document. Please try again.');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while deleting the document.');
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
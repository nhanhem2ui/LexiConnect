

document.addEventListener("DOMContentLoaded", function () {
    const url = window.pdfFileUrl;
    const isPremiumOnly = window.isPremiumOnly; // Lấy từ biến global
    const pdfjsLib = window['pdfjsLib'];
    let container = document.getElementById("pdf-container");

    if (!pdfjsLib) {
        console.error("PDF.js library not loaded");
        return;
    }

    pdfjsLib.getDocument(url).promise.then(function (pdf) {
        console.log("PDF loaded with", pdf.numPages, "pages");

        // Xác định số trang hiển thị dựa trên IsPremiumOnly
        let pagesToShow;
        if (isPremiumOnly) {
            // Nếu là Premium content, chỉ hiển thị 2 trang đầu
            pagesToShow = 2;
        } else {
            // Nếu không phải Premium, hiển thị tất cả
            pagesToShow = pdf.numPages;
        }

        const totalPages = Math.min(pdf.numPages, pagesToShow);

        // Render từng trang
        for (let pageNum = 1; pageNum <= totalPages; pageNum++) {
            pdf.getPage(pageNum).then(function (page) {
                // Tạo container cho mỗi trang
                let pageContainer = document.createElement("div");
                pageContainer.className = "pdf-page-container";
                pageContainer.style.marginBottom = "30px";
                pageContainer.style.border = "1px solid #ddd";
                pageContainer.style.padding = "10px";
                pageContainer.style.backgroundColor = "#f9f9f9";

                // Thêm số trang
                let pageLabel = document.createElement("div");
                pageLabel.className = "page-label";
                pageLabel.textContent = `Page ${pageNum} of ${pdf.numPages}`;
                pageLabel.style.textAlign = "center";
                pageLabel.style.fontWeight = "bold";
                pageLabel.style.marginBottom = "10px";
                pageLabel.style.color = "#333";
                pageContainer.appendChild(pageLabel);

                // Tạo canvas cho trang
                let scale = 1.3;
                let viewport = page.getViewport({ scale: scale });

                let canvas = document.createElement("canvas");
                canvas.style.display = "block";
                canvas.style.margin = "0 auto";
                canvas.style.boxShadow = "0 2px 8px rgba(0,0,0,0.1)";
                let context = canvas.getContext("2d");
                canvas.height = viewport.height;
                canvas.width = viewport.width;

                pageContainer.appendChild(canvas);
                container.appendChild(pageContainer);

                // Render trang
                let renderContext = {
                    canvasContext: context,
                    viewport: viewport
                };
                page.render(renderContext);
            });
        }

        // Hiển thị thông báo nếu là Premium content và có nhiều hơn số trang được hiển thị
        if (isPremiumOnly && pdf.numPages > pagesToShow) {
            let messageContainer = document.createElement("div");
            messageContainer.className = "more-pages-message";
            messageContainer.style.textAlign = "center";
            messageContainer.style.padding = "30px";
            messageContainer.style.marginTop = "20px";
            messageContainer.style.border = "2px dashed #ddd";
            messageContainer.style.borderRadius = "8px";
            messageContainer.style.backgroundColor = "#fff8dc";

            let icon = document.createElement("i");
            icon.className = "fa fa-lock";
            icon.style.fontSize = "32px";
            icon.style.color = "#ff6b6b";
            icon.style.marginBottom = "10px";
            icon.style.display = "block";

            let message = document.createElement("div");
            message.style.color = "#d63031";
            message.style.fontWeight = "bold";
            message.style.fontSize = "16px";
            message.style.marginBottom = "8px";
            message.innerHTML = `🔒 ${pdf.numPages - pagesToShow} page(s) more`;

            let subMessage = document.createElement("div");
            subMessage.style.color = "#636e72";
            subMessage.style.fontSize = "14px";
            subMessage.innerHTML = "Sign up Premium to view all the material";

            messageContainer.appendChild(icon);
            messageContainer.appendChild(message);
            messageContainer.appendChild(subMessage);
            container.appendChild(messageContainer);
        }

    }).catch(err => {
        console.error("PDF.js error:", err);
        container.innerHTML = `
            <div style="text-align:center; padding:40px; color:#e74c3c;">
                <i class="fa fa-exclamation-triangle" style="font-size:48px; margin-bottom:15px;"></i>
                <p style="font-weight:bold; font-size:18px;">Không thể tải PDF</p>
                <p style="font-size:14px; color:#666;">${err.message}</p>
            </div>
        `;
    });
});


// Favorite Toggle Logic
document.addEventListener('DOMContentLoaded', function () {
    const favoriteBtn = document.getElementById('favoriteBtn');
    const favoriteIcon = document.getElementById('favoriteIcon');
    const favoriteText = document.getElementById('favoriteText');

    if (favoriteBtn) {
        favoriteBtn.addEventListener('click', function () {
            const documentId = this.getAttribute('data-document-id');
            toggleFavorite(documentId);
        });
    }

    function toggleFavorite(documentId) {
        // Lấy URL từ biến global được set trong view
        const url = window.toggleFavoriteUrl;

        if (!url) {
            console.error('Toggle favorite URL not found');
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Configuration error. Please refresh the page.'
            });
            return;
        }

        // Lấy anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: new URLSearchParams({
                'id': documentId,
                '__RequestVerificationToken': token || ''
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Cập nhật UI
                    if (data.isFavorited) {
                        favoriteIcon.classList.remove('fa-regular');
                        favoriteIcon.classList.add('fa-solid');
                        favoriteBtn.classList.add('active');
                        favoriteText.textContent = 'Favorited';
                    } else {
                        favoriteIcon.classList.remove('fa-solid');
                        favoriteIcon.classList.add('fa-regular');
                        favoriteBtn.classList.remove('active');
                        favoriteText.textContent = 'Favorite';
                    }

                    // Hiển thị thông báo
                    Swal.fire({
                        icon: 'success',
                        title: data.message,
                        timer: 1500,
                        showConfirmButton: false
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: data.message || 'An error occurred'
                    });
                }
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred. Please try again.'
                });
            });
    }

    // Like Toggle Logic
    const likeBtn = document.getElementById('likeBtn');
    const likeIcon = document.getElementById('likeIcon');
    const likeText = document.getElementById('likeText');
    const likeCount = document.getElementById('likeCount');

    if (likeBtn) {
        likeBtn.addEventListener('click', function () {
            const documentId = this.getAttribute('data-document-id');
            toggleLike(documentId);
        });
    }

    function toggleLike(documentId) {
        const url = window.toggleLikeUrl;

        if (!url) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Configuration error. Please refresh the page.'
            });
            return;
        }

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: new URLSearchParams({
                'id': documentId,
                '__RequestVerificationToken': token || ''
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    if (typeof data.likeCount !== 'undefined' && likeCount) {
                        likeCount.textContent = data.likeCount;
                    }

                    if (data.isLiked) {
                        likeBtn?.classList.add('active');
                        likeIcon?.classList.remove('fa-regular');
                        likeIcon?.classList.add('fa-solid');
                        if (likeText) likeText.textContent = 'Liked';
                    } else {
                        likeBtn?.classList.remove('active');
                        likeIcon?.classList.remove('fa-solid');
                        likeIcon?.classList.add('fa-regular');
                        if (likeText) likeText.textContent = 'Like';
                    }
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: data.message || 'An error occurred'
                    });
                }
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred. Please try again.'
                });
            });
    }
});

// AI Tools JavaScript for Document Detail
(function () {
    let documentFile = null;
    let documentId = null;
    let currentTypingId = null;

    const aiChatContainer = document.getElementById('ai-chat-container');
    const aiQuestionInput = document.getElementById('ai-question-input');
    const aiSendBtn = document.getElementById('ai-send-btn');
    const aiQuickPrompts = document.querySelectorAll('.ai-quick-prompt');

    // Get document ID from the page (you'll need to add this as a data attribute)
    documentId = document.querySelector('[data-document-id]')?.getAttribute('data-document-id');

    // When modal is shown, load the document
    const aiModal = document.getElementById('aiToolsModal');
    aiModal?.addEventListener('shown.bs.modal', async function () {
        if (!documentFile && documentId) {
            await loadDocument();
        }
        aiQuestionInput?.focus();
    });

    // Load document file
    async function loadDocument() {
        try {
            const response = await fetch(`/Document/GetDocumentFileForAI?id=${documentId}`);

            if (!response.ok) {
                throw new Error('Failed to load document');
            }

            const blob = await response.blob();
            const filename = response.headers.get('content-disposition')?.split('filename=')[1]?.replace(/"/g, '') || 'document.pdf';

            documentFile = new File([blob], filename, { type: blob.type });

            console.log('Document loaded successfully:', documentFile.name);
        } catch (error) {
            console.error('Error loading document:', error);
            addAIMessage('Sorry, I could not load the document. Please try again.');
        }
    }

    // Send question
    async function sendQuestion(question) {
        if (!question.trim()) return;

        // Add user message
        addUserMessage(question);
        aiQuestionInput.value = '';

        // Show typing indicator
        currentTypingId = showTypingIndicator();

        try {
            const formData = new FormData();
            formData.append('question', question);

            if (documentFile) {
                formData.append('file', documentFile);
            }

            const response = await fetch('/AIAssistant/AskWithFile', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            const data = await response.json();

            // Remove typing indicator
            if (currentTypingId) {
                removeTypingIndicator(currentTypingId);
                currentTypingId = null;
            }

            if (data.error) {
                addAIMessage(`Sorry, I encountered an error: ${data.error}`);
            } else if (data.answer) {
                addAIMessage(data.answer);
            }
        } catch (error) {
            console.error('Error:', error);
            if (currentTypingId) {
                removeTypingIndicator(currentTypingId);
                currentTypingId = null;
            }
            addAIMessage('Sorry, I encountered an error processing your request.');
        }
    }

    // Add user message to chat
    function addUserMessage(text) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'message user-message mb-3';
        messageDiv.innerHTML = `
            <div class="d-flex align-items-start justify-content-end">
                <div class="message-content bg-secondary text-white p-3 rounded shadow-sm" style="max-width: 80%;">
                    <p class="mb-0">${escapeHtml(text)}</p>
                </div>
                <div class="avatar bg-secondary text-white rounded-circle ms-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px; min-width: 40px;">
                    <i class="bi bi-person-fill"></i>
                </div>
            </div>
        `;
        aiChatContainer.appendChild(messageDiv);
        aiChatContainer.scrollTop = aiChatContainer.scrollHeight;
    }

    // Add AI message to chat
    function addAIMessage(text) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'message ai-message mb-3';
        messageDiv.innerHTML = `
            <div class="d-flex align-items-start">
                <div class="avatar bg-secondary text-white rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px; min-width: 40px;">
                    <i class="bi bi-robot"></i>
                </div>
                <div class="message-content bg-white p-3 rounded shadow-sm" style="max-width: 80%;">
                    ${formatMessage(text)}
                </div>
            </div>
        `;
        aiChatContainer.appendChild(messageDiv);
        aiChatContainer.scrollTop = aiChatContainer.scrollHeight;
    }

    // Show typing indicator
    function showTypingIndicator() {
        const typingDiv = document.createElement('div');
        typingDiv.className = 'message ai-message mb-3';
        typingDiv.id = 'typing-indicator-' + Date.now();
        typingDiv.innerHTML = `
            <div class="d-flex align-items-start">
                <div class="avatar bg-secondary text-white rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px; min-width: 40px;">
                    <i class="bi bi-robot"></i>
                </div>
                <div class="message-content bg-white p-3 rounded shadow-sm">
                    <div class="typing-indicator">
                        <span></span>
                        <span></span>
                        <span></span>
                    </div>
                </div>
            </div>
        `;
        aiChatContainer.appendChild(typingDiv);
        aiChatContainer.scrollTop = aiChatContainer.scrollHeight;
        return typingDiv.id;
    }

    // Remove typing indicator
    function removeTypingIndicator(id) {
        const indicator = document.getElementById(id);
        if (indicator) indicator.remove();
    }

    // Format message with markdown-like syntax
    function formatMessage(text) {
        let formatted = escapeHtml(text);

        // Convert Markdown links
        formatted = formatted.replace(/\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/g, '<a href="$2" target="_blank" class="text-decoration-none text-primary">$1</a>');

        // Paragraphs and line breaks
        formatted = formatted.replace(/\n\n/g, '</p><p class="mb-2">');
        formatted = formatted.replace(/\n/g, '<br>');

        // Bold text
        formatted = formatted.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

        // Handle bullet lists
        const lines = formatted.split('<br>');
        let inList = false;
        let result = [];

        for (let line of lines) {
            if (line.trim().match(/^[-•*]\s+/)) {
                if (!inList) {
                    result.push('<ul class="mb-2 mt-2">');
                    inList = true;
                }
                result.push('<li>' + line.replace(/^[-•*]\s+/, '') + '</li>');
            } else {
                if (inList) {
                    result.push('</ul>');
                    inList = false;
                }
                result.push(line);
            }
        }

        if (inList) result.push('</ul>');
        formatted = result.join('<br>');

        return `<div>${formatted}</div>`;
    }

    // Escape HTML
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Event listeners
    aiSendBtn?.addEventListener('click', () => {
        const question = aiQuestionInput.value.trim();
        if (question) {
            sendQuestion(question);
        }
    });

    aiQuestionInput?.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            const question = aiQuestionInput.value.trim();
            if (question) {
                sendQuestion(question);
            }
        }
    });

    aiQuickPrompts?.forEach(btn => {
        btn.addEventListener('click', () => {
            const prompt = btn.getAttribute('data-prompt');
            aiQuestionInput.value = prompt;
            aiQuestionInput.focus();
        });
    });
})();

// Admin Approve/Deny Document Logic
document.addEventListener('DOMContentLoaded', function () {
    // Get modal elements
    const approveModal = document.getElementById('approveModal');
    const denyModal = document.getElementById('denyModal');
    const approveForm = document.getElementById('approveForm');
    const denyForm = document.getElementById('denyForm');
    const confirmApproveBtn = document.getElementById('confirmApproveBtn');
    const confirmDenyBtn = document.getElementById('confirmDenyBtn');
    
    // Get URLs from window (set in view)
    const approveUrl = window.approveDocumentUrl;
    const rejectUrl = window.rejectDocumentUrl;
    const adminManagementUrl = window.adminManagementUrl;

    // Handle Approve Modal Show
    if (approveModal) {
        approveModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const documentId = button.getAttribute('data-document-id');
            
            // Set document ID in form
            const approveDocumentIdInput = document.getElementById('approveDocumentId');
            if (approveDocumentIdInput) {
                approveDocumentIdInput.value = documentId;
            }
            
            // Reset form
            if (approveForm) {
                approveForm.reset();
                // Remove validation classes
                const pointsInput = document.getElementById('pointsAwarded');
                if (pointsInput) {
                    pointsInput.classList.remove('is-invalid', 'is-valid');
                }
            }
        });
    }

    // Handle Deny Modal Show
    if (denyModal) {
        denyModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const documentId = button.getAttribute('data-document-id');
            
            // Set document ID in form
            const denyDocumentIdInput = document.getElementById('denyDocumentId');
            if (denyDocumentIdInput) {
                denyDocumentIdInput.value = documentId;
            }
            
            // Reset form
            if (denyForm) {
                denyForm.reset();
                // Remove validation classes
                const reasonTextarea = document.getElementById('rejectionReason');
                if (reasonTextarea) {
                    reasonTextarea.classList.remove('is-invalid', 'is-valid');
                }
            }
        });
    }

    // Validate Approve Form
    function validateApproveForm() {
        const pointsInput = document.getElementById('pointsAwarded');
        let isValid = true;

        // Validate points
        if (pointsInput) {
            const points = parseInt(pointsInput.value);
            if (isNaN(points) || points < 0 || points > 1000) {
                pointsInput.classList.add('is-invalid');
                pointsInput.classList.remove('is-valid');
                isValid = false;
            } else {
                pointsInput.classList.remove('is-invalid');
                pointsInput.classList.add('is-valid');
            }
        }

        return isValid;
    }

    // Validate Deny Form
    function validateDenyForm() {
        const reasonTextarea = document.getElementById('rejectionReason');
        let isValid = true;

        // Validate rejection reason
        if (reasonTextarea) {
            const reason = reasonTextarea.value.trim();
            if (!reason || reason.length === 0) {
                reasonTextarea.classList.add('is-invalid');
                reasonTextarea.classList.remove('is-valid');
                isValid = false;
            } else if (reason.length > 500) {
                reasonTextarea.classList.add('is-invalid');
                reasonTextarea.classList.remove('is-valid');
                isValid = false;
            } else {
                reasonTextarea.classList.remove('is-invalid');
                reasonTextarea.classList.add('is-valid');
            }
        }

        return isValid;
    }

    // Real-time validation for points input
    const pointsInput = document.getElementById('pointsAwarded');
    if (pointsInput) {
        pointsInput.addEventListener('input', function () {
            validateApproveForm();
        });

        pointsInput.addEventListener('blur', function () {
            validateApproveForm();
        });
    }

    // Real-time validation for rejection reason
    const reasonTextarea = document.getElementById('rejectionReason');
    if (reasonTextarea) {
        reasonTextarea.addEventListener('input', function () {
            validateDenyForm();
        });

        reasonTextarea.addEventListener('blur', function () {
            validateDenyForm();
        });
    }

    // Handle Approve Form Submit
    if (confirmApproveBtn) {
        confirmApproveBtn.addEventListener('click', function (e) {
            e.preventDefault();

            if (!validateApproveForm()) {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi validation',
                    text: 'Vui lòng kiểm tra lại các trường đã nhập'
                });
                return;
            }

            // Get form data
            const documentId = document.getElementById('approveDocumentId').value;
            const isPremiumOnly = document.getElementById('isVipDocument').checked;
            const pointsAwarded = parseInt(document.getElementById('pointsAwarded').value) || 0;

            // Get anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // Show loading
            Swal.fire({
                title: 'Đang xử lý...',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Submit form data
            const formData = new URLSearchParams({
                'id': documentId,
                'isPremiumOnly': isPremiumOnly,
                'pointsAwarded': pointsAwarded,
                '__RequestVerificationToken': token || ''
            });

            fetch(approveUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: formData,
                redirect: 'follow'
            })
                .then(response => {
                    // Close modal
                    const modalElement = document.getElementById('approveModal');
                    if (modalElement) {
                        const modal = bootstrap.Modal.getInstance(modalElement);
                        if (modal) {
                            modal.hide();
                        }
                    }
                    // Redirect to admin management page
                    window.location.href = adminManagementUrl;
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Có lỗi xảy ra. Vui lòng thử lại.'
                    });
                });
        });
    }

    // Handle Deny Form Submit
    if (confirmDenyBtn) {
        confirmDenyBtn.addEventListener('click', function (e) {
            e.preventDefault();

            if (!validateDenyForm()) {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi validation',
                    text: 'Vui lòng nhập lý do từ chối (tối đa 500 ký tự)'
                });
                return;
            }

            // Get form data
            const documentId = document.getElementById('denyDocumentId').value;
            const rejectionReason = document.getElementById('rejectionReason').value.trim();

            // Get anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // Show loading
            Swal.fire({
                title: 'Đang xử lý...',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Submit form data
            const formData = new URLSearchParams({
                'id': documentId,
                'rejectionReason': rejectionReason,
                '__RequestVerificationToken': token || ''
            });

            fetch(rejectUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: formData,
                redirect: 'follow'
            })
                .then(response => {
                    // Close modal
                    const modalElement = document.getElementById('denyModal');
                    if (modalElement) {
                        const modal = bootstrap.Modal.getInstance(modalElement);
                        if (modal) {
                            modal.hide();
                        }
                    }
                    // Redirect to admin management page
                    window.location.href = adminManagementUrl;
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Có lỗi xảy ra. Vui lòng thử lại.'
                    });
                });
        });
    }
});


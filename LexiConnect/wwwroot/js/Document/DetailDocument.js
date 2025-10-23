//// Enhanced document viewer scripts
//function switchViewer(type) {
//    // Handle PDF viewer switching
//    const embedViewer = document.getElementById('embedViewer');
//    const pdfjsViewer = document.getElementById('pdfjsViewer');
//    const embedBtn = document.getElementById('embedBtn');
//    const pdfjsBtn = document.getElementById('pdfjsBtn');

//    if (type === 'embed') {
//        embedViewer.style.display = 'block';
//        pdfjsViewer.style.display = 'none';
//        embedBtn.classList.add('active');
//        pdfjsBtn.classList.remove('active');
//    } else {
//        embedViewer.style.display = 'none';
//        pdfjsViewer.style.display = 'block';
//        embedBtn.classList.remove('active');
//        pdfjsBtn.classList.add('active');
//    }
//}

//function switchDocViewer(type) {
//    // Handle Word document viewer switching
//    const officeViewer = document.getElementById('officeViewer');
//    const googleViewer = document.getElementById('googleViewer');
//    const docError = document.getElementById('docError');
//    const officeBtn = document.getElementById('officeBtn');
//    const googleBtn = document.getElementById('googleBtn');

//    // Hide error div
//    if (docError) docError.style.display = 'none';

//    if (type === 'office') {
//        officeViewer.style.display = 'block';
//        googleViewer.style.display = 'none';
//        officeBtn.classList.add('active');
//        googleBtn.classList.remove('active');
//    } else {
//        officeViewer.style.display = 'none';
//        googleViewer.style.display = 'block';
//        officeBtn.classList.remove('active');
//        googleBtn.classList.add('active');
//    }
//}

//function handleIframeLoad(iframe) {
//    console.log('Iframe loaded successfully');
//    // You can add loading state management here
//    hideLoadingSpinner();
//}

//function handleIframeError(iframe) {
//    console.error('Iframe failed to load');
//    showDocumentError();
//}

//function showDocumentError() {
//    const officeViewer = document.getElementById('officeViewer');
//    const googleViewer = document.getElementById('googleViewer');
//    const docError = document.getElementById('docError');

//    if (officeViewer) officeViewer.style.display = 'none';
//    if (googleViewer) googleViewer.style.display = 'none';
//    if (docError) docError.style.display = 'block';
//}

//function retryViewer() {
//    // Try switching to Google Docs viewer as fallback
//    switchDocViewer('google');

//    // If that fails too, show download option
//    setTimeout(() => {
//        const googleIframe = document.querySelector('#googleViewer iframe');
//        if (googleIframe) {
//            googleIframe.onload = () => console.log('Google viewer loaded');
//            googleIframe.onerror = () => {
//                alert('Unable to load document preview. Please try downloading the file.');
//            };
//        }
//    }, 1000);
//}

//function openFullscreen() {
//    const modal = document.getElementById('fullscreenModal');
//    const content = document.getElementById('fullscreenContent');

//    // Clone current active viewer
//    let activeViewer = document.querySelector('.viewer-container.active, .viewer-container[style*="block"]');
//    if (!activeViewer) {
//        activeViewer = document.querySelector('.viewer-container');
//    }

//    if (activeViewer) {
//        content.innerHTML = activeViewer.innerHTML;
//        modal.style.display = 'flex';

//        // Adjust iframe size for fullscreen
//        const iframe = content.querySelector('iframe, embed');
//        if (iframe) {
//            iframe.style.height = 'calc(100vh - 60px)';
//        }
//    }
//}

//function closeFullscreen() {
//    const modal = document.getElementById('fullscreenModal');
//    modal.style.display = 'none';
//}

//function hideLoadingSpinner() {
//    const spinners = document.querySelectorAll('.loading-spinner');
//    spinners.forEach(spinner => spinner.style.display = 'none');
//}

//// Load text content for text files
//function loadTextContent(documentId) {
//    fetch(`/Document/ViewFile/${documentId}`)
//        .then(response => response.text())
//        .then(text => {
//            const textContent = document.getElementById('textContent');
//            if (textContent) {
//                textContent.innerHTML = `<pre>${escapeHtml(text)}</pre>`;
//            }
//        })
//        .catch(error => {
//            const textContent = document.getElementById('textContent');
//            if (textContent) {
//                textContent.innerHTML = '<div class="error">Failed to load text content.</div>';
//            }
//        });
//}

//function escapeHtml(unsafe) {
//    return unsafe
//        .replace(/&/g, "&amp;")
//        .replace(/</g, "&lt;")
//        .replace(/>/g, "&gt;")
//        .replace(/"/g, "&quot;")
//        .replace(/'/g, "&#039;");
//}

//// Initialize viewer based on file type
//document.addEventListener('DOMContentLoaded', function () {
//    // Auto-resize iframe based on window size
//    function resizeViewers() {
//        const iframes = document.querySelectorAll('iframe, embed');
//        const minHeight = Math.max(600, window.innerHeight - 300);
//        iframes.forEach(iframe => {
//            if (!iframe.closest('.fullscreen-modal')) {
//                iframe.style.height = minHeight + 'px';
//            }
//        });
//    }

//    // Initial resize
//    resizeViewers();

//    // Resize on window resize
//    window.addEventListener('resize', resizeViewers);

//    // Load text content if it's a text file
//    const textViewer = document.querySelector('.text-viewer');
//    if (textViewer) {
//        const documentId = @Model.Document.DocumentId;
//        loadTextContent(documentId);
//    }

//    // Handle iframe load errors
//    const iframes = document.querySelectorAll('iframe');
//    iframes.forEach(iframe => {
//        iframe.addEventListener('error', function () {
//            console.error('Iframe error:', this.src);
//            // Try alternative viewer or show error
//            if (this.closest('#officeViewer')) {
//                setTimeout(() => switchDocViewer('google'), 2000);
//            }
//        });
//    });

//    // Close fullscreen with Escape key
//    document.addEventListener('keydown', function (e) {
//        if (e.key === 'Escape') {
//            closeFullscreen();
//        }
//    });
//});

//// Existing functions
//function downloadDocument(documentId) {
//    const form = document.createElement('form');
//    form.method = 'POST';
//    form.action = '@Url.Action("Download", "Document")';

//    const input = document.createElement('input');
//    input.type = 'hidden';
//    input.name = 'id';
//    input.value = documentId;
//    form.appendChild(input);

//    const token = document.createElement('input');
//    token.type = 'hidden';
//    token.name = '__RequestVerificationToken';
//    token.value = document.querySelector('input[name="__RequestVerificationToken"]').value;
//    form.appendChild(token);

//    document.body.appendChild(form);
//    form.submit();
//    document.body.removeChild(form);
//}

//function toggleLike(documentId, isLike) {
//    fetch('@Url.Action("ToggleLike", "Document")', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json',
//            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
//        },
//        body: JSON.stringify({ id: documentId, isLike: isLike })
//    })
//        .then(response => response.json())
//        .then(data => {
//            if (data.success) {
//                document.getElementById('likeCount').textContent = data.likeCount;
//            } else {
//                alert(data.message);
//            }
//        })
//        .catch(error => {
//            console.error('Error:', error);
//            alert('An error occurred. Please try again.');
//        });
//}

document.addEventListener("DOMContentLoaded", function () {
    const url = window.pdfFileUrl;
    const pdfjsLib = window['pdfjsLib'];
    let container = document.getElementById("pdf-container");

    if (!pdfjsLib) {
        console.error("PDF.js library not loaded");
        return;
    }

    pdfjsLib.getDocument(url).promise.then(function (pdf) {
        console.log("PDF loaded with", pdf.numPages, "pages");

        // Chỉ hiển thị 2 trang đầu tiên
        const pagesToShow = 2;
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

        // Hiển thị thông báo nếu có nhiều hơn 2 trang
        if (pdf.numPages > pagesToShow) {
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
            message.innerHTML = `🔒  ${pdf.numPages - pagesToShow} page(s) more`;

            let subMessage = document.createElement("div");
            subMessage.style.color = "#636e72";
            subMessage.style.fontSize = "14px";
            subMessage.innerHTML = "Sign up Preminum to view all the material";

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

//document.addEventListener("DOMContentLoaded", function () {
//    const url = window.pdfFileUrl;
//    const pdfjsLib = window['pdfjsLib'];
//    let container = document.getElementById("pdf-container");

//    if (!pdfjsLib) {
//        console.error("PDF.js library not loaded");
//        return;
//    }

//    pdfjsLib.getDocument(url).promise.then(function (pdf) {
//        console.log("PDF loaded", pdf.numPages);

//        let totalPages = Math.min(pdf.numPages, 5);

//        for (let pageNum = 1; pageNum <= totalPages; pageNum++) {
//            pdf.getPage(pageNum).then(function (page) {
//                let scale = 1.2;
//                let viewport = page.getViewport({ scale: scale });

//                let canvas = document.createElement("canvas");
//                canvas.style.display = "block";
//                canvas.style.margin = "0 auto 20px auto";
//                let context = canvas.getContext("2d");
//                canvas.height = viewport.height;
//                canvas.width = viewport.width;

//                container.appendChild(canvas);

//                let renderContext = {
//                    canvasContext: context,
//                    viewport: viewport
//                };
//                page.render(renderContext);
//            });
//        }

//        if (pdf.numPages > totalPages) {
//            let more = document.createElement("div");
//            more.style.color = "red";
//            more.style.textAlign = "center";
//            more.style.fontWeight = "bold";
//            more.innerText = "Login or upgrade to view all pages.";
//            container.appendChild(more);
//        }
//    }).catch(err => {
//        console.error("PDF.js error:", err);
//    });
//});

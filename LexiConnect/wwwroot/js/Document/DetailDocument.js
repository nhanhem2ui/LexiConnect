// Enhanced document viewer scripts
function switchViewer(type) {
    // Handle PDF viewer switching
    const embedViewer = document.getElementById('embedViewer');
    const pdfjsViewer = document.getElementById('pdfjsViewer');
    const embedBtn = document.getElementById('embedBtn');
    const pdfjsBtn = document.getElementById('pdfjsBtn');

    if (type === 'embed') {
        embedViewer.style.display = 'block';
        pdfjsViewer.style.display = 'none';
        embedBtn.classList.add('active');
        pdfjsBtn.classList.remove('active');
    } else {
        embedViewer.style.display = 'none';
        pdfjsViewer.style.display = 'block';
        embedBtn.classList.remove('active');
        pdfjsBtn.classList.add('active');
    }
}

function switchDocViewer(type) {
    // Handle Word document viewer switching
    const officeViewer = document.getElementById('officeViewer');
    const googleViewer = document.getElementById('googleViewer');
    const docError = document.getElementById('docError');
    const officeBtn = document.getElementById('officeBtn');
    const googleBtn = document.getElementById('googleBtn');

    // Hide error div
    if (docError) docError.style.display = 'none';

    if (type === 'office') {
        officeViewer.style.display = 'block';
        googleViewer.style.display = 'none';
        officeBtn.classList.add('active');
        googleBtn.classList.remove('active');
    } else {
        officeViewer.style.display = 'none';
        googleViewer.style.display = 'block';
        officeBtn.classList.remove('active');
        googleBtn.classList.add('active');
    }
}

function handleIframeLoad(iframe) {
    console.log('Iframe loaded successfully');
    // You can add loading state management here
    hideLoadingSpinner();
}

function handleIframeError(iframe) {
    console.error('Iframe failed to load');
    showDocumentError();
}

function showDocumentError() {
    const officeViewer = document.getElementById('officeViewer');
    const googleViewer = document.getElementById('googleViewer');
    const docError = document.getElementById('docError');

    if (officeViewer) officeViewer.style.display = 'none';
    if (googleViewer) googleViewer.style.display = 'none';
    if (docError) docError.style.display = 'block';
}

function retryViewer() {
    // Try switching to Google Docs viewer as fallback
    switchDocViewer('google');

    // If that fails too, show download option
    setTimeout(() => {
        const googleIframe = document.querySelector('#googleViewer iframe');
        if (googleIframe) {
            googleIframe.onload = () => console.log('Google viewer loaded');
            googleIframe.onerror = () => {
                alert('Unable to load document preview. Please try downloading the file.');
            };
        }
    }, 1000);
}

function openFullscreen() {
    const modal = document.getElementById('fullscreenModal');
    const content = document.getElementById('fullscreenContent');

    // Clone current active viewer
    let activeViewer = document.querySelector('.viewer-container.active, .viewer-container[style*="block"]');
    if (!activeViewer) {
        activeViewer = document.querySelector('.viewer-container');
    }

    if (activeViewer) {
        content.innerHTML = activeViewer.innerHTML;
        modal.style.display = 'flex';

        // Adjust iframe size for fullscreen
        const iframe = content.querySelector('iframe, embed');
        if (iframe) {
            iframe.style.height = 'calc(100vh - 60px)';
        }
    }
}

function closeFullscreen() {
    const modal = document.getElementById('fullscreenModal');
    modal.style.display = 'none';
}

function hideLoadingSpinner() {
    const spinners = document.querySelectorAll('.loading-spinner');
    spinners.forEach(spinner => spinner.style.display = 'none');
}

// Load text content for text files
function loadTextContent(documentId) {
    fetch(`/Document/ViewFile/${documentId}`)
        .then(response => response.text())
        .then(text => {
            const textContent = document.getElementById('textContent');
            if (textContent) {
                textContent.innerHTML = `<pre>${escapeHtml(text)}</pre>`;
            }
        })
        .catch(error => {
            const textContent = document.getElementById('textContent');
            if (textContent) {
                textContent.innerHTML = '<div class="error">Failed to load text content.</div>';
            }
        });
}

function escapeHtml(unsafe) {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// Initialize viewer based on file type
document.addEventListener('DOMContentLoaded', function () {
    // Auto-resize iframe based on window size
    function resizeViewers() {
        const iframes = document.querySelectorAll('iframe, embed');
        const minHeight = Math.max(600, window.innerHeight - 300);
        iframes.forEach(iframe => {
            if (!iframe.closest('.fullscreen-modal')) {
                iframe.style.height = minHeight + 'px';
            }
        });
    }

    // Initial resize
    resizeViewers();

    // Resize on window resize
    window.addEventListener('resize', resizeViewers);

    // Load text content if it's a text file
    const textViewer = document.querySelector('.text-viewer');
    if (textViewer) {
        const documentId = @Model.Document.DocumentId;
        loadTextContent(documentId);
    }

    // Handle iframe load errors
    const iframes = document.querySelectorAll('iframe');
    iframes.forEach(iframe => {
        iframe.addEventListener('error', function () {
            console.error('Iframe error:', this.src);
            // Try alternative viewer or show error
            if (this.closest('#officeViewer')) {
                setTimeout(() => switchDocViewer('google'), 2000);
            }
        });
    });

    // Close fullscreen with Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeFullscreen();
        }
    });
});

// Existing functions
function downloadDocument(documentId) {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '@Url.Action("Download", "Document")';

    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = 'id';
    input.value = documentId;
    form.appendChild(input);

    const token = document.createElement('input');
    token.type = 'hidden';
    token.name = '__RequestVerificationToken';
    token.value = document.querySelector('input[name="__RequestVerificationToken"]').value;
    form.appendChild(token);

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
}

function toggleLike(documentId, isLike) {
    fetch('@Url.Action("ToggleLike", "Document")', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ id: documentId, isLike: isLike })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById('likeCount').textContent = data.likeCount;
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred. Please try again.');
        });
}
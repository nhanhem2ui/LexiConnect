

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
});



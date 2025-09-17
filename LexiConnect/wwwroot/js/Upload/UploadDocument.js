(function () {
    const uploadArea = document.getElementById("uploadArea");
    const fileInput = document.getElementById("fileInput");
    const uploadedFiles = document.getElementById("uploadedFiles");

    // Danh sách file người dùng đã chọn
    let filesList = [];

    // Xử lý khi kéo thả file
    uploadArea.addEventListener("dragover", function (e) {
        e.preventDefault();
        uploadArea.classList.add("dragover");
    });

    uploadArea.addEventListener("dragleave", function () {
        uploadArea.classList.remove("dragover");
    });

    uploadArea.addEventListener("drop", function (e) {
        e.preventDefault();
        uploadArea.classList.remove("dragover");
        handleFiles(e.dataTransfer.files);
    });

    // Xử lý khi chọn file từ input
    fileInput.addEventListener("change", function (e) {
        handleFiles(e.target.files);
    });

    // Hàm xử lý file mới
    function handleFiles(newFiles) {
        Array.from(newFiles).forEach(file => {
            // kiểm tra trùng tên file
            if (!filesList.some(f => f.name === file.name && f.size === file.size)) {
                filesList.push(file);
                renderFileItem(file);
            }
        });
    }

    // Render file ra danh sách
    function renderFileItem(file) {
        const fileItem = document.createElement("div");
        fileItem.className = "upload-file-item";

        const fileName = document.createElement("span");
        fileName.className = "upload-file-name";
        fileName.textContent = file.name;

        const fileSize = document.createElement("span");
        fileSize.className = "upload-file-size";
        fileSize.textContent = (file.size / 1024).toFixed(1) + " KB";

        const removeBtn = document.createElement("span");
        removeBtn.className = "upload-remove-file";
        removeBtn.textContent = "✖";
        removeBtn.addEventListener("click", function () {
            filesList = filesList.filter(f => f !== file);
            fileItem.remove();
        });

        fileItem.appendChild(fileName);
        fileItem.appendChild(fileSize);
        fileItem.appendChild(removeBtn);

        uploadedFiles.appendChild(fileItem);
    }
})();

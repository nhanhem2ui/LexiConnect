(function () {
    // ✅ DETECT MULTIPLE INSTANCES
    if (window.uploadScriptInstances) {
        console.error("❌ UPLOAD SCRIPT LOADED MULTIPLE TIMES!");
        console.error(`This is instance #${++window.uploadScriptInstances}`);
        console.error("This will cause filesList conflicts!");
        return; // Exit early to prevent multiple instances
    } else {
        window.uploadScriptInstances = 1;
        console.log("✅ First upload script instance loaded");
    }

    const uploadArea = document.getElementById("uploadArea");
    const fileInput = document.getElementById("fileInput");
    const uploadedFiles = document.getElementById("uploadedFiles");
    const form = document.querySelector('form');

    // ✅ CHECK REQUIRED ELEMENTS
    if (!uploadArea) {
        console.error("❌ uploadArea element not found!");
        return;
    }
    if (!fileInput) {
        console.error("❌ fileInput element not found!");
        return;
    }
    if (!uploadedFiles) {
        console.error("❌ uploadedFiles element not found!");
        return;
    }
    if (!form) {
        console.error("❌ form element not found!");
        return;
    }

    console.log("✅ All required elements found");

    // Danh sách file người dùng đã chọn
    let filesList = [];

    // ✅ MAKE FILELIST GLOBALLY ACCESSIBLE FOR DEBUGGING
    window.currentFilesList = filesList; // This will be updated as reference

    // DEBUG: Function to log filesList contents
    //function debugFilesList(action = "Current state") {
    //    console.log(`=== ${action} ===`);
    //    console.log(`Total files: ${filesList.length}`);
    //    console.log(`Global filesList reference: ${window.currentFilesList.length}`);
    //    filesList.forEach((file, index) => {
    //        console.log(`[${index}] Name: ${file.name}, Size: ${formatFileSize(file.size)}, Type: ${file.type}`);
    //    });
    //    console.log("Raw filesList:", filesList);
    //    console.log("===========================");
    //}

    // DEBUG: Add button to manually check filesList
    //function addDebugButton() {
    //    // Remove existing debug button if any
    //    const existing = document.querySelector('.debug-files-btn');
    //    if (existing) {
    //        existing.remove();
    //    }

    //    const debugBtn = document.createElement("button");
    //    debugBtn.type = "button";
    //    debugBtn.className = "debug-files-btn";
    //    debugBtn.textContent = "Debug Files List";
    //    debugBtn.style.margin = "10px";
    //    debugBtn.style.padding = "10px";
    //    debugBtn.style.backgroundColor = "#dc3545";
    //    debugBtn.style.color = "white";
    //    debugBtn.style.border = "none";
    //    debugBtn.style.borderRadius = "5px";
    //    debugBtn.style.cursor = "pointer";
    //    debugBtn.onclick = () => debugFilesList("Manual Debug Check");

    //    // Insert after upload area
    //    uploadArea.parentNode.insertBefore(debugBtn, uploadArea.nextSibling);
    //    console.log("Debug button added successfully");
    //}

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
    //    e.target.value = '';
    });

    // Process new files
    function handleFiles(newFiles) {
        console.log(`Processing ${newFiles.length} new files...`);

        Array.from(newFiles).forEach(file => {
            console.log(`Checking file: ${file.name}`);

            // Check for duplicate files (same name and size)
            if (!filesList.some(f => f.name === file.name && f.size === file.size)) {
                // Validate file before adding
                if (validateFile(file)) {
                    filesList.push(file);
                    console.log(`✅ File added: ${file.name}`);

                    // ✅ UPDATE GLOBAL REFERENCE
                    window.currentFilesList = filesList;

                    renderFileItem(file);
                //    updateFileInput();
                } else {
                    console.log(`❌ File validation failed: ${file.name}`);
                }
            } else {
                console.log(`❌ Duplicate file: ${file.name}`);
                alert(`File "${file.name}" is already selected.`);
            }

            updateFileInput();
        });

        debugFilesList("After adding files");
    }

    // Validate individual file
    function validateFile(file) {
        const maxSize = 10 * 1024 * 1024; // 10MB
        const allowedTypes = ['.pdf', '.doc', '.docx'];
        const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

        if (file.size === 0) {
            alert(`File "${file.name}" is empty.`);
            return false;
        }

        if (file.size > maxSize) {
            alert(`File "${file.name}" exceeds the maximum size of 10MB.`);
            return false;
        }

        if (!allowedTypes.includes(fileExtension)) {
            alert(`File "${file.name}" has an invalid type. Only PDF, DOC, and DOCX files are allowed.`);
            return false;
        }

        return true;
    }

    // Render file item in the UI
    function renderFileItem(file) {
        const fileItem = document.createElement("div");
        fileItem.className = "upload-file-item";

        const fileName = document.createElement("span");
        fileName.className = "upload-file-name";
        fileName.textContent = file.name;

        const fileSize = document.createElement("span");
        fileSize.className = "upload-file-size";
        fileSize.textContent = formatFileSize(file.size);

        const removeBtn = document.createElement("span");
        removeBtn.className = "upload-remove-file";
        removeBtn.textContent = "✖";
        removeBtn.style.cursor = "pointer";
        removeBtn.addEventListener("click", function () {
            console.log(`Removing file: ${file.name}`);

            //// Remove from filesList
            filesList = filesList.filter(f => f !== file);

            // ✅ UPDATE GLOBAL REFERENCE
            window.currentFilesList = filesList;

            // Remove from DOM
            fileItem.remove();
            // Update file input
            updateFileInput();

            debugFilesList("After removing file");
        });

        fileItem.appendChild(fileName);
        fileItem.appendChild(fileSize);
        fileItem.appendChild(removeBtn);
        uploadedFiles.appendChild(fileItem);
    }

    // Format file size for display
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Update the file input with current files
    function updateFileInput() {
        const dt = new DataTransfer();
        filesList.forEach(file => {
            dt.items.add(file);
        });
        fileInput.files = dt.files;
        console.log(`File input updated. Now has ${fileInput.files.length} files`);
    }

    
    
    // Form submission validation
    form.addEventListener('submit', function (e) {

        console.log("=== FORM SUBMISSION CHECK ===");
        console.log(`filesList.length: ${filesList.length}`);
        console.log(`window.currentFilesList.length: ${window.currentFilesList.length}`);
        console.log(`fileInput.files.length: ${fileInput.files.length}`);

        console.log("👉 [Submit] filesList.length =", filesList.length);
        console.log("👉 [Submit] fileInput.files.length =", fileInput.files.length);
        console.log("👉 [Submit] window.currentFilesList.length =", window.currentFilesList.length);

        debugFilesList("Before form submission");

        //if (filesList.length === 0) {
        //    e.preventDefault();
        //    console.log("❌ Form submission prevented - no files selected");
        //    alert('Please select at least one file to upload.');
        //    return false;
        //}

        console.log(`✅ Submitting form with ${filesList.length} files`);

        // Show loading state
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = 'Uploading...';
        }
    });

    console.log("Form event listener added successfully");

    // Initialize debug features
    function initialize() {
        console.log("Initializing upload script...");
        //addDebugButton();
        debugFilesList("Initial state");

        // Make debug functions globally accessible
        window.debugFiles = {
            list: () => filesList,
            current: () => window.currentFilesList,
            count: () => filesList.length,
            clear: () => {
                filesList = [];
                window.currentFilesList = filesList;
                uploadedFiles.innerHTML = '';
                updateFileInput();
                console.log("FilesList cleared");
                debugFilesList("After clearing");
            },
            log: () => debugFilesList("Console debug"),
            instances: () => window.uploadScriptInstances || 0
        };

        console.log("Debug commands available:");
        console.log("- window.debugFiles.list() - Get current files");
        console.log("- window.debugFiles.current() - Get global reference");
        console.log("- window.debugFiles.count() - Get files count");
        console.log("- window.debugFiles.instances() - Check script instances");
        console.log("- window.debugFiles.log() - Show detailed info");
        console.log("- window.debugFiles.clear() - Clear all files");
    }

    // Initialize based on DOM state
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

    console.log("Upload script initialized successfully");
})();
// FileCleanup.js - Handle file cleanup ONLY on back navigation
(function () {
    console.log("File cleanup script loaded");

    let hasUnsavedChanges = true;
    let isFormSubmitted = false;
    let isPageLoaded = false;

    // Wait for page to be fully loaded
    document.addEventListener('DOMContentLoaded', function () {
        isPageLoaded = true;
        console.log("Page fully loaded, cleanup script ready");

        // Mark current page state
        if (window.history.pushState) {
            const currentState = {
                page: 'details',
                timestamp: Date.now(),
                hasDocuments: true
            };
            window.history.replaceState(currentState, document.title);
            console.log("History state marked");
        }
    });

    // Track if user submitted the form (going forward)
    const form = document.querySelector('form[action*="SaveDetails"]');
    if (form) {
        form.addEventListener('submit', function () {
            console.log("Form submitted - disabling cleanup permanently");
            hasUnsavedChanges = false;
            isFormSubmitted = true;
        });
    }

    // Track Continue button clicks
    const continueButtons = document.querySelectorAll('.doc-next-btn, button[type="submit"]');
    continueButtons.forEach(btn => {
        if (btn) {
            btn.addEventListener('click', function () {
                console.log("Continue button clicked - disabling cleanup permanently");
                hasUnsavedChanges = false;
                isFormSubmitted = true;
            });
        }
    });

    // Enhanced back navigation detection
    let navigationTimer = null;

    // Method 1: popstate event
    window.addEventListener('popstate', function (event) {
        console.log("Popstate event detected - user navigating");
        console.log("Event state:", event.state);

        if (isPageLoaded && hasUnsavedChanges && !isFormSubmitted) {
            console.log("Back navigation with unsaved changes - performing cleanup");
            cleanupTempFiles();
        }
    });

    // Method 2: beforeunload - more reliable
    window.addEventListener('beforeunload', function (e) {
        console.log("Before unload event triggered");
        console.log("hasUnsavedChanges:", hasUnsavedChanges);
        console.log("isFormSubmitted:", isFormSubmitted);
        console.log("isPageLoaded:", isPageLoaded);

        // Only cleanup if conditions are met
        if (isPageLoaded && hasUnsavedChanges && !isFormSubmitted) {
            console.log("Cleanup conditions met - attempting cleanup");
            cleanupTempFiles();

            // Optional: Show confirmation dialog (for testing)
            // e.preventDefault();
            // e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
        } else {
            console.log("Cleanup conditions not met - no cleanup");
        }
    });

    //// Method 3: Alternative for mobile browsers
    //document.addEventListener('visibilitychange', function () {
    //    if (document.visibilityState === 'hidden') {
    //        console.log("Page visibility changed to hidden");

    //        // Use a short delay to distinguish between tab switch and navigation
    //        navigationTimer = setTimeout(() => {
    //            if (isPageLoaded && hasUnsavedChanges && !isFormSubmitted) {
    //                console.log("Page likely navigated away - performing cleanup");
    //                cleanupTempFiles();
    //            }
    //        }, 500);
    //    } else if (document.visibilityState === 'visible') {
    //        // Page became visible again, cancel cleanup
    //        if (navigationTimer) {
    //            clearTimeout(navigationTimer);
    //            navigationTimer = null;
    //            console.log("Page became visible again - cleanup cancelled");
    //        }
    //    }
    //});

    function cleanupTempFiles() {
        console.log("=== CLEANUP FUNCTION CALLED ===");

        const documentIds = getTempDocumentIds();
        console.log("Document IDs to cleanup:", documentIds);

        if (documentIds && documentIds.length > 0) {
            console.log("Starting cleanup process for documents:", documentIds);

            const cleanupData = new FormData();
            cleanupData.append('documentIds', documentIds.join(','));

            // Add anti-forgery token if available
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                cleanupData.append('__RequestVerificationToken', token.value);
                console.log("Anti-forgery token added");
            } else {
                console.warn("Anti-forgery token not found");
            }

            // Method 1: Use sendBeacon (preferred for page unload)
            const beaconSuccess = navigator.sendBeacon('/Upload/CleanupTempFiles', cleanupData);
            console.log("sendBeacon result:", beaconSuccess);

            // Method 2: Fallback with fetch
            if (!beaconSuccess) {
                console.log("sendBeacon failed, trying fetch");

                try {
                    fetch('/Upload/CleanupTempFiles', {
                        method: 'POST',
                        body: cleanupData,
                        keepalive: true
                    }).then(response => {
                        console.log("Fetch cleanup response:", response.status);
                        return response.json();
                    }).then(data => {
                        console.log("Cleanup result:", data);
                    }).catch(error => {
                        console.error("Fetch cleanup failed:", error);
                    });
                } catch (error) {
                    console.error("Fetch cleanup exception:", error);
                }
            }

            // Method 3: Synchronous XMLHttpRequest (last resort)
            if (!beaconSuccess && navigator.sendBeacon === undefined) {
                console.log("Using synchronous XHR as last resort");

                try {
                    const xhr = new XMLHttpRequest();
                    xhr.open('POST', '/Upload/CleanupTempFiles', false); // synchronous
                    xhr.send(cleanupData);
                    console.log("XHR cleanup response:", xhr.status);
                } catch (error) {
                    console.error("XHR cleanup failed:", error);
                }
            }
        } else {
            console.log("No document IDs found for cleanup");
        }
    }

    function getTempDocumentIds() {
        console.log("=== GETTING DOCUMENT IDS ===");

        // Method 1: Try hidden field first
        const hiddenField = document.querySelector('input[name="TempDocumentIds"]');
        if (hiddenField && hiddenField.value) {
            const ids = hiddenField.value.split(',').map(id => parseInt(id)).filter(id => !isNaN(id));
            console.log("Found document IDs in hidden field:", ids);
            return ids;
        } else {
            console.log("Hidden field not found or empty");
        }

        // Method 2: Try to get from document sections
        const docSections = document.querySelectorAll('.doc-section');
        console.log("Found doc sections:", docSections.length);

        const ids = [];
        docSections.forEach((section, index) => {
            const hiddenInput = section.querySelector('input[type="hidden"][name*="DocumentId"]');
            console.log(`Section ${index} hidden input:`, hiddenInput ? hiddenInput.value : 'not found');

            if (hiddenInput && hiddenInput.value) {
                const id = parseInt(hiddenInput.value);
                if (!isNaN(id)) {
                    ids.push(id);
                }
            }
        });

        if (ids.length > 0) {
            console.log("Found document IDs from sections:", ids);
            return ids;
        }

        // Method 3: Check TempData in page source (if available)
        const scripts = document.querySelectorAll('script');
        scripts.forEach(script => {
            if (script.innerHTML && script.innerHTML.includes('DocumentIds')) {
                console.log("Found potential DocumentIds in script:", script.innerHTML.substring(0, 100));
            }
        });

        console.log("No document IDs found anywhere");
        return [];
    }

    // Debug helpers - globally accessible
    window.debugCleanup = {
        hasUnsavedChanges: () => hasUnsavedChanges,
        isFormSubmitted: () => isFormSubmitted,
        isPageLoaded: () => isPageLoaded,
        documentIds: getTempDocumentIds,
        forceCleanup: () => {
            console.log("=== FORCE CLEANUP TRIGGERED ===");
            cleanupTempFiles();
        },
        reset: () => {
            hasUnsavedChanges = true;
            isFormSubmitted = false;
            console.log("Cleanup state reset");
        },
        testBeacon: () => {
            const testData = new FormData();
            testData.append('test', 'data');
            const success = navigator.sendBeacon('/Upload/GetUploadProgress', testData);
            console.log("Test beacon result:", success);
        }
    };

    console.log("=== FILE CLEANUP SCRIPT INITIALIZED ===");
    console.log("Debug commands available:");
    console.log("- window.debugCleanup.forceCleanup() - Force cleanup");
    console.log("- window.debugCleanup.documentIds() - Show document IDs");
    console.log("- window.debugCleanup.testBeacon() - Test sendBeacon");
})();
// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// User Profile Dropdown JavaScript
document.addEventListener('DOMContentLoaded', function () {
    const userProfile = document.getElementById('userProfile');
    const userDropdown = document.getElementById('userDropdown');
    const dropdownOverlay = document.getElementById('dropdownOverlay');

    if (userProfile && userDropdown) {
        // Toggle dropdown
        userProfile.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const isOpen = userDropdown.classList.contains('show');

            if (isOpen) {
                closeDropdown();
            } else {
                openDropdown();
            }
        });

        // Prevent dropdown from closing when clicking inside it
        userDropdown.addEventListener('click', function (e) {
            e.stopPropagation();
        });

        // Close dropdown when clicking overlay
        dropdownOverlay.addEventListener('click', function (e) {
            e.preventDefault();
            closeDropdown();
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            if (!userProfile.contains(e.target)) {
                closeDropdown();
            }
        });

        // Close dropdown on escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeDropdown();
            }
        });

        function openDropdown() {
            userProfile.classList.add('active');
            userDropdown.classList.add('show');
            dropdownOverlay.classList.add('show');
        }

        function closeDropdown() {
            userProfile.classList.remove('active');
            userDropdown.classList.remove('show');
            dropdownOverlay.classList.remove('show');
        }
    }
});
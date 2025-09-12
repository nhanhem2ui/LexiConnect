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
        if (dropdownOverlay) {
            dropdownOverlay.addEventListener('click', function (e) {
                e.preventDefault();
                closeDropdown();
            });
        }

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
            if (dropdownOverlay) {
                dropdownOverlay.classList.add('show');
            }
        }

        function closeDropdown() {
            userProfile.classList.remove('active');
            userDropdown.classList.remove('show');
            if (dropdownOverlay) {
                dropdownOverlay.classList.remove('show');
            }
        }
    }
});

// Enhanced Header Search Functionality - Add this to your layout or header script section
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.querySelector('.search-input');
    const searchBtn = document.querySelector('.search-btn');
    const searchContainer = document.querySelector('.search-container');

    // Only initialize if search elements exist (authenticated users)
    if (!searchInput || !searchBtn || !searchContainer) {
        return;
    }

    // Create suggestions dropdown
    let suggestionsDropdown = document.getElementById('searchSuggestions');
    if (!suggestionsDropdown) {
        suggestionsDropdown = document.createElement('div');
        suggestionsDropdown.className = 'search-suggestions';
        suggestionsDropdown.id = 'searchSuggestions';
        suggestionsDropdown.style.display = 'none';
        searchContainer.appendChild(suggestionsDropdown);
    }

    let debounceTimer;
    let selectedIndex = -1;

    // Perform search function
    function performSearch(query) {
        if (query.trim()) {
            // You can customize this URL based on your search controller/action
            window.location.href = `/Search/Results?query=${encodeURIComponent(query.trim())}`;
        }
    }

    // Search button click
    searchBtn.addEventListener('click', function () {
        performSearch(searchInput.value);
    });

    // Enter key search
    searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const suggestions = suggestionsDropdown.querySelectorAll('.suggestion-item');

            if (selectedIndex >= 0 && suggestions[selectedIndex]) {
                suggestions[selectedIndex].click();
            } else {
                performSearch(this.value);
            }
        }
    });

    // Input changes for suggestions
    searchInput.addEventListener('input', function (e) {
        const query = e.target.value.trim();
        selectedIndex = -1; // Reset selection

        clearTimeout(debounceTimer);

        if (query.length < 2) {
            hideSuggestions();
            return;
        }

        // Show loading state
        showLoading();

        debounceTimer = setTimeout(() => {
            searchForSuggestions(query);
        }, 300);
    });

    // Keyboard navigation
    searchInput.addEventListener('keydown', function (e) {
        const suggestions = suggestionsDropdown.querySelectorAll('.suggestion-item');

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            selectedIndex = Math.min(selectedIndex + 1, suggestions.length - 1);
            updateSelection(suggestions);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            selectedIndex = Math.max(selectedIndex - 1, -1);
            updateSelection(suggestions);
        } else if (e.key === 'Escape') {
            hideSuggestions();
            selectedIndex = -1;
            searchInput.blur();
        }
    });

    // Focus behavior
    searchInput.addEventListener('focus', function () {
        if (this.value.trim().length >= 2) {
            searchForSuggestions(this.value.trim());
        }
    });

    // Click outside to hide
    document.addEventListener('click', function (e) {
        if (!searchContainer.contains(e.target)) {
            hideSuggestions();
        }
    });

    function searchForSuggestions(query) {
        // You can customize this URL based on your suggestions endpoint
        fetch(`/Search/Suggestions?query=${encodeURIComponent(query)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                displaySuggestions(data, query);
            })
            .catch(error => {
                console.error('Search error:', error);
                showError();
            });
    }

    function displaySuggestions(suggestions, query) {
        if (!suggestions || suggestions.length === 0) {
            showEmpty();
            return;
        }

        suggestionsDropdown.innerHTML = '';
        suggestionsDropdown.className = 'search-suggestions';

        suggestions.forEach((item, index) => {
            const suggestionItem = document.createElement('div');
            suggestionItem.className = 'suggestion-item';
            suggestionItem.setAttribute('data-index', index);

            // Create icon based on type
            const icon = getTypeIcon(item.type);

            suggestionItem.innerHTML = `
                <div class="suggestion-content">
                    <div class="suggestion-header">
                        <span class="suggestion-icon">${icon}</span>
                        <span class="suggestion-title">${highlightMatch(item.title, query)}</span>
                    </div>
                    <div class="suggestion-meta">${item.type} • ${item.university || item.meta || ''}</div>
                </div>
            `;

            suggestionItem.addEventListener('click', function () {
                selectSuggestion(item);
            });

            suggestionsDropdown.appendChild(suggestionItem);
        });

        suggestionsDropdown.style.display = 'block';
    }

    function showLoading() {
        suggestionsDropdown.innerHTML = '<div class="suggestion-loading">Searching...</div>';
        suggestionsDropdown.className = 'search-suggestions loading';
        suggestionsDropdown.style.display = 'block';
    }

    function showEmpty() {
        suggestionsDropdown.innerHTML = '<div class="suggestion-empty">No results found</div>';
        suggestionsDropdown.className = 'search-suggestions empty';
        suggestionsDropdown.style.display = 'block';
    }

    function showError() {
        suggestionsDropdown.innerHTML = '<div class="suggestion-error">Search unavailable</div>';
        suggestionsDropdown.className = 'search-suggestions error';
        suggestionsDropdown.style.display = 'block';
    }

    function hideSuggestions() {
        suggestionsDropdown.style.display = 'none';
        selectedIndex = -1;
    }

    function selectSuggestion(item) {
        searchInput.value = item.title;
        hideSuggestions();
        // Navigate to the item's URL or perform search with the item's title
        if (item.url) {
            window.location.href = item.url;
        } else {
            performSearch(item.title);
        }
    }

    function highlightMatch(text, query) {
        if (!query) return text;
        const regex = new RegExp(`(${escapeRegex(query)})`, 'gi');
        return text.replace(regex, '<strong>$1</strong>');
    }

    function escapeRegex(string) {
        return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    function updateSelection(suggestions) {
        suggestions.forEach((item, index) => {
            if (index === selectedIndex) {
                item.classList.add('selected');
                item.scrollIntoView({ block: 'nearest' });
            } else {
                item.classList.remove('selected');
            }
        });
    }

    function getTypeIcon(type) {
        switch (type?.toLowerCase()) {
            case 'document':
            case 'pdf':
            case 'file':
                return '📄';
            case 'course':
            case 'subject':
                return '📚';
            case 'university':
            case 'school':
                return '🏫';
            case 'user':
            case 'student':
                return '👤';
            case 'question':
                return '❓';
            case 'answer':
                return '💡';
            default:
                return '🔍';
        }
    }
});
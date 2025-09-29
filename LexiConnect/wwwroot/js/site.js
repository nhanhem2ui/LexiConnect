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
                return '<svg viewBox="0 0 24 24" width = "24" height = "24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M13 3L13.7071 2.29289C13.5196 2.10536 13.2652 2 13 2V3ZM19 9H20C20 8.73478 19.8946 8.48043 19.7071 8.29289L19 9ZM13.109 8.45399L14 8V8L13.109 8.45399ZM13.546 8.89101L14 8L13.546 8.89101ZM10 13C10 12.4477 9.55228 12 9 12C8.44772 12 8 12.4477 8 13H10ZM8 16C8 16.5523 8.44772 17 9 17C9.55228 17 10 16.5523 10 16H8ZM8.5 9C7.94772 9 7.5 9.44772 7.5 10C7.5 10.5523 7.94772 11 8.5 11V9ZM9.5 11C10.0523 11 10.5 10.5523 10.5 10C10.5 9.44772 10.0523 9 9.5 9V11ZM8.5 6C7.94772 6 7.5 6.44772 7.5 7C7.5 7.55228 7.94772 8 8.5 8V6ZM9.5 8C10.0523 8 10.5 7.55228 10.5 7C10.5 6.44772 10.0523 6 9.5 6V8ZM17.908 20.782L17.454 19.891L17.454 19.891L17.908 20.782ZM18.782 19.908L19.673 20.362L18.782 19.908ZM5.21799 19.908L4.32698 20.362H4.32698L5.21799 19.908ZM6.09202 20.782L6.54601 19.891L6.54601 19.891L6.09202 20.782ZM6.09202 3.21799L5.63803 2.32698L5.63803 2.32698L6.09202 3.21799ZM5.21799 4.09202L4.32698 3.63803L4.32698 3.63803L5.21799 4.09202ZM12 3V7.4H14V3H12ZM14.6 10H19V8H14.6V10ZM12 7.4C12 7.66353 11.9992 7.92131 12.0169 8.13823C12.0356 8.36682 12.0797 8.63656 12.218 8.90798L14 8C14.0293 8.05751 14.0189 8.08028 14.0103 7.97537C14.0008 7.85878 14 7.69653 14 7.4H12ZM14.6 8C14.3035 8 14.1412 7.99922 14.0246 7.9897C13.9197 7.98113 13.9425 7.9707 14 8L13.092 9.78201C13.3634 9.92031 13.6332 9.96438 13.8618 9.98305C14.0787 10.0008 14.3365 10 14.6 10V8ZM12.218 8.90798C12.4097 9.2843 12.7157 9.59027 13.092 9.78201L14 8V8L12.218 8.90798ZM8 13V16H10V13H8ZM8.5 11H9.5V9H8.5V11ZM8.5 8H9.5V6H8.5V8ZM13 2H8.2V4H13V2ZM4 6.2V17.8H6V6.2H4ZM8.2 22H15.8V20H8.2V22ZM20 17.8V9H18V17.8H20ZM19.7071 8.29289L13.7071 2.29289L12.2929 3.70711L18.2929 9.70711L19.7071 8.29289ZM15.8 22C16.3436 22 16.8114 22.0008 17.195 21.9694C17.5904 21.9371 17.9836 21.8658 18.362 21.673L17.454 19.891C17.4045 19.9162 17.3038 19.9539 17.0322 19.9761C16.7488 19.9992 16.3766 20 15.8 20V22ZM18 17.8C18 18.3766 17.9992 18.7488 17.9761 19.0322C17.9539 19.3038 17.9162 19.4045 17.891 19.454L19.673 20.362C19.8658 19.9836 19.9371 19.5904 19.9694 19.195C20.0008 18.8114 20 18.3436 20 17.8H18ZM18.362 21.673C18.9265 21.3854 19.3854 20.9265 19.673 20.362L17.891 19.454C17.7951 19.6422 17.6422 19.7951 17.454 19.891L18.362 21.673ZM4 17.8C4 18.3436 3.99922 18.8114 4.03057 19.195C4.06287 19.5904 4.13419 19.9836 4.32698 20.362L6.10899 19.454C6.0838 19.4045 6.04612 19.3038 6.02393 19.0322C6.00078 18.7488 6 18.3766 6 17.8H4ZM8.2 20C7.62345 20 7.25117 19.9992 6.96784 19.9761C6.69617 19.9539 6.59545 19.9162 6.54601 19.891L5.63803 21.673C6.01641 21.8658 6.40963 21.9371 6.80497 21.9694C7.18864 22.0008 7.65645 22 8.2 22V20ZM4.32698 20.362C4.6146 20.9265 5.07354 21.3854 5.63803 21.673L6.54601 19.891C6.35785 19.7951 6.20487 19.6422 6.10899 19.454L4.32698 20.362ZM8.2 2C7.65645 2 7.18864 1.99922 6.80497 2.03057C6.40963 2.06287 6.01641 2.13419 5.63803 2.32698L6.54601 4.10899C6.59545 4.0838 6.69617 4.04612 6.96784 4.02393C7.25117 4.00078 7.62345 4 8.2 4V2ZM6 6.2C6 5.62345 6.00078 5.25117 6.02393 4.96784C6.04612 4.69617 6.0838 4.59545 6.10899 4.54601L4.32698 3.63803C4.13419 4.01641 4.06287 4.40963 4.03057 4.80497C3.99922 5.18864 4 5.65645 4 6.2H6ZM5.63803 2.32698C5.07354 2.6146 4.6146 3.07354 4.32698 3.63803L6.10899 4.54601C6.20487 4.35785 6.35785 4.20487 6.54601 4.10899L5.63803 2.32698Z" fill="#000000"></path> </g></svg>';
            case 'course':
            case 'subject':
                return '<svg fill="#000000" viewBox="0 0 256 256" width = "24" height = "24" id="Flat" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M233.61523,195.5752,192.2041,41.02637a16.01548,16.01548,0,0,0-19.5957-11.31348l-30.91016,8.28223c-.33935.09094-.66357.20923-.99219.32043A15.96591,15.96591,0,0,0,128,32H96a15.8799,15.8799,0,0,0-8,2.16492A15.8799,15.8799,0,0,0,80,32H48A16.01833,16.01833,0,0,0,32,48V208a16.01833,16.01833,0,0,0,16,16H80a15.8799,15.8799,0,0,0,8-2.16492A15.8799,15.8799,0,0,0,96,224h32a16.01833,16.01833,0,0,0,16-16V108.40283l27.7959,103.73584a15.992,15.992,0,0,0,19.5957,11.31445l30.91016-8.28222A16.01822,16.01822,0,0,0,233.61523,195.5752ZM176.749,45.167l6.21338,23.18238-30.91113,8.28247L145.83984,53.4502ZM128,48l.00732,120H96V48ZM80,48V72H48V48Zm48,160H96V184h32.0083l.00147,24Zm90.16016-8.28418-30.90967,8.28223-6.21143-23.18189,30.918-8.28491,6.21289,23.18164Z"></path> </g></svg>';
            case 'university':
            case 'school':
                return '<svg fill="#000000" viewBox="0 0 32 32" width = "24" height = "24" version="1.1" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <title>school</title> <path d="M30 21.25h-6.25v-8.957l5.877 3.358c0.107 0.062 0.236 0.098 0.373 0.099h0c0.414-0.001 0.749-0.336 0.749-0.751 0-0.277-0.15-0.519-0.373-0.649l-0.004-0.002-13.623-7.784v-0.552c0.172 0.016 0.35 0.068 0.519 0.068 0.004 0 0.010 0 0.015 0 0.475 0 0.934-0.067 1.368-0.193l-0.035 0.009c0.323-0.063 0.693-0.099 1.073-0.099 0.392 0 0.775 0.039 1.146 0.112l-0.037-0.006c0.039 0.007 0.083 0.012 0.129 0.012 0.184 0 0.352-0.068 0.479-0.181l-0.001 0.001c0.161-0.139 0.263-0.343 0.264-0.571v-2.812c0 0 0-0 0-0 0-0.355-0.247-0.653-0.579-0.73l-0.005-0.001c-0.419-0.111-0.9-0.176-1.396-0.176-0.5 0-0.985 0.065-1.446 0.187l0.039-0.009c-0.288 0.067-0.618 0.105-0.958 0.105-0.231 0-0.457-0.018-0.678-0.052l0.025 0.003c-0.122-0.256-0.378-0.43-0.676-0.43-0.412 0-0.746 0.334-0.746 0.746 0 0.001 0 0.003 0 0.004v-0 4.565l-13.622 7.784c-0.227 0.132-0.378 0.374-0.378 0.651 0 0.414 0.336 0.75 0.75 0.75 0.137 0 0.265-0.037 0.376-0.101l-0.004 0.002 5.878-3.359v8.957h-6.25c-0.414 0-0.75 0.336-0.75 0.75v0 8c0 0.414 0.336 0.75 0.75 0.75h28c0.414-0 0.75-0.336 0.75-0.75v0-8c-0-0.414-0.336-0.75-0.75-0.75v0zM18.658 3.075c0.298-0.082 0.64-0.13 0.993-0.13 0.183 0 0.363 0.013 0.539 0.037l-0.020-0.002v1.339c-0.16-0.013-0.345-0.021-0.533-0.021-0.489 0-0.966 0.052-1.425 0.151l0.044-0.008c-0.304 0.088-0.653 0.139-1.014 0.139-0.174 0-0.344-0.012-0.512-0.034l0.020 0.002v-1.323c0.15 0.014 0.325 0.021 0.502 0.021 0.499 0 0.984-0.062 1.447-0.18l-0.041 0.009zM2.75 22.75h5.5v6.5h-5.5zM9.75 22v-10.564l6.25-3.571 6.25 3.572v17.814h-2.5v-5.25c-0-0.414-0.336-0.75-0.75-0.75h-6c-0.414 0-0.75 0.336-0.75 0.75v0 5.25h-2.5zM13.75 29.25v-4.5h4.5v4.5zM29.25 29.25h-5.5v-6.5h5.5zM16 19.75c2.071 0 3.75-1.679 3.75-3.75s-1.679-3.75-3.75-3.75c-2.071 0-3.75 1.679-3.75 3.75v0c0.002 2.070 1.68 3.748 3.75 3.75h0zM16 13.75c1.243 0 2.25 1.007 2.25 2.25s-1.007 2.25-2.25 2.25c-1.243 0-2.25-1.007-2.25-2.25v0c0.002-1.242 1.008-2.248 2.25-2.25h0z"></path> </g></svg>';
            case 'user':
            case 'student':
                return '<svg viewBox="0 0 16 16" width = "24" height = "24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M8 7C9.65685 7 11 5.65685 11 4C11 2.34315 9.65685 1 8 1C6.34315 1 5 2.34315 5 4C5 5.65685 6.34315 7 8 7Z" fill="#000000"></path> <path d="M14 12C14 10.3431 12.6569 9 11 9H5C3.34315 9 2 10.3431 2 12V15H14V12Z" fill="#000000"></path> </g></svg>';
            case 'question':
                return '❓';
            case 'answer':
                return '💡';
            default:
                return '🔍';
        }
    }
});
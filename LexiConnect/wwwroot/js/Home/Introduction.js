
// Enter key search
document.querySelector('.search-input').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        document.querySelector('.search-button').click();
    }
});

// Enhanced search functionality - Add this to your @section Scripts

document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.querySelector('.search-input');
    const searchButton = document.querySelector('.search-button');
    const searchContainer = document.querySelector('.search-container');

    // Create suggestions dropdown
    const suggestionsDropdown = document.createElement('div');
    suggestionsDropdown.className = 'search-suggestions';
    suggestionsDropdown.style.display = 'none';
    searchContainer.appendChild(suggestionsDropdown);

    let debounceTimer;
    let selectedIndex = -1;

    // Perform search function
    function performSearch(query) {
        if (query.trim()) {
            window.location.href = `/Search/Results?query=${encodeURIComponent(query.trim())}`;
        }
    }

    // Search button click
    searchButton.addEventListener('click', function () {
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
                    <div class="suggestion-meta">${item.type} • ${item.university}</div>
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
        window.location.href = item.url;
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
        switch (type.toLowerCase()) {
            case 'document':
                return '📄';
            case 'course':
                return '📚';
            case 'university':
                return '🏫';
            default:
                return '🔍';
        }
    }
});
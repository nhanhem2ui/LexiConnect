// Universities search functionality with live suggestions
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('universitySearch');
    const searchResults = document.getElementById('searchResults');
    let searchTimeout;
    let currentFocus = -1;

    if (!searchInput || !searchResults) return;

    // Handle input changes
    searchInput.addEventListener('input', function (e) {
        const query = e.target.value.trim();

        // Clear previous timeout
        clearTimeout(searchTimeout);

        // Hide results if query is too short
        if (query.length < 2) {
            hideResults();
            return;
        }

        // Debounce the search
        searchTimeout = setTimeout(() => {
            searchUniversities(query);
        }, 300);
    });

    // Handle keyboard navigation
    searchInput.addEventListener('keydown', function (e) {
        const items = searchResults.querySelectorAll('.search-result-item');

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            currentFocus++;
            addActive(items);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            currentFocus--;
            addActive(items);
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (currentFocus > -1 && items[currentFocus]) {
                items[currentFocus].click();
            } else if (query.length >= 2) {
                // Submit search
                window.location.href = `/University/AllUniversities?search=${encodeURIComponent(query)}`;
            }
        } else if (e.key === 'Escape') {
            hideResults();
        }
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function (e) {
        if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
            hideResults();
        }
    });

    // Handle search button click
    const searchBtn = document.querySelector('.search-btn-large');
    if (searchBtn) {
        searchBtn.addEventListener('click', function () {
            const query = searchInput.value.trim();
            if (query.length >= 2) {
                window.location.href = `/University/AllUniversities?search=${encodeURIComponent(query)}`;
            }
        });
    }

    // Search universities function
    function searchUniversities(query) {
        fetch(`/Search/UniversitiesSearchSuggestions?query=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(data => {
                displayResults(data);
            })
            .catch(error => {
                console.error('Search error:', error);
                hideResults();
            });
    }

    // Display search results
    function displayResults(universities) {
        currentFocus = -1;

        if (!universities || universities.length === 0) {
            searchResults.innerHTML = `
                <div class="search-no-results">
                    <p>No universities found</p>
                </div>
            `;
            searchResults.classList.add('show');
            return;
        }

        let html = '';
        universities.forEach(uni => {
            const displayName = uni.shortName ? `${uni.name} (${uni.shortName})` : uni.name;
            const location = `${uni.city}, ${uni.country}`;

            html += `
                <a href="${uni.url}" class="search-result-item">
                    <div class="search-result-logo">
                        ${uni.logoUrl ?
                    `<img src="${uni.logoUrl}" alt="${uni.name}" />` :
                    `<svg viewBox="0 0 24 24" width="24" height="24">
                                <path d="M12 3L1 9L5 11.18V17.18L12 21L19 17.18V11.18L21 10.09V17H23V9L12 3Z" />
                            </svg>`
                }
                    </div>
                    <div class="search-result-info">
                        <div class="search-result-name">${displayName}</div>
                        <div class="search-result-details">${location}</div>
                    </div>
                </a>
            `;
        });

        searchResults.innerHTML = html;
        searchResults.classList.add('show');
    }

    // Hide search results
    function hideResults() {
        searchResults.classList.remove('show');
        searchResults.innerHTML = '';
        currentFocus = -1;
    }

    // Add active class to current item
    function addActive(items) {
        if (!items || items.length === 0) return;

        // Remove active class from all items
        removeActive(items);

        // Handle wrapping
        if (currentFocus >= items.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = items.length - 1;

        // Add active class to current item
        items[currentFocus].classList.add('active');
        items[currentFocus].scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }

    // Remove active class from all items
    function removeActive(items) {
        items.forEach(item => item.classList.remove('active'));
    }
});
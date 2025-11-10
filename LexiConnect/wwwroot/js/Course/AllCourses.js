document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('courseSearch');
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
            searchCourses(query);
        }, 300);
    });

    // Handle keyboard navigation
    searchInput.addEventListener('keydown', function (e) {
        const items = searchResults.querySelectorAll('.search-result-item');
        const query = searchInput.value.trim();

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
                window.location.href = `/Course/AllCourses?search=${encodeURIComponent(query)}`;
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
                window.location.href = `/Course/AllCourses?search=${encodeURIComponent(query)}`;
            }
        });
    }

    // Search courses function
    function searchCourses(query) {
        fetch(`/Course/CoursesSearchSuggestions?query=${encodeURIComponent(query)}`)
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
    function displayResults(courses) {
        currentFocus = -1;

        if (!courses || courses.length === 0) {
            searchResults.innerHTML = `
                <div class="search-no-results">
                    <p>No courses found</p>
                </div>
            `;
            searchResults.classList.add('show');
            return;
        }

        let html = '';
        courses.forEach(course => {
            const displayName = `${course.courseCode} - ${course.courseName}`;
            const details = course.majorName || 'General Course';
            const docCount = course.documentCount || 0;

            html += `
                <a href="${course.url}" class="search-result-item">
                    <div class="search-result-icon">
                        <svg viewBox="0 0 24 24" width="20" height="20">
                            <path d="M19,2H5C3.89,2 3,2.89 3,4V20C3,21.11 3.89,22 5,22H19C20.11,22 21,21.11 21,20V4C21,2.89 20.11,2 19,2M19,20H5V4H19V20M17,12H7V14H17V12M17,9H7V11H17V9M17,6H7V8H17V6Z" />
                        </svg>
                    </div>
                    <div class="search-result-info">
                        <div class="search-result-name">${displayName}</div>
                        <div class="search-result-details">${details} • ${docCount} documents</div>
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
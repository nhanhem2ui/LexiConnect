// UniversitySearch.js - Real-time university search functionality

let searchTimeout = null;
const searchDelay = 300; // milliseconds

document.addEventListener('DOMContentLoaded', function () {
    initializeUniversitySearch();
});

function initializeUniversitySearch() {
    const searchInputs = document.querySelectorAll('.university-search-input');

    searchInputs.forEach(input => {
        const index = input.dataset.index;
        const dropdown = document.querySelector(`.university-dropdown[data-index="${index}"]`);
        const hiddenInput = input.closest('.doc-form').querySelector('.university-id-input');

        // Load initial universities on focus
        input.addEventListener('focus', function () {
            if (dropdown.querySelectorAll('.university-item').length === 0) {
                loadUniversities('', dropdown, hiddenInput, input);
            }
            dropdown.style.display = 'block';
        });

        // Search as user types
        input.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            const searchTerm = this.value.trim();

            searchTimeout = setTimeout(() => {
                loadUniversities(searchTerm, dropdown, hiddenInput, input);
                dropdown.style.display = 'block';
            }, searchDelay);
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            if (!input.contains(e.target) && !dropdown.contains(e.target)) {
                dropdown.style.display = 'none';
            }
        });

        // Handle keyboard navigation
        input.addEventListener('keydown', function (e) {
            handleKeyboardNavigation(e, dropdown);
        });
    });
}

function loadUniversities(searchTerm, dropdown, hiddenInput, input) {
    // Show loading state
    dropdown.innerHTML = '<div class="university-dropdown-loading">Searching...</div>';
    dropdown.style.display = 'block';

    const url = `/Upload/SearchUniversities?searchTerm=${encodeURIComponent(searchTerm)}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (data.error) {
                dropdown.innerHTML = `<div class="university-dropdown-error">Error: ${data.error}</div>`;
                return;
            }

            if (data.length === 0) {
                dropdown.innerHTML = '<div class="university-dropdown-empty">No universities found</div>';
                return;
            }

            renderUniversityList(data, dropdown, hiddenInput, input);
        })
        .catch(error => {
            console.error('Error loading universities:', error);
            dropdown.innerHTML = '<div class="university-dropdown-error">Failed to load universities</div>';
        });
}

function renderUniversityList(universities, dropdown, hiddenInput, input) {
    const html = universities.map(uni => `
        <div class="university-item" 
             data-id="${uni.id}" 
             data-name="${escapeHtml(uni.name)}"
             data-display="${escapeHtml(uni.displayText)}">
            ${uni.logoUrl ? `<img src="${uni.logoUrl}" alt="${escapeHtml(uni.name)}" class="university-logo" onerror="this.style.display='none'">` : ''}
            <div class="university-info">
                <div class="university-name">${highlightMatch(uni.name, input.value)}</div>
                <div class="university-location">${escapeHtml(uni.city)}, ${escapeHtml(uni.country)}</div>
                ${uni.shortName ? `<div class="university-short-name">${escapeHtml(uni.shortName)}</div>` : ''}
            </div>
        </div>
    `).join('');

    dropdown.innerHTML = html;

    // Add click handlers
    dropdown.querySelectorAll('.university-item').forEach(item => {
        item.addEventListener('click', function () {
            selectUniversity(this, hiddenInput, input, dropdown);
        });
    });
}

function selectUniversity(item, hiddenInput, input, dropdown) {
    const universityId = item.dataset.id;
    const universityName = item.dataset.name;
    const displayText = item.dataset.display;

    // Set values
    hiddenInput.value = universityId;
    input.value = displayText;

    // Visual feedback
    input.classList.add('university-selected');
    input.classList.remove('is-invalid');

    // Hide dropdown
    dropdown.style.display = 'none';

    console.log('Selected university:', { id: universityId, name: universityName });
}

function handleKeyboardNavigation(e, dropdown) {
    const items = dropdown.querySelectorAll('.university-item');
    if (items.length === 0) return;

    const currentIndex = Array.from(items).findIndex(item =>
        item.classList.contains('university-item-active')
    );

    switch (e.key) {
        case 'ArrowDown':
            e.preventDefault();
            const nextIndex = currentIndex < items.length - 1 ? currentIndex + 1 : 0;
            setActiveItem(items, nextIndex);
            break;

        case 'ArrowUp':
            e.preventDefault();
            const prevIndex = currentIndex > 0 ? currentIndex - 1 : items.length - 1;
            setActiveItem(items, prevIndex);
            break;

        case 'Enter':
            e.preventDefault();
            if (currentIndex >= 0) {
                items[currentIndex].click();
            }
            break;

        case 'Escape':
            dropdown.style.display = 'none';
            break;
    }
}

function setActiveItem(items, index) {
    items.forEach(item => item.classList.remove('university-item-active'));
    items[index].classList.add('university-item-active');
    items[index].scrollIntoView({ block: 'nearest' });
}

function highlightMatch(text, searchTerm) {
    if (!searchTerm) return escapeHtml(text);

    const regex = new RegExp(`(${escapeRegex(searchTerm)})`, 'gi');
    return escapeHtml(text).replace(regex, '<mark>$1</mark>');
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function escapeRegex(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

// Clear university selection if input is manually cleared
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.university-search-input').forEach(input => {
        input.addEventListener('change', function () {
            if (this.value.trim() === '') {
                const hiddenInput = this.closest('.doc-form').querySelector('.university-id-input');
                hiddenInput.value = '';
                this.classList.remove('university-selected');
            }
        });
    });
});
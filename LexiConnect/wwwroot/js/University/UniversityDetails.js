// University Detail Page JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Smooth scroll for alphabet filter
    initAlphabetFilter();

    // Search functionality
    initSearch();

    // Horizontal scroll for document grids
    initHorizontalScroll();
});

function initAlphabetFilter() {
    const letterBtns = document.querySelectorAll('.letter-btn');
    letterBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            // Visual feedback
            letterBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

function initSearch() {
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        // Auto-submit on Enter
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                this.closest('form').submit();
            }
        });

        // Focus on Ctrl/Cmd + K
        document.addEventListener('keydown', function (e) {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                searchInput.focus();
            }
        });
    }
}

function initHorizontalScroll() {
    const grids = document.querySelectorAll('.documents-grid, .universities-grid');

    grids.forEach(grid => {
        let isDown = false;
        let startX;
        let scrollLeft;

        // Optional: Add mouse drag scrolling
        grid.addEventListener('mousedown', (e) => {
            if (e.target.closest('a')) return; // Don't interfere with links
            isDown = true;
            grid.style.cursor = 'grabbing';
            startX = e.pageX - grid.offsetLeft;
            scrollLeft = grid.scrollLeft;
        });

        grid.addEventListener('mouseleave', () => {
            isDown = false;
            grid.style.cursor = 'default';
        });

        grid.addEventListener('mouseup', () => {
            isDown = false;
            grid.style.cursor = 'default';
        });

        grid.addEventListener('mousemove', (e) => {
            if (!isDown) return;
            e.preventDefault();
            const x = e.pageX - grid.offsetLeft;
            const walk = (x - startX) * 2;
            grid.scrollLeft = scrollLeft - walk;
        });
    });
}

// Animation on scroll
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0)';
        }
    });
}, observerOptions);

// Observe sections for fade-in animation
document.querySelectorAll('.documents-section, .courses-section, .other-universities-section').forEach(section => {
    section.style.opacity = '0';
    section.style.transform = 'translateY(20px)';
    section.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
    observer.observe(section);
});
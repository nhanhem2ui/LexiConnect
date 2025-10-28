document.addEventListener('DOMContentLoaded', function () {
    // ===============================
    // Trending scroll functionality
    // ===============================
    const trendingGrid = document.getElementById('trendingGrid');
    const prevBtn = document.getElementById('trendingPrev');
    const nextBtn = document.getElementById('trendingNext');

    if (prevBtn && nextBtn && trendingGrid) {
        prevBtn.addEventListener('click', () => {
            trendingGrid.scrollBy({ left: -300, behavior: 'smooth' });
        });

        nextBtn.addEventListener('click', () => {
            trendingGrid.scrollBy({ left: 300, behavior: 'smooth' });
        });
    }

    // ===============================
    // Follow button functionality (AJAX)
    // ===============================
    const followBtn = document.getElementById('followBtn');
    if (followBtn) {
        followBtn.addEventListener('click', async function (e) {
            e.preventDefault();

            const courseId = window.location.pathname.split('/').pop(); // assumes /Course/CourseDetails/{id}
            const isFollowing = this.classList.contains('following');

            try {
                const response = await fetch('/Course/ToggleFollowCourse', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': getCsrfToken()
                    },
                    body: `courseId=${encodeURIComponent(courseId)}`
                });

                if (!response.ok) {
                    if (response.status === 401) {
                        alert('Please log in to follow courses.');
                        return;
                    }
                    throw new Error('Server error while toggling follow');
                }

                const result = await response.json();

                if (result.success) {
                    if (result.isFollowing) {
                        followBtn.classList.add('following');
                        followBtn.innerHTML = `
                            <svg viewBox="0 0 24 24" width="20" height="20">
                                <path d="M12 21.35L10.55 20.03C5.4 15.36 2 12.28 
                                         2 8.5C2 5.42 4.42 3 7.5 3C9.24 3 10.91 3.81 
                                         12 5.09C13.09 3.81 14.76 3 16.5 3C19.58 3 
                                         22 5.42 22 8.5C22 12.28 18.6 15.36 
                                         13.45 20.04L12 21.35Z"/>
                            </svg> Following`;
                    } else {
                        followBtn.classList.remove('following');
                        followBtn.innerHTML = `
                            <svg viewBox="0 0 24 24" width="20" height="20">
                                <path d="M12 21.35L10.55 20.03C5.4 15.36 
                                         2 12.28 2 8.5C2 5.42 4.42 3 
                                         7.5 3C9.24 3 10.91 3.81 12 5.09
                                         C13.09 3.81 14.76 3 16.5 3C19.58 3 
                                         22 5.42 22 8.5C22 12.28 18.6 15.36 
                                         13.45 20.04L12 21.35Z"/>
                            </svg> Follow`;
                    }
                } else {
                    alert('Failed to toggle follow. Please try again.');
                }
            } catch (err) {
                console.error('Error:', err);
                alert('An error occurred while following/unfollowing.');
            }
        });
    }

    // Helper function to get AntiForgeryToken from hidden input (MVC requirement)
    function getCsrfToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // ===============================
    // Sort buttons functionality
    // ===============================
    const sortButtons = document.querySelectorAll('.sort-btn');
    sortButtons.forEach(button => {
        button.addEventListener('click', function () {
            sortButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            console.log('Sorting by:', this.dataset.sort);
            // You can add AJAX reload for documents here later
        });
    });

    // ===============================
    // View toggle functionality
    // ===============================
    const viewButtons = document.querySelectorAll('.view-btn');
    const documentsList = document.getElementById('documentsList');

    viewButtons.forEach(button => {
        button.addEventListener('click', function () {
            viewButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            const viewType = this.dataset.view;

            if (viewType === 'grid') {
                documentsList.classList.remove('list-view');
                documentsList.classList.add('grid-view');
            } else {
                documentsList.classList.remove('grid-view');
                documentsList.classList.add('list-view');
            }
        });
    });

    // ===============================
    // Search functionality (AJAX placeholder)
    // ===============================
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(async () => {
                const searchTerm = this.value.trim();
                const courseId = window.location.pathname.split('/').pop();
                if (searchTerm.length > 0) {
                    console.log('Searching for:', searchTerm);

                    try {
                        const res = await fetch(`/Course/SearchCourseDocuments?courseId=${courseId}&query=${encodeURIComponent(searchTerm)}`);
                        const data = await res.json();
                        console.log('Search results:', data.documents);
                        // TODO: update UI with new documents
                    } catch (err) {
                        console.error('Search error:', err);
                    }
                }
            }, 500);
        });
    }

    // ===============================
    // More options placeholder
    // ===============================
    document.querySelectorAll('.more-options').forEach(button => {
        button.addEventListener('click', e => {
            e.preventDefault();
            e.stopPropagation();
            console.log('More options clicked');
        });
    });

    document.querySelectorAll('.document-card').forEach(card => {
        card.addEventListener('click', e => {
            if (e.target.closest('.more-options')) e.preventDefault();
        });
    });
});

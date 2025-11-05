

////$(document).on("click", ".like-button", function () {
////    var btn = $(this);
////    var docId = btn.data("id");

////    $.ajax({
////        url: '/Document/ToggleLike',
////        type: 'POST',
////        data: {
////            id: docId,
////            isLike: true, // gửi true khi bấm like
////            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
////        },
////        success: function (res) {
////            if (res.success) {
////                // cập nhật số like ở span completion-percentage
////                btn.closest(".top-document-card").find(".completion-percentage").text(res.likeCount);
////            } else {
////                alert(res.message);
////            }
////        },
////        error: function () {
////            alert("Có lỗi xảy ra khi like tài liệu");
////        }
////    });
////});


//$(document).on("click", ".like-button", function () {
//    var btn = $(this);
//    var docId = btn.data("id");
//    var isLiked = btn.hasClass("liked"); // kiểm tra trạng thái hiện tại
//    var token = $('input[name="__RequestVerificationToken"]').val();

//    $.ajax({
//        url: '/Document/ToggleLike',
//        type: 'POST',
//        data: {
//            id: docId,
//            isLike: !isLiked, // đảo trạng thái
//            __RequestVerificationToken: token
//        },
//        success: function (res) {
//            if (res.success) {
//                btn.closest(".top-document-card").find(".completion-percentage").text(res.likeCount);

//                // đổi trạng thái nút
//                if (isLiked) {
//                    btn.removeClass("liked").text("👍 ");
//                } else {
//                    btn.addClass("liked").text("👎 ");
//                }
//            } else {
//                alert(res.message);
//            }
//        },
//        error: function () {
//            alert("Có lỗi xảy ra khi like tài liệu");
//        }
//    });
//});

$(document).on("click", ".like-button", function () {
    var btn = $(this);
    var docId = btn.data("id");
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Document/ToggleLike',
        type: 'POST',
        data: {
            id: docId,
            __RequestVerificationToken: token
        },
        success: function (res) {
            if (res.success) {
                btn.closest(".top-document-card")
                    .find(".completion-percentage")
                    .text(res.likeCount);

                if (res.isLiked) {
                    btn.addClass("liked");
                    // Show filled thumbs up (liked state)
                    btn.html('<svg version="1.0" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 64 64" width="24" height="24" enable-background="new 0 0 64 64" xml:space="preserve" fill="#000000"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g> <circle fill="#231F20" cx="7" cy="57" r="1"></circle> <g> <path fill="#231F20" d="M14,26c0-2.212-1.789-4-4-4H4c-2.211,0-4,1.788-4,4v34c0,2.21,1.789,4,4,4h6c2.211,0,4-1.79,4-4V26z M7,60 c-1.657,0-3-1.344-3-3c0-1.658,1.343-3,3-3s3,1.342,3,3C10,58.656,8.657,60,7,60z"></path> <path fill="#231F20" d="M64,28c0-3.314-2.687-6-6-6H41l0,0h-0.016H41l2-18c0.209-2.188-1.287-4-3.498-4h-4.001 C33,0,31.959,1.75,31,4l-8,18c-2.155,5.169-5,6-7,6v30.218c1.203,0.285,2.714,0.945,4.21,2.479C23.324,63.894,27.043,64,29,64h23 c3.313,0,6-2.688,6-6c0-1.731-0.737-3.288-1.91-4.383C58.371,52.769,60,50.577,60,48c0-1.731-0.737-3.288-1.91-4.383 C60.371,42.769,62,40.577,62,38c0-1.731-0.737-3.288-1.91-4.383C62.371,32.769,64,30.577,64,28z"></path> </g> </g> </g></svg>');
                } else {
                    btn.removeClass("liked");
                    // Show outline thumbs up (unliked state)
                    btn.html('<svg version="1.0" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 64 64" width="24" height="24" enable-background="new 0 0 64 64" xml:space="preserve" fill="#000000"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g> <circle fill="#231F20" cx="7" cy="35" r="1"></circle> <g> <path fill="#231F20" d="M0,4c0-2.211,1.789-4,4-4h6c2.211,0,4,1.789,4,4v34c0,2.211-1.789,4-4,4H4c-2.211,0-4-1.789-4-4V4z M7,38 c1.657,0,3-1.343,3-3s-1.343-3-3-3s-3,1.343-3,3S5.343,38,7,38z"></path> <path fill="#231F20" d="M64,36c0,3.313-2.687,6-6,6H41l0,0h-0.016H41l2,18c0.209,2.187-1.287,4-3.498,4h-4.001 C33,64,31.959,62.25,31,60l-8-18c-2.155-5.17-5-6-7-6V5.781c1.203-0.285,2.714-0.945,4.21-2.479C23.324,0.105,27.043,0,29,0h23 c3.313,0,6,2.687,6,6c0,1.73-0.737,3.287-1.91,4.382C58.371,11.23,60,13.422,60,16c0,1.73-0.737,3.287-1.91,4.382 C60.371,21.23,62,23.422,62,26c0,1.73-0.737,3.287-1.91,4.382C62.371,31.23,64,33.422,64,36z"></path> </g> </g> </g></svg>');
                }
            } else {
                alert(res.message);
            }
        },
        error: function () {
            alert("Có lỗi xảy ra khi like tài liệu");
        }
    });
});
// Enhanced Homepage JavaScript
$(document).ready(function () {
    // Initialize card animations
    initializeCardAnimations();

    // Handle like button clicks
    handleLikeButtons();

    // Add smooth scroll behavior
    addSmoothScroll();

    // Initialize tooltips if Bootstrap is available
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        initializeTooltips();
    }
});

// Animate cards on scroll
function initializeCardAnimations() {
    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                // Add staggered animation delay
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, index * 50);

                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe all document cards
    document.querySelectorAll('.top-document-card').forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'all 0.5s ease-out';
        observer.observe(card);
    });
}

// Handle like button functionality
function handleLikeButtons() {
    $('.like-button').on('click', function (e) {
        e.stopPropagation();
        const $button = $(this);
        const documentId = $button.data('id');

        // Optimistic UI update
        const isLiked = $button.hasClass('liked');
        $button.toggleClass('liked');
        $button.text(isLiked ? '🤍' : '❤️');

        // Add animation
        $button.addClass('heartbeat-animation');
        setTimeout(() => {
            $button.removeClass('heartbeat-animation');
        }, 600);

        // Make AJAX request
        $.ajax({
            url: '/Document/ToggleLike',
            method: 'POST',
            data: {
                id: documentId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    // Update like count in the badge
                    const $card = $button.closest('.top-document-card');
                    const $badge = $card.find('.completion-badge .completion-percentage');
                    $badge.text(response.likeCount + ' likes');

                    // Show success feedback
                    showToast(response.isLiked ? 'Added to favorites!' : 'Removed from favorites', 'success');
                } else {
                    // Revert on error
                    $button.toggleClass('liked');
                    $button.text(isLiked ? '❤️' : '🤍');
                    showToast(response.message || 'Failed to update', 'error');
                }
            },
            error: function (xhr) {
                // Revert on error
                $button.toggleClass('liked');
                $button.text(isLiked ? '❤️' : '🤍');

                if (xhr.status === 401) {
                    showToast('Please login to like documents', 'warning');
                } else {
                    showToast('Failed to update. Please try again.', 'error');
                }
            }
        });
    });
}

// Add smooth scrolling
function addSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// Initialize tooltips
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(
        document.querySelectorAll('[data-bs-toggle="tooltip"]')
    );
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Show toast notification
function showToast(message, type = 'info') {
    // Remove existing toasts
    $('.custom-toast').remove();

    const icons = {
        success: '✓',
        error: '✕',
        warning: '⚠',
        info: 'ℹ'
    };

    const colors = {
        success: '#10b981',
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    };

    const toast = $(`
        <div class="custom-toast" style="
            position: fixed;
            bottom: 24px;
            right: 24px;
            background: white;
            padding: 16px 24px;
            border-radius: 12px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
            display: flex;
            align-items: center;
            gap: 12px;
            z-index: 9999;
            animation: slideIn 0.3s ease-out;
            border-left: 4px solid ${colors[type]};
        ">
            <span style="
                font-size: 20px;
                color: ${colors[type]};
                font-weight: bold;
            ">${icons[type]}</span>
            <span style="
                color: #1f2937;
                font-weight: 500;
            ">${message}</span>
        </div>
    `);

    $('body').append(toast);

    // Add slide in animation
    const style = $('<style>@keyframes slideIn { from { transform: translateX(400px); opacity: 0; } to { transform: translateX(0); opacity: 1; } }</style>');
    $('head').append(style);

    // Auto remove after 3 seconds
    setTimeout(() => {
        toast.css({
            animation: 'slideOut 0.3s ease-out',
            transform: 'translateX(400px)',
            opacity: '0'
        });
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Add loading skeleton for cards
function showLoadingSkeleton(container) {
    const skeletonCard = `
        <div class="top-document-card loading" style="
            background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
            background-size: 200% 100%;
            animation: shimmer 1.5s infinite;
        ">
            <div style="height: 140px; background: #e9ecef; border-radius: 12px; margin-bottom: 16px;"></div>
            <div style="height: 20px; background: #e9ecef; border-radius: 4px; margin-bottom: 8px;"></div>
            <div style="height: 16px; background: #e9ecef; border-radius: 4px; width: 60%;"></div>
        </div>
    `;

    $(container).html(skeletonCard.repeat(3));
}

// Lazy load images
function lazyLoadImages() {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });

    document.querySelectorAll('img.lazy').forEach(img => {
        imageObserver.observe(img);
    });
}

// Add parallax effect to welcome banner
$(window).on('scroll', function () {
    const scrolled = $(window).scrollTop();
    $('.welcome-banner').css('transform', `translateY(${scrolled * 0.3}px)`);
});

// Prevent card click when clicking like button
$(document).on('click', '.top-document-card', function (e) {
    if ($(e.target).closest('.like-button').length === 0) {
        // Allow navigation
        return true;
    }
    return false;
});

// Add hover effect sound (optional - uncomment if you want sound effects)
/*
function playHoverSound() {
    const audio = new Audio('/sounds/hover.mp3');
    audio.volume = 0.2;
    audio.play().catch(() => {}); // Ignore errors
}

$('.top-document-card, .btn-action, .btn-see-all').on('mouseenter', function() {
    playHoverSound();
});
*/

// Handle window resize
let resizeTimer;
$(window).on('resize', function () {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(function () {
        // Reinitialize any responsive features
        console.log('Window resized');
    }, 250);
});

// Add keyboard navigation
$(document).on('keydown', function (e) {
    if (e.key === 'Escape') {
        // Close any open modals or overlays
        $('.modal').modal('hide');
    }
});

// Performance monitoring (optional)
if ('PerformanceObserver' in window) {
    const perfObserver = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
            if (entry.duration > 100) {
                console.warn('Slow operation detected:', entry.name, entry.duration);
            }
        }
    });

    perfObserver.observe({ entryTypes: ['measure'] });
}

// Export functions for use in other scripts
window.dashboardUtils = {
    showToast,
    showLoadingSkeleton,
    lazyLoadImages
};
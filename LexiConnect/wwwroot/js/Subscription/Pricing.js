// Subscription Page Interactive Features
document.addEventListener('DOMContentLoaded', function () {

    // Smooth animations for cards
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const cardObserver = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, index * 150);
            }
        });
    }, observerOptions);

    // Observe all pricing cards
    document.querySelectorAll('.pricing-card').forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(50px)';
        card.style.transition = 'all 0.8s cubic-bezier(0.4, 0, 0.2, 1)';
        cardObserver.observe(card);
    });

    // Add interactive hover effects - but skip cards with disabled buttons
    document.querySelectorAll('.pricing-card').forEach(card => {
        const button = card.querySelector('.plan-button');

        // Check if the button is disabled or has current plan class - if so, skip hover effects
        if (button && (button.disabled || button.classList.contains('btn-current'))) {
            card.style.cursor = 'default';
            // Add special styling for current plan cards
            if (card.classList.contains('current-plan')) {
                card.addEventListener('mouseenter', function () {
                    this.style.transform = 'scale(1.02)';
                    this.style.boxShadow = '0 8px 25px rgba(34, 197, 94, 0.2)';
                });
                card.addEventListener('mouseleave', function () {
                    this.style.transform = 'scale(1.02)';
                    this.style.boxShadow = '0 8px 25px rgba(34, 197, 94, 0.15)';
                });
            }
            return; // Skip adding normal hover effects for disabled/current plan cards
        }

        card.addEventListener('mouseenter', function () {
            this.style.transform += ' rotateY(5deg)';
            // Add subtle glow effect
            this.style.boxShadow = '0 25px 50px rgba(0, 0, 0, 0.2), 0 0 30px rgba(25, 118, 210, 0.1)';
        });

        card.addEventListener('mouseleave', function () {
            if (this.classList.contains('popular')) {
                this.style.transform = 'scale(1.05) translateY(-10px)';
            } else {
                this.style.transform = 'translateY(-10px)';
            }
            this.style.boxShadow = '0 25px 50px rgba(0, 0, 0, 0.2)';
        });

        // Button click effects - only for enabled buttons
        if (button) {
            button.addEventListener('click', function (e) {
                // Skip effects for disabled buttons or current plan buttons
                if (this.disabled || this.classList.contains('loading') || this.classList.contains('btn-current')) {
                    e.preventDefault();
                    return false;
                }

                // Create ripple effect
                const ripple = document.createElement('span');
                const rect = this.getBoundingClientRect();
                const size = Math.max(rect.width, rect.height);
                const x = e.clientX - rect.left - size / 2;
                const y = e.clientY - rect.top - size / 2;

                ripple.style.cssText = `
                    position: absolute;
                    width: ${size}px;
                    height: ${size}px;
                    left: ${x}px;
                    top: ${y}px;
                    background: rgba(255, 255, 255, 0.5);
                    border-radius: 50%;
                    transform: scale(0);
                    animation: ripple 0.6s linear;
                    pointer-events: none;
                `;

                this.style.position = 'relative';
                this.appendChild(ripple);

                // Add loading state
                this.classList.add('loading');
                const originalText = this.textContent;
                this.textContent = 'Processing...';

                // Clean up ripple
                setTimeout(() => {
                    ripple.remove();
                }, 600);

                // Let the form submit naturally after a short delay for visual feedback
                setTimeout(() => {
                    // If for some reason the form doesn't submit, restore the button
                    this.classList.remove('loading');
                    this.textContent = originalText;
                }, 2000);

                // Don't prevent default - let the form submit naturally
            });
        }
    });

    // Parallax effect for background
    window.addEventListener('scroll', function () {
        const scrolled = window.pageYOffset;
        const parallax = document.querySelector('.subscription-page::before');
        const rate = scrolled * -0.5;

        if (parallax) {
            parallax.style.transform = `translateY(${rate}px)`;
        }
    });

    // Add floating animation to popular badge
    const popularBadges = document.querySelectorAll('.pricing-card.popular::before');
    popularBadges.forEach(badge => {
        setInterval(() => {
            badge.style.animation = 'none';
            setTimeout(() => {
                badge.style.animation = 'pulse 2s infinite';
            }, 10);
        }, 3000);
    });

    // Plan comparison functionality - exclude disabled cards and current plan cards
    function initPlanComparison() {
        const cards = document.querySelectorAll('.pricing-card');
        let activeCard = null;

        cards.forEach(card => {
            const button = card.querySelector('.plan-button');

            // Skip disabled cards and current plan cards for comparison functionality
            if (button && (button.disabled || button.classList.contains('btn-current'))) {
                return;
            }

            card.addEventListener('click', function (e) {
                // Don't interfere with button clicks
                if (e.target.classList.contains('plan-button') || e.target.closest('form')) {
                    return;
                }

                // Remove active state from other cards
                cards.forEach(c => c.classList.remove('active-comparison'));

                // Add active state to clicked card
                this.classList.add('active-comparison');
                activeCard = this;

                // Highlight differences
                highlightFeatures();
            });
        });
    }

    function highlightFeatures() {
        const allFeatures = document.querySelectorAll('.plan-features li');
        allFeatures.forEach(feature => {
            feature.style.background = 'transparent';
        });

        // Add subtle highlight to unique features
        setTimeout(() => {
            allFeatures.forEach(feature => {
                feature.style.transition = 'background-color 0.3s ease';
            });
        }, 100);
    }

    // Initialize features
    initPlanComparison();

    // Force cursor styles for disabled buttons
    document.querySelectorAll('.plan-button:disabled, .btn-current, .btn-disabled').forEach(button => {
        button.style.cursor = 'not-allowed';
        button.addEventListener('mouseenter', function () {
            this.style.cursor = 'not-allowed';
        });
        button.addEventListener('mouseleave', function () {
            this.style.cursor = 'not-allowed';
        });

        // Prevent any click events
        button.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            return false;
        });
    });
});

// Add CSS for ripple animation and loading states
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    .plan-button.loading {
        opacity: 0.7;
        cursor: wait;
        position: relative;
        overflow: hidden;
    }
    
    .active-comparison {
        transform: translateY(-15px) scale(1.02) !important;
        box-shadow: 0 30px 60px rgba(25, 118, 210, 0.3) !important;
    }
    
    .pricing-card {
        cursor: pointer;
    }
    
    .plan-button:disabled,
    .btn-current,
    .btn-disabled {
        opacity: 1 !important;
        cursor: not-allowed !important;
        pointer-events: none;
        background: linear-gradient(135deg, #f3f4f6, #e5e7eb) !important;
        color: #6b7280 !important;
        border: 2px solid #d1d5db !important;
    }
    
    .plan-button:disabled:hover,
    .btn-current:hover,
    .btn-disabled:hover {
        cursor: not-allowed !important;
        transform: none !important;
        background: linear-gradient(135deg, #f3f4f6, #e5e7eb) !important;
        color: #6b7280 !important;
    }
    
    /* Special styling for current plan buttons */
    .btn-current,
    .pricing-card.current-plan .btn-current {
        background: linear-gradient(135deg, #dcfce7, #bbf7d0) !important;
        color: #16a34a !important;
        border: 2px solid #22c55e !important;
        font-weight: 600 !important;
    }
    
    .btn-current:hover {
        background: linear-gradient(135deg, #dcfce7, #bbf7d0) !important;
        color: #16a34a !important;
    }
    
    /* Style cards with disabled buttons differently */
    .pricing-card:has(.plan-button:disabled) {
        cursor: default;
    }
    
    .pricing-card.current-plan {
        cursor: default;
    }
    
    .pricing-card:has(.plan-button:disabled):hover:not(.current-plan) {
        transform: translateY(-10px) rotateX(5deg) !important;
        box-shadow: 0 25px 50px rgba(0, 0, 0, 0.15) !important;
    }
    
    .pricing-card.current-plan:hover {
        transform: scale(1.02) !important;
        box-shadow: 0 8px 25px rgba(34, 197, 94, 0.2) !important;
    }
`;
document.head.appendChild(style);
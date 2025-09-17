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

    // Add interactive hover effects
    document.querySelectorAll('.pricing-card').forEach(card => {
        const button = card.querySelector('.plan-button');

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

        // Button click effects
        if (button) {
            button.addEventListener('click', function (e) {
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

                setTimeout(() => {
                    ripple.remove();
                }, 600);
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

    // Plan comparison functionality
    function initPlanComparison() {
        const cards = document.querySelectorAll('.pricing-card');
        let activeCard = null;

        cards.forEach(card => {
            card.addEventListener('click', function (e) {
                if (e.target.classList.contains('plan-button')) return;

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

    // Add loading animation for buttons
    document.querySelectorAll('.plan-button').forEach(button => {
        button.addEventListener('click', function (e) {
            if (this.classList.contains('loading')) return;

            e.preventDefault();
            this.classList.add('loading');

            const originalText = this.textContent;
            this.textContent = 'Processing...';

            // Simulate processing
            setTimeout(() => {
                // Here you would normally handle the actual subscription
                window.location.href = '/Subscription/Subscribe?planId=' + this.closest('.pricing-card').dataset.planId;
            }, 1500);
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
        cursor: not-allowed;
        pointer-events: none;
    }
    
    .active-comparison {
        transform: translateY(-15px) scale(1.02) !important;
        box-shadow: 0 30px 60px rgba(25, 118, 210, 0.3) !important;
    }
    
    .pricing-card {
        cursor: pointer;
    }
`;
document.head.appendChild(style);
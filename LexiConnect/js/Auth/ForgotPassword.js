document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('forgotPasswordForm');
    const resetBtn = document.getElementById('resetBtn');
    const btnText = resetBtn.querySelector('.btn-text');

    form.addEventListener('submit', function (e) {
        // Show loading state
        resetBtn.disabled = true;
        resetBtn.classList.add('loading');
        btnText.innerHTML = '<span class="spinner"></span><b>Sending...</b>';
    });

    // Reset button state if there are validation errors
    if (document.querySelector('.validation-summary')) {
        resetBtn.disabled = false;
        resetBtn.classList.remove('loading');
        btnText.innerHTML = '<b>Send Reset Link</b>';
    }
});

// Email validation
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

document.getElementById('email').addEventListener('blur', function () {
    const email = this.value;
    const errorSpan = this.nextElementSibling;

    if (email && !validateEmail(email)) {
        this.classList.add('input-validation-error');
        errorSpan.textContent = 'Please enter a valid email address';
    } else {
        this.classList.remove('input-validation-error');
        errorSpan.textContent = '';
    }
});
    function togglePassword(inputId, button) {
            const passwordInput = document.getElementById(inputId);
    const toggleText = button.querySelector('.toggle-text');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
    toggleText.textContent = 'Hide';
            } else {
        passwordInput.type = 'password';
    toggleText.textContent = 'Show';
            }
        }

    document.addEventListener('DOMContentLoaded', function() {
            const form = document.getElementById('resetPasswordForm');
    const resetBtn = document.getElementById('resetBtn');
    const btnText = resetBtn?.querySelector('.btn-text');
    const newPasswordInput = document.getElementById('newPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');

    // Form submission handling
    if (form) {
        form.addEventListener('submit', function (e) {
            // Show loading state
            resetBtn.disabled = true;
            resetBtn.classList.add('loading');
            btnText.innerHTML = '<span class="spinner"></span><b>Resetting...</b>';
        });

    // Reset button state if there are validation errors
    if (document.querySelector('.validation-summary')) {
        resetBtn.disabled = false;
    resetBtn.classList.remove('loading');
    btnText.innerHTML = '<b>Reset Password</b>';
                }
            }

    // Password strength checker
    if (newPasswordInput) {
        newPasswordInput.addEventListener('input', function () {
            checkPasswordStrength(this.value);
        });
            }

    // Confirm password validation
    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('blur', function () {
            const newPassword = newPasswordInput.value;
            const confirmPassword = this.value;
            const errorSpan = this.nextElementSibling;

            if (confirmPassword && newPassword !== confirmPassword) {
                this.classList.add('input-validation-error');
                errorSpan.textContent = 'Passwords do not match';
            } else {
                this.classList.remove('input-validation-error');
                errorSpan.textContent = '';
            }
        });
            }
        });

    function checkPasswordStrength(password) {
            const strengthFill = document.getElementById('strengthFill');
    const strengthText = document.getElementById('strengthText');
    const requirements = document.getElementById('requirements');

    // Reset all requirement indicators
    const reqs = requirements.children;
    for (let req of reqs) {
        req.classList.remove('valid');
            }

    if (!password) {
        strengthFill.className = 'strength-fill';
    strengthText.textContent = 'Password strength';
    return;
            }

    let score = 0;

            // Check length
            if (password.length >= 8) {
        reqs[0].classList.add('valid');
    score++;
            }

    // Check uppercase
    if (/[A-Z]/.test(password)) {
        reqs[1].classList.add('valid');
    score++;
            }

    // Check lowercase
    if (/[a-z]/.test(password)) {
        reqs[2].classList.add('valid');
    score++;
            }

    // Check number
    if (/\d/.test(password)) {
        reqs[3].classList.add('valid');
    score++;
            }

    // Check special character
    if (/[^A-Za-z0-9]/.test(password)) {
        reqs[4].classList.add('valid');
    score++;
            }

    // Update strength indicator
    strengthFill.className = 'strength-fill';
    if (score < 3) {
        strengthFill.classList.add('weak');
    strengthText.textContent = 'Weak password';
            } else if (score < 5) {
        strengthFill.classList.add('medium');
    strengthText.textContent = 'Medium password';
            } else {
        strengthFill.classList.add('strong');
    strengthText.textContent = 'Strong password';
            }
        }
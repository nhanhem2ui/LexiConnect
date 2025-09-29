// Dynamically reload majors when university changes
document.querySelector('select[name="UniversityId"]').addEventListener('change', function () {
    const selectedUniversity = this.value;
    const majorSelect = document.querySelector('select[name="MajorId"]');

    if (!selectedUniversity) {
        majorSelect.innerHTML = '<option value="">Select your major</option>';
        return;
    }

    fetch(`/Home/GetMajorsByUniversity?universityId=${selectedUniversity}`)
        .then(response => response.json())
        .then(majors => {
            // Clear current options
            majorSelect.innerHTML = '<option value="">Select your major</option>';

            // Populate with filtered majors
            majors.forEach(major => {
                const option = document.createElement('option');
                option.value = major.majorId;
                option.textContent = major.name;
                majorSelect.appendChild(option);
            });
        })
        .catch(error => {
            console.error('Error fetching majors:', error);
        });
});

// Avatar preview functionality
document.getElementById('avatar-upload').addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const preview = document.getElementById('avatar-preview');
            if (preview.tagName === 'IMG') {
                preview.src = e.target.result;
            } else {
                // Replace placeholder div with img element
                const img = document.createElement('img');
                img.id = 'avatar-preview';
                img.src = e.target.result;
                img.alt = 'Avatar Preview';
                preview.parentNode.replaceChild(img, preview);
            }
        };
        reader.readAsDataURL(file);
    }
});

// Form validation enhancement
document.querySelector('.edit-profile-form').addEventListener('submit', function (e) {
    const fullName = document.querySelector('input[name="FullName"]');
    const email = document.querySelector('input[name="Email"]');

    if (!fullName.value.trim()) {
        e.preventDefault();
        fullName.focus();
        alert('Full name is required.');
        return false;
    }

    if (!email.value.trim()) {
        e.preventDefault();
        email.focus();
        alert('Email is required.');
        return false;
    }
});
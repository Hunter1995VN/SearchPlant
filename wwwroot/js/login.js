// Eco Wellness Login Form JavaScript - PHIÊN BẢN ĐẦY ĐỦ VÀ ĐÃ SỬA LỖI
class EcoWellnessLoginForm {
    constructor() {
        this.form = document.querySelector('.harmony-form');
        if (!this.form) return;

        // Tìm tất cả các trường input password và nút toggle con mắt trong form
        this.passwordFields = Array.from(this.form.querySelectorAll('input[type="password"]'));
        this.toggleButtons = Array.from(this.form.querySelectorAll('.nature-toggle'));

        this.emailInput = document.getElementById('email');
        this.submitButton = this.form.querySelector('.harmony-button');
        this.successMessage = document.getElementById('successMessage');
        this.socialButtons = document.querySelectorAll('.earth-social');
        this.init();
    }
    
    init() {
        this.bindEvents();
        this.setupAllPasswordToggles();
        this.setupSocialButtons();
        this.setupWellnessEffects();
    }
    
    bindEvents() {
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));
        if (this.emailInput) {
            this.emailInput.addEventListener('blur', () => this.validateEmail());
            this.emailInput.addEventListener('input', () => this.clearError('email'));
            this.emailInput.setAttribute('placeholder', ' ');
        }
        // Thêm hiệu ứng cho tất cả các trường password
        this.passwordFields.forEach(input => {
            input.addEventListener('focus', (e) => {
                const field = e.target.closest('.organic-field');
                if (field) this.triggerMindfulEffect(field);
            });
            input.addEventListener('blur', (e) => {
                const field = e.target.closest('.organic-field');
                if (field) this.resetMindfulEffect(field);
            });
        });
    }
    
    setupAllPasswordToggles() {
        // Tìm tất cả các cặp input và nút toggle trong form
        this.toggleButtons.forEach(toggleBtn => {
            const field = toggleBtn.closest('.organic-field');
            if (!field) return;
            // Luôn lấy input đầu tiên trong .organic-field
            const input = field.querySelector('input');
            if (!input) return;
            toggleBtn.addEventListener('click', () => {
                const type = input.type === 'password' ? 'text' : 'password';
                input.type = type;
                toggleBtn.classList.toggle('toggle-visible', type === 'text');
            });
        });
    }
    
    setupSocialButtons() {
        if (this.socialButtons.length > 0) {
            this.socialButtons.forEach(button => {
                button.addEventListener('click', (e) => {
                    const provider = button.querySelector('span').textContent.trim();
                    this.handleSocialLogin(provider, button);
                });
            });
        }
    }
    
    setupWellnessEffects() {
        // Lọc ra các input thực sự tồn tại trên trang trước khi thêm hiệu ứng.
        const existingInputs = [this.emailInput, this.passwordInput].filter(input => input !== null);
        
        existingInputs.forEach(input => {
            input.addEventListener('focus', (e) => {
                const field = e.target.closest('.organic-field');
                if (field) this.triggerMindfulEffect(field);
            });
            
            input.addEventListener('blur', (e) => {
                const field = e.target.closest('.organic-field');
                if (field) this.resetMindfulEffect(field);
            });
        });
    }
    
    triggerMindfulEffect(field) {
        const fieldNature = field.querySelector('.field-nature');
        if (fieldNature) {
            fieldNature.style.animation = 'gentleBreath 3s ease-in-out infinite';
        }
    }
    
    resetMindfulEffect(field) {
        const fieldNature = field.querySelector('.field-nature');
        if (fieldNature) {
            fieldNature.style.animation = '';
        }
    }
    
    validateEmail() {
        // Nếu trang không có ô email, coi như hợp lệ.
        if (!this.emailInput) return true;

        const email = this.emailInput.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        
        if (!email) {
            this.showError('email', 'Your email helps us connect with you mindfully');
            return false;
        }
        
        if (!emailRegex.test(email)) {
            this.showError('email', 'Please share a valid email address');
            return false;
        }
        
        this.clearError('email');
        return true;
    }
    
    validatePassword() {
        // Nếu trang không có ô mật khẩu, coi như hợp lệ.
        if (!this.passwordInput) return true;

        const password = this.passwordInput.value;
        
        if (!password) {
            this.showError('password', 'Your sanctuary needs a protective key');
            return false;
        }
        
        if (password.length < 6) {
            this.showError('password', 'Please choose a stronger protection (6+ characters)');
            return false;
        }
        
        this.clearError('password');
        return true;
    }
    
    showError(field, message) {
        const inputElement = document.getElementById(field);
        if (!inputElement) return;
        
        const organicField = inputElement.closest('.organic-field');
        const errorElement = document.getElementById(`${field}Error`);
        
        if (organicField && errorElement) {
            organicField.classList.add('error');
            errorElement.textContent = message;
            errorElement.classList.add('show');
        }
    }
    
    clearError(field) {
        const inputElement = document.getElementById(field);
        if (!inputElement) return;

        const organicField = inputElement.closest('.organic-field');
        const errorElement = document.getElementById(`${field}Error`);
        
        if (organicField && errorElement) {
            organicField.classList.remove('error');
            errorElement.classList.remove('show');
            setTimeout(() => {
                errorElement.textContent = '';
            }, 300);
        }
    }
    
    async handleSubmit(e) {
        const isEmailValid = this.validateEmail();
        const isPasswordValid = this.validatePassword();

        // Chỉ ngăn chặn việc gửi form nếu validation ở phía client thất bại.
        if (!isEmailValid || !isPasswordValid) {
            e.preventDefault();
            return;
        }
        // Nếu không, cứ để form được submit theo cách mặc định của Razor Page.
    }

    async handleSocialLogin(provider, button) {
        console.log(`Connecting with ${provider} mindfully...`);
        
        const originalHTML = button.innerHTML;
        button.style.pointerEvents = 'none';
        button.style.opacity = '0.7';
        
        const loadingHTML = `
            <div class="social-earth"></div>
            <div style="display: flex; gap: 4px;">
                <div style="width: 6px; height: 6px; background: #4caf50; border-radius: 50%; animation: organicGrow 1.5s ease-in-out infinite;"></div>
                <div style="width: 6px; height: 6px; background: #4caf50; border-radius: 50%; animation: organicGrow 1.5s ease-in-out infinite; animation-delay: 0.2s;"></div>
                <div style="width: 6px; height: 6px; background: #4caf50; border-radius: 50%; animation: organicGrow 1.5s ease-in-out infinite; animation-delay: 0.4s;"></div>
            </div>
            <span>Connecting...</span>
            <div class="social-glow"></div>
        `;
        
        button.innerHTML = loadingHTML;
        
        try {
            await new Promise(resolve => setTimeout(resolve, 2200));
            console.log(`Redirecting to ${provider} wellness connection...`);
            // window.location.href = `/auth/${provider.toLowerCase()}`;
        } catch (error) {
            console.error(`${provider} connection was interrupted: ${error.message}`);
        } finally {
            button.style.pointerEvents = 'auto';
            button.style.opacity = '1';
            button.innerHTML = originalHTML;
        }
    }
    
    setLoading(loading) {
        if (!this.submitButton) return;
        this.submitButton.classList.toggle('loading', loading);
        this.submitButton.disabled = loading;
        
        this.socialButtons.forEach(button => {
            button.style.pointerEvents = loading ? 'none' : 'auto';
            button.style.opacity = loading ? '0.6' : '1';
        });
    }
    
    showHarmonySuccess() {
        if (!this.form || !this.successMessage) return;

        this.form.style.transform = 'scale(0.95)';
        this.form.style.opacity = '0';
        
        setTimeout(() => {
            this.form.style.display = 'none';
            const socialSection = document.querySelector('.natural-social');
            const signupSection = document.querySelector('.nurture-signup');
            const divider = document.querySelector('.balance-divider');

            if(socialSection) socialSection.style.display = 'none';
            if(signupSection) signupSection.style.display = 'none';
            if(divider) divider.style.display = 'none';
            
            this.successMessage.classList.add('show');
            
        }, 300);
        
        setTimeout(() => {
            console.log('Welcome to your wellness sanctuary...');
            // window.location.href = '/wellness-dashboard';
        }, 3500);
    }
}

// Thêm các keyframes animation vào CSS một cách tự động
if (!document.querySelector('#wellness-keyframes')) {
    const style = document.createElement('style');
    style.id = 'wellness-keyframes';
    style.textContent = `
        @keyframes gentleBreath {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.01); }
        }
        @keyframes organicGrow {
            0%, 100% { transform: scale(0.5); opacity: 0.5; }
            50% { transform: scale(1); opacity: 1; }
        }
    `;
    document.head.appendChild(style);
}

// Khởi tạo form khi trang đã tải xong
document.addEventListener('DOMContentLoaded', () => {
    // Chỉ khởi tạo class nếu trang này có một form với class .harmony-form
    if (document.querySelector('.harmony-form')) {
        new EcoWellnessLoginForm();
    }
});
// Plant Diagnosis Page JavaScript - Natural & Interactive
class PlantDiagnosisSystem {
    constructor() {
        this.form = document.getElementById('diagnosisForm');
        this.fileInput = document.getElementById('imageInput');
        this.uploadZone = document.querySelector('.upload-zone');
        this.previewContainer = document.getElementById('imagePreview');
        this.previewImage = document.getElementById('preview');
        this.submitButton = document.getElementById('submitBtn');
        this.isDragOver = false;

        this.init();
    }

    init() {
        console.log('PlantDiagnosisSystem initialized');
        this.bindEvents();
        this.createBackgroundElements();
        this.setupAnimations();
    }

    bindEvents() {
        // File input events
        if (this.fileInput) {
            this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        }

        // Drag and drop events
        if (this.uploadZone) {
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                this.uploadZone.addEventListener(eventName, (e) => this.preventDefaults(e), false);
            });

            ['dragenter', 'dragover'].forEach(eventName => {
                this.uploadZone.addEventListener(eventName, () => this.handleDragEnter(), false);
            });

            ['dragleave', 'drop'].forEach(eventName => {
                this.uploadZone.addEventListener(eventName, () => this.handleDragLeave(), false);
            });

            this.uploadZone.addEventListener('drop', (e) => this.handleDrop(e), false);
            this.uploadZone.addEventListener('click', () => this.triggerFileInput(), false);
        }

        // Form submission
        if (this.form) {
            this.form.addEventListener('submit', (e) => this.handleSubmit(e));
        }

        // Image preview interactions
        if (this.previewImage) {
            console.log('Binding click event to preview image');
            this.previewImage.addEventListener('click', () => this.enlargeImage());
        } else {
            console.log('Preview image not found');
        }

        // Add click handlers for result images
        this.addResultImageZoom();
    }

    addResultImageZoom() {
        // Add click handlers for all result images
        const previewResults = document.querySelectorAll('.result-card .preview-image');
        console.log('Found full preview images:', previewResults.length);
        previewResults.forEach(img => {
            img.addEventListener('click', () => this.enlargeResultImage(img.src));
            img.style.cursor = 'pointer';
        });

        // Add click handlers for thumbnail galleries
        const thumbnails = document.querySelectorAll('.result-thumbnail');
        console.log('Found thumbnail images:', thumbnails.length);
        thumbnails.forEach(img => {
            const fullSrc = img.dataset.fullSrc || img.src;
            img.addEventListener('click', () => this.enlargeResultImage(fullSrc));
            img.style.cursor = 'pointer';
        });
    }

    preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    handleDragEnter() {
        if (!this.isDragOver) {
            this.isDragOver = true;
            this.uploadZone.classList.add('drag-over');
        }
    }

    handleDragLeave() {
        this.isDragOver = false;
        setTimeout(() => {
            if (!this.isDragOver) {
                this.uploadZone.classList.remove('drag-over');
            }
        }, 100);
    }

    handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;

        if (files.length > 0) {
            this.fileInput.files = files;
            this.handleFileSelect({ target: this.fileInput });
        }
    }

    triggerFileInput() {
        if (this.fileInput) {
            this.fileInput.click();
        }
    }

    handleFileSelect(e) {
        const file = e.target.files[0];
        if (file) {
            // Validate file
            if (!this.validateFile(file)) {
                return;
            }

            this.updateUploadZone(file);
            this.showImagePreview(file);
        }
    }

    validateFile(file) {
        // Check file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png'];
        if (!allowedTypes.includes(file.type)) {
            this.showAlert('Chỉ chấp nhận file ảnh JPG, JPEG, PNG!', 'danger');
            return false;
        }

        // Check file size (10MB max)
        const maxSize = 10 * 1024 * 1024; // 10MB
        if (file.size > maxSize) {
            this.showAlert('File quá lớn! Tối đa 10MB.', 'danger');
            return false;
        }

        return true;
    }

    updateUploadZone(file) {
        const uploadText = this.uploadZone.querySelector('.upload-text');
        const uploadHint = this.uploadZone.querySelector('.upload-hint');

        if (uploadText && uploadHint) {
            uploadText.textContent = `Đã chọn: ${file.name}`;
            uploadHint.textContent = `Kích thước: ${(file.size / 1024 / 1024).toFixed(2)} MB`;
        }
    }

    showImagePreview(file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            if (this.previewImage) {
                this.previewImage.src = e.target.result;
            }
            if (this.previewContainer) {
                this.previewContainer.style.display = 'block';
            }
        };
        reader.readAsDataURL(file);
    }

    resetFileInput() {
        if (this.fileInput) {
            this.fileInput.value = '';
        }
        if (this.previewContainer) {
            this.previewContainer.style.display = 'none';
        }

        const uploadText = this.uploadZone.querySelector('.upload-text');
        const uploadHint = this.uploadZone.querySelector('.upload-hint');

        if (uploadText && uploadHint) {
            uploadText.textContent = 'Chọn ảnh cây trồng';
            uploadHint.textContent = 'hoặc kéo thả file vào đây';
        }
    }

    handleSubmit(e) {
        e.preventDefault();

        if (!this.fileInput || !this.fileInput.files[0]) {
            this.showAlert('Vui lòng chọn ảnh trước khi chẩn đoán!', 'warning');
            return;
        }

        this.showLoadingOverlay();
        this.form.submit();
    }

    showLoadingOverlay() {
        const overlay = document.createElement('div');
        overlay.id = 'loadingOverlay';
        overlay.innerHTML = `
            <div class="loading-content">
                <div class="spinner-border text-success" role="status">
                    <span class="sr-only">Loading...</span>
                </div>
                <h5>Đang chẩn đoán...</h5>
                <p>Vui lòng đợi trong giây lát</p>
            </div>
        `;

        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.8);
            backdrop-filter: blur(5px);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
        `;

        const style = document.createElement('style');
        style.textContent = `
            .loading-content {
                text-align: center;
                color: white;
                background: rgba(255, 255, 255, 0.1);
                padding: 2rem;
                border-radius: 16px;
                backdrop-filter: blur(10px);
                border: 1px solid rgba(255, 255, 255, 0.2);
            }
            .loading-content .spinner-border {
                width: 3rem;
                height: 3rem;
                margin-bottom: 1rem;
            }
        `;

        document.head.appendChild(style);
        document.body.appendChild(overlay);
    }

    enlargeImage() {
        console.log('enlargeImage called');
        // Tạo modal với animation đẹp
        const modal = document.createElement('div');
        modal.id = 'imageModal';
        modal.innerHTML = `
            <div class="modal-backdrop" onclick="this.parentElement.remove()"></div>
            <div class="modal-content">
                <img src="${this.previewImage.src}" alt="Enlarged preview" />
                <button class="close-btn" onclick="this.parentElement.parentElement.remove()">&times;</button>
            </div>
        `;

        // Style cho modal
        modal.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 10000;
            display: flex;
            align-items: center;
            justify-content: center;
            opacity: 0;
            animation: modalFadeIn 0.3s ease-out forwards;
        `;

        // Thêm CSS cho modal
        const style = document.createElement('style');
        style.textContent = `
            .modal-backdrop {
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.85);
                backdrop-filter: blur(8px);
                animation: backdropFadeIn 0.3s ease-out;
            }
            .modal-content {
                position: relative;
                max-width: 95vw;
                max-height: 95vh;
                z-index: 10001;
                animation: imageZoomIn 0.4s ease-out;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            .modal-content img {
                max-width: 100%;
                max-height: 100%;
                border-radius: 16px;
                box-shadow: 0 25px 50px rgba(0, 0, 0, 0.4);
                object-fit: contain;
                transition: transform 0.3s ease;
            }
            .modal-content img:hover {
                transform: scale(1.05);
            }
            .close-btn {
                position: absolute;
                top: -20px;
                right: -20px;
                width: 40px;
                height: 40px;
                border-radius: 50%;
                background: #fff;
                border: none;
                font-size: 24px;
                font-weight: bold;
                cursor: pointer;
                box-shadow: 0 8px 20px rgba(0, 0, 0, 0.3);
                display: flex;
                align-items: center;
                justify-content: center;
                color: #333;
                transition: all 0.3s ease;
                z-index: 10002;
            }
            .close-btn:hover {
                background: #f8f9fa;
                transform: scale(1.1);
                box-shadow: 0 10px 25px rgba(0, 0, 0, 0.4);
            }
            @keyframes modalFadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }
            @keyframes backdropFadeIn {
                from { backdrop-filter: blur(0px); background: rgba(0, 0, 0, 0); }
                to { backdrop-filter: blur(8px); background: rgba(0, 0, 0, 0.85); }
            }
            @keyframes imageZoomIn {
                from {
                    opacity: 0;
                    transform: scale(0.8) rotate(-5deg);
                }
                to {
                    opacity: 1;
                    transform: scale(1) rotate(0deg);
                }
            }
        `;

        document.head.appendChild(style);
        document.body.appendChild(modal);

        // Thêm event listener để đóng modal khi nhấn ESC
        const closeModal = () => {
            modal.style.animation = 'modalFadeOut 0.3s ease-out forwards';
            setTimeout(() => {
                if (modal.parentElement) {
                    modal.parentElement.removeChild(modal);
                }
                if (style.parentElement) {
                    style.parentElement.removeChild(style);
                }
            }, 300);
        };

        // Đóng modal khi nhấn ESC
        const handleKeyPress = (e) => {
            if (e.key === 'Escape') {
                closeModal();
                document.removeEventListener('keydown', handleKeyPress);
            }
        };
        document.addEventListener('keydown', handleKeyPress);

        // Đóng modal khi click backdrop
        modal.querySelector('.modal-backdrop').addEventListener('click', closeModal);
    }

    enlargeResultImage(imageSrc) {
        // Same modal logic but for result images
        const modal = document.createElement('div');
        modal.id = 'resultImageModal';
        modal.innerHTML = `
            <div class="modal-backdrop" onclick="this.parentElement.remove()"></div>
            <div class="modal-content">
                <img src="${imageSrc}" alt="Enlarged result" />
                <button class="close-btn" onclick="this.parentElement.parentElement.remove()">&times;</button>
            </div>
        `;

        // Style cho modal
        modal.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 10000;
            display: flex;
            align-items: center;
            justify-content: center;
            opacity: 0;
            animation: modalFadeIn 0.3s ease-out forwards;
        `;

        // Thêm CSS cho modal
        const style = document.createElement('style');
        style.textContent = `
            .modal-backdrop {
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.85);
                backdrop-filter: blur(8px);
                animation: backdropFadeIn 0.3s ease-out;
            }
            .modal-content {
                position: relative;
                max-width: 95vw;
                max-height: 95vh;
                z-index: 10001;
                animation: imageZoomIn 0.4s ease-out;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            .modal-content img {
                max-width: 100%;
                max-height: 100%;
                border-radius: 16px;
                box-shadow: 0 25px 50px rgba(0, 0, 0, 0.4);
                object-fit: contain;
                transition: transform 0.3s ease;
            }
            .modal-content img:hover {
                transform: scale(1.05);
            }
            .close-btn {
                position: absolute;
                top: -20px;
                right: -20px;
                width: 40px;
                height: 40px;
                border-radius: 50%;
                background: #fff;
                border: none;
                font-size: 24px;
                font-weight: bold;
                cursor: pointer;
                box-shadow: 0 8px 20px rgba(0, 0, 0, 0.3);
                display: flex;
                align-items: center;
                justify-content: center;
                color: #333;
                transition: all 0.3s ease;
                z-index: 10002;
            }
            .close-btn:hover {
                background: #f8f9fa;
                transform: scale(1.1);
                box-shadow: 0 10px 25px rgba(0, 0, 0, 0.4);
            }
            @keyframes modalFadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }
            @keyframes backdropFadeIn {
                from { backdrop-filter: blur(0px); background: rgba(0, 0, 0, 0); }
                to { backdrop-filter: blur(8px); background: rgba(0, 0, 0, 0.85); }
            }
            @keyframes imageZoomIn {
                from {
                    opacity: 0;
                    transform: scale(0.8) rotate(-5deg);
                }
                to {
                    opacity: 1;
                    transform: scale(1) rotate(0deg);
                }
            }
        `;

        document.head.appendChild(style);
        document.body.appendChild(modal);

        // Thêm event listener để đóng modal khi nhấn ESC
        const closeModal = () => {
            modal.style.animation = 'modalFadeOut 0.3s ease-out forwards';
            setTimeout(() => {
                if (modal.parentElement) {
                    modal.parentElement.removeChild(modal);
                }
                if (style.parentElement) {
                    style.parentElement.removeChild(style);
                }
            }, 300);
        };

        // Đóng modal khi nhấn ESC
        const handleKeyPress = (e) => {
            if (e.key === 'Escape') {
                closeModal();
                document.removeEventListener('keydown', handleKeyPress);
            }
        };
        document.addEventListener('keydown', handleKeyPress);

        // Đóng modal khi click backdrop
        modal.querySelector('.modal-backdrop').addEventListener('click', closeModal);
    }

    createBackgroundElements() {
        // Create floating leaves and flowers for background animation
        const container = document.querySelector('.global-background') || document.body;

        // Create leaves
        for (let i = 1; i <= 5; i++) {
            const leaf = document.createElement('div');
            leaf.className = `floating-leaf leaf-${i}`;
            leaf.innerHTML = '🍃';
            container.appendChild(leaf);
        }

        // Create flowers
        for (let i = 1; i <= 3; i++) {
            const flower = document.createElement('div');
            flower.className = `floating-flower flower-${i}`;
            flower.innerHTML = '🌸';
            container.appendChild(flower);
        }
    }

    setupAnimations() {
        // Setup any additional animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.animationPlayState = 'running';
                }
            });
        }, observerOptions);

        // Observe floating elements
        document.querySelectorAll('.floating-leaf, .floating-flower').forEach(el => {
            observer.observe(el);
        });
    }

    showAlert(message, type = 'info') {
        const alertContainer = document.createElement('div');
        alertContainer.innerHTML = `
            <div class="alert alert-${type} alert-dismissible fade show status-alert" role="alert">
                <i class="bi bi-info-circle-fill me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        // Insert at the top of the diagnosis body
        const diagnosisBody = document.querySelector('.diagnosis-body');
        if (diagnosisBody) {
            diagnosisBody.insertBefore(alertContainer.firstElementChild, diagnosisBody.firstElementChild);
        }

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            const alert = alertContainer.querySelector('.alert');
            if (alert) {
                alert.classList.remove('show');
                setTimeout(() => alert.remove(), 150);
            }
        }, 5000);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new PlantDiagnosisSystem();
});
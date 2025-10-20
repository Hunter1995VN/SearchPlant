// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Navbar dropdown (Bootstrap 5: tự động, nhưng bạn có thể thêm hiệu ứng nếu muốn)
document.addEventListener('DOMContentLoaded', function(){
    // Sidebar active highlight (nếu cần tự động chuyển active)
    document.querySelectorAll('.admin-sidebar .nav-link').forEach(function(link){
        if (link.href === location.origin + location.pathname) {
            link.classList.add('active');
        }
    });

    // Tìm kiếm: ngăn submit nếu không nhập gì
    var searchBox = document.querySelector('.search-box');
    if(searchBox) {
        searchBox.addEventListener('submit', function(e){
            var input = searchBox.querySelector('input[type="search"],input[type="text"]');
            if(input && !input.value.trim()) e.preventDefault();
        });
    }
});


//Search
function confirmDelete(id, name) {
    Swal.fire({
        title: 'Xác nhận xóa?',
        html: 'Bạn có chắc chắn muốn xóa cây <b>' + name + '</b>?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.href = '/Plant/Delete/' + id;
        }
    });
}

/* =================================================================== */
/* ===== SCRIPT FOR SWEETALERT DELETE CONFIRMATION             ===== */
/* =================================================================== */
// Chức năng này sẽ tìm tất cả các form có class 'delete-form' 
// và hiển thị hộp thoại xác nhận trước khi gửi đi.
function initializeDeleteConfirmation() {
    const deleteForms = document.querySelectorAll('.delete-form');
    
    deleteForms.forEach(form => {
        form.addEventListener('submit', function (event) {
            // Chỉ thực hiện nếu chưa được xử lý
            if (form.dataset.sweetalert) return;

            event.preventDefault(); // Ngăn form submit ngay lập tức
            
            Swal.fire({
                title: 'Bạn có chắc chắn muốn xóa?',
                text: "Dữ liệu đã xóa không thể khôi phục!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Xác nhận xóa',
                cancelButtonText: 'Hủy bỏ',
                customClass: {
                    confirmButton: 'btn btn-danger mx-1',
                    cancelButton: 'btn btn-secondary mx-1'
                },
                buttonsStyling: false
            }).then((result) => {
                if (result.isConfirmed) {
                    form.dataset.sweetalert = true; // Đánh dấu đã xử lý để tránh lặp
                    form.submit(); // Nếu người dùng xác nhận, submit form
                }
            });
        });
    });
}

// Gọi hàm khi trang tải xong
// Nếu bạn dùng turbolinks hoặc các thư viện SPA khác, bạn có thể cần gọi hàm này sau mỗi lần chuyển trang
document.addEventListener('DOMContentLoaded', initializeDeleteConfirmation);


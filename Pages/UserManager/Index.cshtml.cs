using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models; // Đảm bảo bạn đã using namespace của model
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchPlant.Pages.UsersManager
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly SearchPlant.Models.SearchPlantContext _context;

        public IndexModel(SearchPlant.Models.SearchPlantContext context)
        {
            _context = context;
        }

        public IList<Appuser> Users { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CurrentFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }
        public async Task OnGetAsync()
        {
            // Bắt đầu với một IQueryable, điều này rất hiệu quả vì các điều kiện lọc
            // sẽ được gộp vào một câu lệnh SQL duy nhất.
            IQueryable<Appuser> usersQuery = _context.Appusers.AsQueryable();

            // Lọc theo chuỗi tìm kiếm (tên hoặc email)
            if (!string.IsNullOrEmpty(CurrentFilter))
            {
                var SearchString = CurrentFilter.ToLower();
                usersQuery = usersQuery.Where(u => u.Username.ToLower().Contains(SearchString) ||
                u.Email.ToLower().Contains(SearchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                usersQuery = usersQuery.Where(u => u.Status == StatusFilter);
            }

            // Thực thi truy vấn và lấy dữ liệu từ database
            Users = await usersQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(int userId, string newStatus)
        {
            // Tìm người dùng trong database bằng Primary Key
            var userToUpdate = await _context.Appusers.FindAsync(userId);

            if (userToUpdate != null)
            {
                // Cập nhật trạng thái
                userToUpdate.Status = newStatus;
                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();
            }

            // Tải lại trang với các tham số lọc hiện tại để người dùng không bị mất ngữ cảnh
            return RedirectToPage(new { CurrentFilter = this.CurrentFilter, StatusFilter = this.StatusFilter });
        }
    }
}
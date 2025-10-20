using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SearchPlant.Models;
using SearchPlant.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SearchPlant.Pages.Plants
{
    public class DetailsModel : PageModel
    {
        private readonly SearchPlantContext _context;
        private readonly ILogger<DetailsModel> _logger;
        private readonly ICompositeViewEngine _viewEngine;

        public DetailsModel(SearchPlantContext context, ILogger<DetailsModel> logger, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _logger = logger;
            _viewEngine = viewEngine;
        }

        public Plant? Plant { get; set; }
        public string? ReturnUrl { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public bool IsFavoritedByUser { get; set; }

        [BindProperty]
        public string? NewCommentContent { get; set; }

        public string? CurrentUserAvatarUrl { get; set; }

        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int CurrentUserRating { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id, string? returnUrl)
        {
            ReturnUrl = string.IsNullOrEmpty(returnUrl) ? Url.Page("/Search/Index") : returnUrl;
            if (id == null) return NotFound();

            Plant = await _context.Plants
                .Include(p => p.Cycle).Include(p => p.Types).Include(p => p.Regions).Include(p => p.Seasons)
                .Include(p => p.Soils).Include(p => p.Fertilizers).Include(p => p.Diseases)
                .Include(p => p.Properties).Include(p => p.Waterings).Include(p => p.Statuses)
                .FirstOrDefaultAsync(p => p.Plantid == id);

            if (Plant == null) return NotFound();
            var ratings = await _context.Ratings // Sử dụng model 'Rating' của bạn
                                           .Where(r => r.Plantid == Plant.Plantid)
                                           .ToListAsync();
            if (User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdString, out int userId))
                {
                    // Kiểm tra trong bảng UserFavorites xem có bản ghi nào khớp không
                    IsFavoritedByUser = await _context.UserFavorites
                        .AnyAsync(f => f.Plantid == id && f.Userid == userId);
                }
            }
            else
            {
                IsFavoritedByUser = false;
            }

            if (ratings.Any())
            {
                AverageRating = ratings.Average(r => r.RatingValue);
                TotalRatings = ratings.Count;
            }

            // Kiểm tra xem người dùng hiện tại đã đánh giá chưa
            if (User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out var userId))
                {
                    var userRating = ratings.FirstOrDefault(r => r.Userid == userId);
                    CurrentUserRating = userRating?.RatingValue ?? 0; // Gán 0 nếu chưa có
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out var userId))
                {
                    var currentUser = await _context.Appusers.FindAsync(userId);
                    CurrentUserAvatarUrl = currentUser?.Avatarurl;

                }
            }

            await LoadComments(Plant.Plantid);
            return Page();
        }

        public async Task<IActionResult> OnPostNewCommentAsync(int plantId, int? parentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                _logger.LogError("Không tìm thấy UserID trong Claims. Chuyển hướng về trang Login.");
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(NewCommentContent))
            {
                ModelState.AddModelError("NewCommentContent", "Nội dung bình luận không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Nội dung bình luận không hợp lệ." }) { StatusCode = 400 };
                }
                await OnGetAsync(plantId, ReturnUrl);
                return Page();
            }

            var comment = new Comment
            {
                Plantid = plantId,
                Userid = userId,
                CommentText = NewCommentContent,
                ParentCommentId = parentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            _logger.LogWarning("--- Gửi bình luận THÀNH CÔNG ---");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                await _context.Entry(comment).Reference(c => c.User).LoadAsync();

                var viewModel = new CommentViewModel
                {
                    Id = comment.Id,
                    Content = comment.CommentText,
                    Username = comment.User?.Username ?? "Người dùng ẩn",
                    AvatarUrl = comment.User?.Avatarurl,
                    CreatedAtFormatted = "Vừa xong",
                    Likes = 0,
                    Dislikes = 0,
                    CanDelete = true
                };

                string commentHtml = await RenderPartialViewToStringAsync("_CommentItem", viewModel);
                return new JsonResult(new { success = true, html = commentHtml });
            }

            return RedirectToPage(new { id = plantId });
        }

        public async Task<IActionResult> OnPostReactionAsync(int commentId, short reactionType)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var existingReaction = await _context.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == commentId && r.Userid == userId);

            if (existingReaction != null)
            {
                if (existingReaction.ReactionType == reactionType) _context.CommentReactions.Remove(existingReaction);
                else existingReaction.ReactionType = reactionType;
            }
            else
            {
                _context.CommentReactions.Add(new CommentReaction { CommentId = commentId, Userid = userId, ReactionType = reactionType });
            }

            await _context.SaveChangesAsync();
            var likes = await _context.CommentReactions.CountAsync(r => r.CommentId == commentId && r.ReactionType == 1);
            var dislikes = await _context.CommentReactions.CountAsync(r => r.CommentId == commentId && r.ReactionType == -1);
            return new JsonResult(new { success = true, likes, dislikes });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var currentUserId))
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập." }) { StatusCode = 401 };
            }

            var commentToDelete = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);

            if (commentToDelete == null)
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy bình luận." }) { StatusCode = 404 };
            }

            bool isOwner = commentToDelete.Userid == currentUserId;
            bool isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin)
            {
                _logger.LogError($"Người dùng {currentUserId} không có quyền xóa bình luận {commentId}.");
                return new JsonResult(new { success = false, message = "Bạn không có quyền thực hiện hành động này." }) { StatusCode = 403 };
            }

            var replies = await _context.Comments.Where(c => c.ParentCommentId == commentId).ToListAsync();
            if (replies.Any())
            {
                _context.Comments.RemoveRange(replies);
            }

            _context.Comments.Remove(commentToDelete);
            await _context.SaveChangesAsync();

            _logger.LogWarning($"Bình luận {commentId} đã được xóa thành công bởi người dùng {currentUserId}.");
            return new JsonResult(new { success = true });
        }

        private async Task LoadComments(int plantId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var currentUserId);

            var allComments = await _context.Comments
                .Where(c => c.Plantid == plantId)
                .Include(c => c.User)
                .Include(c => c.CommentReactions)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var commentViewModels = allComments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                ParentId = c.ParentCommentId,
                Content = c.CommentText,
                Username = c.User?.Username ?? "Người dùng ẩn",
                AvatarUrl = c.User?.Avatarurl,
                CreatedAtFormatted = c.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Không rõ",
                Likes = c.CommentReactions.Count(r => r.ReactionType == 1),
                Dislikes = c.CommentReactions.Count(r => r.ReactionType == -1),
                CanDelete = (currentUserId != 0 && c.Userid == currentUserId) || User.IsInRole("Admin"),
                CurrentUserReaction = currentUserId == 0 ? null : c.CommentReactions.FirstOrDefault(r => r.Userid == currentUserId)?.ReactionType
            }).ToList();

            var commentDict = commentViewModels.ToDictionary(c => c.Id);
            var nestedComments = new List<CommentViewModel>();

            foreach (var comment in commentViewModels)
            {
                if (comment.ParentId.HasValue && commentDict.ContainsKey(comment.ParentId.Value))
                {
                    commentDict[comment.ParentId.Value].Replies.Add(comment);
                }
                else
                {
                    nestedComments.Add(comment);
                }
            }
            Comments = nestedComments;
        }

        // Phương thức helper để render một Partial View thành chuỗi HTML
        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            var partialViewPath = $"~/Pages/Shared/{viewName}.cshtml";

            var actionContext = new ActionContext(
                this.HttpContext,
                this.RouteData,
                this.PageContext.ActionDescriptor
            );

            ViewEngineResult viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: partialViewPath, isMainPage: false);

            if (viewResult.View == null)
            {
                _logger.LogError($"Không tìm thấy Partial View tại đường dẫn: {partialViewPath}");
                throw new ArgumentNullException($"{partialViewPath} does not match any available view");
            }

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }

        public async Task<IActionResult> OnPostRatePlantAsync(int plantId, short ratingValue)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập để đánh giá." }) { StatusCode = 401 };
            }

            // SỬA ĐỔI: Cho phép giá trị 0, chỉ chặn các giá trị âm hoặc lớn hơn 5
            if (ratingValue < 0 || ratingValue > 5)
            {
                return new JsonResult(new { success = false, message = "Giá trị đánh giá không hợp lệ." }) { StatusCode = 400 };
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.Plantid == plantId && r.Userid == userId);

            // THÊM MỚI: Logic xử lý cho việc HỦY hoặc ĐÁNH GIÁ
            if (ratingValue == 0)
            {
                // Nếu người dùng gửi 0 (hủy đánh giá) và có đánh giá cũ -> Xóa nó đi
                if (existingRating != null)
                {
                    _context.Ratings.Remove(existingRating);
                }
            }
            else
            {
                // Logic cũ của bạn: Cập nhật hoặc Thêm mới đánh giá
                if (existingRating != null)
                {
                    // Nếu đã tồn tại -> Cập nhật
                    existingRating.RatingValue = ratingValue;
                }
                else
                {
                    // Nếu chưa tồn tại -> Thêm mới
                    var newRating = new Rating
                    {
                        Plantid = plantId,
                        Userid = userId,
                        RatingValue = ratingValue,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Ratings.Add(newRating);
                }
            }

            await _context.SaveChangesAsync();

            // Tính toán lại điểm trung bình và trả về cho client (giữ nguyên)
            var allRatings = await _context.Ratings.Where(r => r.Plantid == plantId).ToListAsync();
            double newAverage = 0;
            if (allRatings.Any()) // Kiểm tra để tránh lỗi chia cho 0
            {
                newAverage = allRatings.Average(r => r.RatingValue);
            }
            var newTotal = allRatings.Count;

            return new JsonResult(new
            {
                success = true,
                averageRating = newAverage.ToString("0.0"), // Định dạng lại cho đẹp hơn
                totalRatings = newTotal
            });
        }


    }
}
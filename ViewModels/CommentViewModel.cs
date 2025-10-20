using System.Collections.Generic;

namespace SearchPlant.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string? Content { get; set; }
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; } // Cho phép null nếu người dùng không có avatar
        public string CreatedAtFormatted { get; set; }

            public bool CanDelete { get; set; }

        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public short? CurrentUserReaction { get; set; } // Phản ứng của người dùng hiện tại (1, -1, or null)
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }
}
using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Plantid { get; set; }

    public int? ParentCommentId { get; set; }

    public string CommentText { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Comment? ParentComment { get; set; }

    public virtual Plant Plant { get; set; } = null!;

    public virtual Appuser User { get; set; } = null!;
}

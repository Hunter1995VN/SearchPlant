using System;
using System.Collections.Generic;

namespace SearchPlant.Models;

public partial class CommentReaction
{
    public int Id { get; set; }

    public int CommentId { get; set; }

    public int Userid { get; set; }

    public short ReactionType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Comment Comment { get; set; } = null!;

    public virtual Appuser User { get; set; } = null!;
}

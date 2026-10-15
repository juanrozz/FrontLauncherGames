using System;
using System.Collections.Generic;

namespace GameLauncher.Models;

public partial class PlayerCommentsJdr
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? ParentId { get; set; }

    public string? Title { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<CommentLikesJdr> CommentLikesJdrs { get; set; } = new List<CommentLikesJdr>();

    public virtual ICollection<PlayerCommentsJdr> InverseParent { get; set; } = new List<PlayerCommentsJdr>();

    public virtual PlayerCommentsJdr? Parent { get; set; }

    public virtual UsersJdr User { get; set; } = null!;
}

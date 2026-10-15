using System;
using System.Collections.Generic;

namespace GameLauncher.Models;

public partial class CommentLikesJdr
{
    public int Id { get; set; }

    public int CommentId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PlayerCommentsJdr Comment { get; set; } = null!;

    public virtual UsersJdr User { get; set; } = null!;
}

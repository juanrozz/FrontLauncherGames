using System;
using System.Collections.Generic;

namespace GameLauncher.Models;

public partial class UsersJdr
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public int DomainId { get; set; }

    public bool? Status { get; set; }

    public string? Name { get; set; }

    public string? Adp { get; set; }

    public virtual ICollection<CommentLikesJdr> CommentLikesJdrs { get; set; } = new List<CommentLikesJdr>();

    public virtual ICollection<PlayerCommentsJdr> PlayerCommentsJdrs { get; set; } = new List<PlayerCommentsJdr>();
}

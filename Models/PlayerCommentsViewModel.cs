namespace GameLauncher.Models
{
	public class PlayerCommentsViewModel
	{
		public List<CommentViewModel> Comments { get; set; } = new();
		public int? CurrentUserId { get; set; }
		public bool IsLoggedIn { get; set; }
	}

	public class CommentViewModel
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string? UserAdp { get; set; }
		public int? ParentId { get; set; }
		public string? Title { get; set; }
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public int LikesCount { get; set; }
		public int RepliesCount { get; set; }
		public bool IsLikedByCurrentUser { get; set; }
		public bool IsOwner { get; set; }
		public List<CommentViewModel> Replies { get; set; } = new();
	}

	public class CreateCommentRequest
	{
		public int? ParentId { get; set; }
		public string? Title { get; set; }
		public string Content { get; set; } = string.Empty;
	}
}
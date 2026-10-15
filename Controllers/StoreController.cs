using GameLauncher.Models;
using GameLauncher.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLauncher.Controllers
{
	public class StoreController : Controller
	{
		private readonly InternsDbContext _context;
		private readonly EmployeePhotoService _photoService;

		public StoreController(InternsDbContext context, EmployeePhotoService photoService)
		{
			_context = context;
			_photoService = photoService;
		}

		// STORE
		public async Task<ActionResult> Index()
		{
			List<GamesJdr> games = await _context.GamesJdrs
				.Where(g => g.Status)
				.OrderBy(g => g.CreatedAt)
				.ToListAsync();

			return View(games);
		}

		public IActionResult Forums()
		{
			return View();
		}

		public IActionResult Documentation()
		{
			return View();
		}

		 //PLAYER COMMENTS 

		// GET: /Store/PlayerComments
		public async Task<IActionResult> PlayerComments()
		{
			var currentUserId = GetCurrentUserId();

			// Traer todos los comentarios activos con sus relaciones
			List<PlayerCommentsJdr> allComments = await _context.PlayerCommentsJdrs
				.Where(c => c.Status)
				.Include(c => c.User)
				.Include(c => c.CommentLikesJdrs)
				.OrderByDescending(c => c.CreatedAt)
				.ToListAsync();

			// Construir solo los comentarios raíz (ParentId == null)
			List<CommentViewModel> rootComments = allComments
				.Where(c => c.ParentId == null)
				.Select(c => BuildCommentTree(c, allComments, currentUserId))
				.ToList();

			PlayerCommentsViewModel viewModel = new PlayerCommentsViewModel
			{
				Comments = rootComments,
				CurrentUserId = currentUserId,
				IsLoggedIn = currentUserId.HasValue
			};

			return View(viewModel);
		}

		// GET: /Store/GetReplies/5
		[HttpGet]
		public async Task<IActionResult> GetReplies(int id)
		{
			var currentUserId = GetCurrentUserId();

			List<PlayerCommentsJdr> allComments = await _context.PlayerCommentsJdrs
				.Where(c => c.Status)
				.Include(c => c.User)
				.Include(c => c.CommentLikesJdrs)
				.ToListAsync();

			PlayerCommentsJdr? comment = allComments.FirstOrDefault(c => c.Id == id);
			if (comment == null)
				return NotFound();

			CommentViewModel commentVm = BuildCommentTree(comment, allComments, currentUserId);

			return Json(commentVm);
		}

		// POST: /Store/CreateComment
		[HttpPost]
		public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
		{
			var currentUserId = GetCurrentUserId();
			if (!currentUserId.HasValue)
				return Unauthorized(new { message = "You must be logged in to comment." });

			if (string.IsNullOrWhiteSpace(request.Content))
				return BadRequest(new { message = "Content is required." });

			// Si es reply, no necesita título
			// Si es raíz, título es opcional pero recomendado
			PlayerCommentsJdr comment = new PlayerCommentsJdr
			{
				UserId = currentUserId.Value,
				ParentId = request.ParentId,
				Title = request.ParentId == null ? request.Title : null,
				Content = request.Content.Trim(),
				CreatedAt = DateTime.Now,
				Status = true
			};

			_context.PlayerCommentsJdrs.Add(comment);
			await _context.SaveChangesAsync();

			// Recargar con navegaciones
			await _context.Entry(comment)
				.Reference(c => c.User)
				.LoadAsync();

			CommentViewModel commentVm = new CommentViewModel
			{
				Id = comment.Id,
				UserId = comment.UserId,
				UserName = comment.User?.Name ?? "Unknown",
				UserAdp = comment.User?.Adp,
				ParentId = comment.ParentId,
				Title = comment.Title,
				Content = comment.Content,
				CreatedAt = comment.CreatedAt,
				LikesCount = 0,
				RepliesCount = 0,
				IsLikedByCurrentUser = false,
				IsOwner = true,
				Replies = new List<CommentViewModel>()
			};

			return Ok(commentVm);
		}

		// POST: /Store/ToggleLike
		[HttpPost]
		public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
		{
			var currentUserId = GetCurrentUserId();
			if (!currentUserId.HasValue)
				return Unauthorized(new { message = "You must be logged in to like." });

			CommentLikesJdr? existingLike = await _context.CommentLikesJdrs
				.FirstOrDefaultAsync(l => l.CommentId == request.CommentId && l.UserId == currentUserId.Value);

			bool isLiked;

			if (existingLike != null)
			{
				// Quitar like
				_context.CommentLikesJdrs.Remove(existingLike);
				isLiked = false;
			}
			else
			{
				// Dar like
				CommentLikesJdr newLike = new CommentLikesJdr
				{
					CommentId = request.CommentId,
					UserId = currentUserId.Value,
					CreatedAt = DateTime.Now
				};
				_context.CommentLikesJdrs.Add(newLike);
				isLiked = true;
			}

			await _context.SaveChangesAsync();

			var likesCount = await _context.CommentLikesJdrs
				.CountAsync(l => l.CommentId == request.CommentId);

			return Ok(new { isLiked, likesCount });
		}

		// POST: /Store/DeleteComment
		[HttpPost]
		public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentRequest request)
		{
			var currentUserId = GetCurrentUserId();
			if (!currentUserId.HasValue)
				return Unauthorized(new { message = "You must be logged in to delete." });

			PlayerCommentsJdr? comment = await _context.PlayerCommentsJdrs
				.FirstOrDefaultAsync(c => c.Id == request.CommentId && c.Status);

			if (comment == null)
				return NotFound(new { message = "Comment not found." });

			if (comment.UserId != currentUserId.Value)
				return Forbid();

			// Soft delete: desactivar el comentario y todos sus hijos
			await SoftDeleteRecursive(comment.Id);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Comment deleted." });
		}

		[HttpGet]
		public async Task<IActionResult> UserPhoto(string adp)
		{
			// Solo usuarios logueados pueden ver fotos
			var currentUserId = GetCurrentUserId();
			if (!currentUserId.HasValue)
				return NotFound();

			if (string.IsNullOrEmpty(adp))
				return NotFound();

			var photoUrl = await _photoService.GetCroppedPhoto(adp);

			if (string.IsNullOrEmpty(photoUrl))
				return NotFound();

			try
			{
				using var httpClient = new HttpClient(new HttpClientHandler
				{
					ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
				});

				var response = await httpClient.GetAsync(photoUrl);

				if (!response.IsSuccessStatusCode)
					return NotFound();

				var imageBytes = await response.Content.ReadAsByteArrayAsync();
				var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

				return File(imageBytes, contentType);
			}
			catch
			{
				return NotFound();
			}
		}

		// HELPERS 

		private int? GetCurrentUserId()
		{
			var userIdStr = HttpContext.Session.GetString("UserId");
			if (int.TryParse(userIdStr, out int userId))
				return userId;
			return null;
		}

		private CommentViewModel BuildCommentTree(PlayerCommentsJdr comment, List<PlayerCommentsJdr> allComments, int? currentUserId)
		{
			var replies = allComments
				.Where(c => c.ParentId == comment.Id)
				.OrderBy(c => c.CreatedAt)
				.Select(c => BuildCommentTree(c, allComments, currentUserId))
				.ToList();

			return new CommentViewModel
			{
				Id = comment.Id,
				UserId = comment.UserId,
				UserName = comment.User?.Name ?? "Unknown",
				UserAdp = comment.User?.Adp,
				ParentId = comment.ParentId,
				Title = comment.Title,
				Content = comment.Content,
				CreatedAt = comment.CreatedAt,
				LikesCount = comment.CommentLikesJdrs.Count,
				RepliesCount = CountAllReplies(comment.Id, allComments),
				IsLikedByCurrentUser = currentUserId.HasValue && comment.CommentLikesJdrs.Any(l => l.UserId == currentUserId.Value),
				IsOwner = currentUserId.HasValue && comment.UserId == currentUserId.Value,
				Replies = replies
			};
		}


		private int CountAllReplies(int commentId, List<PlayerCommentsJdr> allComments)
		{
			var directReplies = allComments.Where(c => c.ParentId == commentId).ToList();
			int count = directReplies.Count;
			foreach (var reply in directReplies)
			{
				count += CountAllReplies(reply.Id, allComments);
			}
			return count;
		}

		private async Task SoftDeleteRecursive(int commentId)
		{
			PlayerCommentsJdr? comment = await _context.PlayerCommentsJdrs.FindAsync(commentId);
			if (comment == null) return;

			comment.Status = false;

			List<PlayerCommentsJdr> children = await _context.PlayerCommentsJdrs
				.Where(c => c.ParentId == commentId && c.Status)
				.ToListAsync();

			foreach (var child in children)
			{
				await SoftDeleteRecursive(child.Id);
			}
		}
	}

	// Request models (puedes moverlos a Models/ si prefieres)
	public class ToggleLikeRequest
	{
		public int CommentId { get; set; }
	}

	public class DeleteCommentRequest
	{
		public int CommentId { get; set; }
	}
}
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace GameLauncher.Controllers
{
	public class CultureController : Controller
	{
		public IActionResult SetLanguage(string culture)
		{
			Response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(
					new RequestCulture(culture)),
				new CookieOptions
				{
					Expires = DateTimeOffset.UtcNow.AddYears(1),
					IsEssential = true,
					SameSite = SameSiteMode.Lax
				}
			);

			var referer = Request.Headers["Referer"].ToString();
			if (!string.IsNullOrEmpty(referer))
				return Redirect(referer);

			return RedirectToAction("Index", "Store");
		}
	}
}
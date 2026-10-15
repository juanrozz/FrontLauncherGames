using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using GameLauncher.Models;
using GameLauncher.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameLauncher.Controllers
{
	public class AccountController : Controller
	{
		private readonly string _apiBase =
			"http://us0145ldev01.lat.sitel-world.net/CO/RED/DEV/EndspointsPage/api";

		private readonly JsonSerializerOptions _jsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		private readonly EmployeePhotoService _photoService;
		private readonly DomainService _domainService;

		public AccountController(EmployeePhotoService photoService, DomainService domainService)
		{
			_photoService = photoService;
			_domainService = domainService;
		}

		
		// GET: /Account/Login
		public async Task<IActionResult> Login(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			ViewBag.Domains = await _domainService.GetDomainsAsync();
			return View();
		}

		// POST: /Account/Login
		[HttpPost]
		public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Domains = await _domainService.GetDomainsAsync();
				return View(model);
			}

			try
			{
				using var client = new HttpClient();
				var json = JsonSerializer.Serialize(model);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await client.PostAsync($"{_apiBase}/Login", content);
				var responseBody = await response.Content.ReadAsStringAsync();

				var result = JsonSerializer.Deserialize<ApiResponse<LoginData>>(responseBody, _jsonOptions);

				if (result == null)
				{
					ModelState.AddModelError("", "Unable to process server response.");
					ViewData["ReturnUrl"] = returnUrl;
					ViewBag.Domains = await _domainService.GetDomainsAsync();
					return View(model);
				}

				if (result.Success && result.Data != null)
				{
					HttpContext.Session.SetString("Token", result.Data.Token);
					HttpContext.Session.SetString("UserName", result.Data.Name);

					var handler = new JwtSecurityTokenHandler();
					var jwt = handler.ReadJwtToken(result.Data.Token);
					var userId = jwt.Claims.First(c => c.Type == "nameid").Value;
					HttpContext.Session.SetString("UserId", userId);

					var adp = result.Data.ADP;

					if (!string.IsNullOrEmpty(adp))
					{
						var photoUrl = await _photoService.GetCroppedPhoto(adp);
						if (!string.IsNullOrEmpty(photoUrl))
						{
							HttpContext.Session.SetString("UserPhoto", photoUrl);
						}
					}

					if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
						return Redirect(returnUrl);

					return RedirectToAction("Index", "Store");
				}
				else
				{
					ModelState.AddModelError("", result.Message);
					ViewData["ReturnUrl"] = returnUrl;
					ViewBag.Domains = await _domainService.GetDomainsAsync();
					return View(model);
				}
			}
			catch (Exception)
			{
				ModelState.AddModelError("", "Unable to connect to the server. Please try again.");
				ViewData["ReturnUrl"] = returnUrl;
				ViewBag.Domains = await _domainService.GetDomainsAsync();
				return View(model);
			}
		}

		// GET: /Account/SignUp
		public async Task<IActionResult> SignUp()
		{
			ViewBag.Domains = await _domainService.GetDomainsAsync();
			return View();
		}

		// POST: /Account/SignUp
		[HttpPost]
		public async Task<IActionResult> SignUp(SignUpModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			try
			{
				using var client = new HttpClient();

				var requestBody = new
				{
					Name = model.Name,
					UserName = model.UserName,
					ADP = model.ADP,
					DomainId = model.DomainId
				};

				var json = JsonSerializer.Serialize(requestBody);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await client.PostAsync($"{_apiBase}/SignUp", content);
				var responseBody = await response.Content.ReadAsStringAsync();

				var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody, _jsonOptions);

				if (result == null)
				{
					ModelState.AddModelError("", "Unable to process server response.");
					ViewBag.Domains = await _domainService.GetDomainsAsync();
					return View(model);
				}

				if (result.Success)
				{
					TempData["SuccessMessage"] = result.Message;
					return RedirectToAction("Login");
				}
				else
				{
					ModelState.AddModelError("", result.Message);
					ViewBag.Domains = await _domainService.GetDomainsAsync();
					return View(model);
				}
			}
			catch (Exception)
			{
				ModelState.AddModelError("", "Unable to connect to the server. Please try again.");
				ViewBag.Domains = await _domainService.GetDomainsAsync();
				return View(model);
			}
		}

		// GET: /Account/Logout
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Login");
		}
	}
}
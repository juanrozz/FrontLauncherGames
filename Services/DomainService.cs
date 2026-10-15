using System.Text.Json;
using GameLauncher.Models;

namespace GameLauncher.Services
{
	public class DomainService
	{
		private readonly string _apiBase =
			"http://us0145ldev01.lat.sitel-world.net/CO/RED/DEV/EndspointsPage/api";

		private readonly JsonSerializerOptions _jsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public async Task<List<DomainModel>> GetDomainsAsync()
		{
			try
			{
				using var client = new HttpClient();
				var response = await client.GetAsync($"{_apiBase}/Domains");
				var responseBody = await response.Content.ReadAsStringAsync();

				var result = JsonSerializer.Deserialize<ApiResponse<List<DomainModel>>>(responseBody, _jsonOptions);

				if (result != null && result.Success && result.Data != null)
				{
					return result.Data;
				}

				return new List<DomainModel>();
			}
			catch
			{
				return new List<DomainModel>();
			}
		}
	}
}
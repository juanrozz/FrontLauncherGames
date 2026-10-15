namespace GameLauncher.Services
{
	public class EmployeePhotoService
	{
		private const string API_BASE = "https://us0145lapp01.lat.sitel-world.net/CO/RED/PROD/APIS/SpaceIDCard_employeeImagesAPI/api/ImagenUser/GetImagen";
		private const string API_KEY = "3lR3p0rt3C0r0Nel##M1duDeyJCPR";
		private const string IMG_PREFIX = "https://us0145lapp01/CO/RED/PROD/Admin/SpaceIDCard/";

		public async Task<string?> GetCroppedPhoto(string adp)
		{
			try
			{
				using var client = new HttpClient();
				client.DefaultRequestHeaders.Add("ApiKeyValidator", API_KEY);

				var response = await client.GetAsync($"{API_BASE}/{adp}");

				if (!response.IsSuccessStatusCode)
					return null;

				var body = await response.Content.ReadAsStringAsync();
				var json = System.Text.Json.JsonDocument.Parse(body);

				var root = json.RootElement;

				// Verificar status == "Success"
				if (root.GetProperty("status").GetString() != "Success")
					return null;

				// Obtener croppedImage
				var data = root.GetProperty("data");
				var croppedImage = data.GetProperty("croppedImage").GetString();

				if (string.IsNullOrEmpty(croppedImage))
					return null;

				return IMG_PREFIX + croppedImage;
			}
			catch
			{
				return null;
			}
		}
	}
}
namespace GameLauncher.Models
{
	// Respuesta genérica que siempre devuelve la API
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public T? Data { get; set; }
		public string? Errors { get; set; }
		public DateTime Timestamp { get; set; }
	}

	// Data del Login
	public class LoginData
	{
		public string Token { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? ADP { get; set; }
	}
}
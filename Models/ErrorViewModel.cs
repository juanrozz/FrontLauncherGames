namespace GameLauncher.Models
{
	public class ErrorViewModel
	{
		// Almacena el ID único de la solicitud HTTP actual
		public string? RequestId { get; set; }

		// Propiedad calculada que determina si se debe mostrar el RequestId en la vista
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}

using System;
using System.Collections.Generic;

namespace GameLauncher.Models;

public partial class GamesJdr
{
    public int GameId { get; set; }

    public string GameName { get; set; } = null!;

    public bool Status { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? IconUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? GameNameEs { get; set; }

    public string? DescriptionEs { get; set; }
}

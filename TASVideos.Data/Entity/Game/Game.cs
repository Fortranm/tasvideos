﻿namespace TASVideos.Data.Entity.Game;

/// <summary>
/// Represents a Game
/// This is the central reference point for all site content
/// </summary>
public class Game : BaseEntity
{
	public int Id { get; set; }
	public virtual ICollection<GameRom> Roms { get; set; } = new HashSet<GameRom>();

	public int SystemId { get; set; }
	public virtual GameSystem? System { get; set; }

	public virtual ICollection<Publication> Publications { get; set; } = new HashSet<Publication>();
	public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
	public virtual ICollection<GameGenre> GameGenres { get; set; } = new HashSet<GameGenre>();
	public virtual ICollection<UserFile> UserFiles { get; set; } = new HashSet<UserFile>();
	public virtual ICollection<GameGameGroup> GameGroups { get; set; } = new HashSet<GameGameGroup>();

	[Required]
	[StringLength(250)]
	[Description("Good Set or some other official naming convention")]
	public string GoodName { get; set; } = "";

	[Required]
	[StringLength(100)]
	public string DisplayName { get; set; } = "";

	[StringLength(8)]
	public string? Abbreviation { get; set; }

	[StringLength(64)]
	public string? SearchKey { get; set; }

	[Required]
	[StringLength(250)]
	public string YoutubeTags { get; set; } = "";

	[StringLength(250)]
	public string? ScreenshotUrl { get; set; }

	[StringLength(300)]
	public string? GameResourcesPage { get; set; }
}

public static class GameExtensions
{
	public static IQueryable<Game> ForGroup(this IQueryable<Game> query, int gameGroupId)
	{
		return query.Where(g => g.GameGroups.Any(gg => gg.GameGroupId == gameGroupId));
	}

	public static IQueryable<Game> ForSystem(this IQueryable<Game> query, int systemId)
	{
		return query.Where(g => g.Roms.Count == 0 || g.Roms.Any(r => r.SystemId == systemId));
	}

	public static IQueryable<Game> ForSystemCode(this IQueryable<Game> query, string? code)
	{
		return !string.IsNullOrWhiteSpace(code)
			? query.Where(g => g.Roms.Count == 0 || g.Roms.Any(r => r.System!.Code == code))
			: query;
	}

	public static IQueryable<Game> ForSystemCodes(this IQueryable<Game> query, IEnumerable<string> codes)
	{
		var codeList = codes.ToList();
		return codeList.Any()
			? query.Where(g => g.Roms.Select(r => r.System!.Code).Any(c => codeList.Contains(c)))
			: query;
	}

	public static void SetGenres(this ICollection<GameGenre> genres, IEnumerable<int> genreIds)
	{
		genres.Clear();
		genres.AddRange(genreIds.Select(g => new GameGenre
		{
			GenreId = g
		}));
	}

	public static void SetGroups(this ICollection<GameGameGroup> groups, IEnumerable<int> groupIds)
	{
		groups.Clear();
		groups.AddRange(groupIds.Select(g => new GameGameGroup
		{
			GameGroupId = g
		}));
	}
}

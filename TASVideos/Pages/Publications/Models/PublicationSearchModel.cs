﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Models;

public class PublicationSearchModel : IPublicationTokens
{
	[Display(Name = "Platform")]
	public IEnumerable<string> SystemCodes { get; set; } = new List<string>();
	public IEnumerable<string> Classes { get; set; } = new List<string>();
	public IEnumerable<int> Years { get; set; } = new List<int>();
	public IEnumerable<string> Tags { get; set; } = new List<string>();
	public IEnumerable<string> Genres { get; set; } = new List<string>();
	public IEnumerable<string> Flags { get; set; } = new List<string>();

	[Display(Name = "Show Obsoleted")]
	public bool ShowObsoleted { get; set; }

	[Display(Name = "Only Obsoleted")]
	public bool OnlyObsoleted { get; set; }

	public string SortBy { get; set; } = "";
	public int? Limit { get; set; }

	public IEnumerable<int> Authors { get; set; } = new List<int>();

	public IEnumerable<int> MovieIds { get; set; } = new List<int>();

	public IEnumerable<int> Games { get; set; } = new List<int>();

	[Display(Name = "Game Groups")]
	public IEnumerable<int> GameGroups { get; set; } = new List<int>();

	public bool IsEmpty => !SystemCodes.Any()
		&& !Classes.Any()
		&& !Years.Any()
		&& !Flags.Any()
		&& !Tags.Any()
		&& !Genres.Any()
		&& !Authors.Any()
		&& !MovieIds.Any()
		&& !Games.Any()
		&& !GameGroups.Any();

	public string ToUrl()
	{
		var sb = new StringBuilder();
		sb.Append(string.Join("-", Classes));
		if (SystemCodes.Any())
		{
			sb.Append('-').Append(string.Join("-", SystemCodes));
		}

		if (Years.Any())
		{
			sb.Append('-').Append(string.Join("-", Years.Select(y => $"Y{y}")));
		}

		if (Tags.Any())
		{
			sb.Append('-').Append(string.Join("-", Tags));
		}

		if (Flags.Any())
		{
			sb.Append('-').Append(string.Join("-", Flags));
		}

		if (Genres.Any())
		{
			sb.Append('-').Append(string.Join("-", Genres));
		}

		if (Games.Any())
		{
			sb.Append('-').Append(string.Join("-", Games.Select(g => $"{g}g")));
		}

		if (GameGroups.Any())
		{
			sb.Append('-').Append(string.Join("-", GameGroups.Select(gg => $"group{gg}")));
		}

		if (Authors.Any())
		{
			sb.Append('-').Append(string.Join("-", Authors.Select(a => $"author{a}")));
		}

		if (OnlyObsoleted && !IsEmpty)
		{
			sb.Append("-ObsOnly");
		}
		else if (ShowObsoleted && !IsEmpty)
		{
			sb.Append("-Obs");
		}

		if (!string.IsNullOrWhiteSpace(SortBy))
		{
			sb.Append("-Sort").Append(SortBy);
		}

		return sb.ToString().Trim('-');
	}

	public static PublicationSearchModel FromTokens(IReadOnlyCollection<string> tokens, IPublicationTokens tokenLookup)
	{
		var limitStr = tokens
			.Where(t => t.StartsWith("limit"))
			.Select(t => t.Replace("limit", ""))
			.FirstOrDefault();
		int? limit = null;
		if (int.TryParse(limitStr, out int l))
		{
			limit = l;
		}

		return new PublicationSearchModel
		{
			Classes = tokenLookup.Classes.Where(tokens.Contains),
			SystemCodes = tokenLookup.SystemCodes.Where(tokens.Contains),
			ShowObsoleted = tokens.Contains("obs"),
			OnlyObsoleted = tokens.Contains("obsonly"),
			SortBy = tokens.Where(t => t.StartsWith("sort")).Select(t => t.Replace("sort", "")).FirstOrDefault() ?? "",
			Limit = limit,
			Years = tokenLookup.Years.Where(y => tokens.Contains("y" + y)),
			Tags = tokenLookup.Tags.Where(tokens.Contains),
			Genres = tokenLookup.Genres.Where(tokens.Contains),
			Flags = tokenLookup.Flags.Where(tokens.Contains),
			MovieIds = tokens.ToIdList('m'),
			Games = tokens.ToIdList('g'),
			GameGroups = tokens.ToIdListPrefix("group"),
			Authors = tokens
				.Where(t => t.ToLower().Contains("author"))
				.Select(t => t.ToLower().Replace("author", ""))
				.Select(t => int.TryParse(t, out var temp) ? temp : (int?)null)
				.Where(t => t.HasValue)
				.Select(t => t!.Value)
				.ToList()
		};
	}
}

﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.DisplayMovies)]
public class DisplayMovies : ViewComponent
{
	private readonly ApplicationDbContext _db;
	private readonly IMovieSearchTokens _tokens;

	public DisplayMovies(
		ApplicationDbContext db,
		IMovieSearchTokens tokens)
	{
		_db = db;
		_tokens = tokens;
	}

	public async Task<IViewComponentResult> InvokeAsync(
		IList<string> pubClass,
		IList<string> systemCode,
		bool obs,
		bool obsOnly,
		IList<int> year,
		IList<string> tag,
		IList<string> flag,
		IList<int> group,
		IList<int> id,
		IList<int> game,
		IList<int> author,
		string? sort,
		int? limit)
	{
		var tokenLookup = await _tokens.GetTokens();

		var searchModel = new PublicationSearchModel
		{
			Classes = tokenLookup.Classes.Where(c => pubClass.Select(tt => tt.ToLower()).Contains(c)),
			SystemCodes = tokenLookup.SystemCodes.Where(s => systemCode.Select(c => c.ToLower()).Contains(s)),
			ShowObsoleted = obs,
			OnlyObsoleted = obsOnly,
			SortBy = sort?.ToLower() ?? "",
			Limit = limit,
			Years = tokenLookup.Years.Where(year.Contains),
			Tags = tokenLookup.Tags.Where(t => tag.Select(tt => tt.ToLower()).Contains(t)),
			Genres = tokenLookup.Genres.Where(g => tag.Select(tt => tt.ToLower()).Contains(g)),
			Flags = tokenLookup.Flags.Where(f => flag.Select(ff => ff.ToLower()).Contains(f)),
			MovieIds = id,
			Games = game,
			GameGroups = group,
			Authors = author
		};

		if (searchModel.IsEmpty)
		{
			return View(new List<PublicationDisplayModel>());
		}

		var results = await _db.Publications
			.FilterByTokens(searchModel)
			.ToViewModel(searchModel.SortBy == "y")
			.ToListAsync();
		return View(results);
	}
}

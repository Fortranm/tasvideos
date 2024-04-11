﻿using TASVideos.Core;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiTextChangeLog)]
public class WikiTextChangeLog(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<Entry> Log { get; set; } = PageOf<Entry>.Empty();

	public async Task<IViewComponentResult> InvokeAsync(bool includeMinors)
	{
		DefaultPageSize = 100;

		var query = db.WikiPages
			.ThatAreNotDeleted()
			.ByMostRecent();

		if (!includeMinors)
		{
			query = query.ExcludingMinorEdits();
		}

		Log = await query
			.Select(wp => new Entry
			{
				PageName = wp.PageName,
				Revision = wp.Revision,
				Author = wp.Author!.UserName,
				CreateTimestamp = wp.CreateTimestamp,
				MinorEdit = wp.MinorEdit,
				RevisionMessage = wp.RevisionMessage
			})
			.PageOf(GetPaging());

		return View();
	}

	public class Entry
	{
		public DateTime CreateTimestamp { get; init; }
		public string? Author { get; init; }
		public string PageName { get; init; } = "";
		public int Revision { get; init; }
		public bool MinorEdit { get; init; }
		public string? RevisionMessage { get; init; }
	}
}
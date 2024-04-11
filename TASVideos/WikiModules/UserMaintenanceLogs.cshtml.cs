﻿using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UserMaintenanceLogs)]
public class UserMaintenanceLogs(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<UserMaintenanceLogEntry> Logs { get; set; } = PageOf<UserMaintenanceLogEntry>.Empty();

	public async Task<IViewComponentResult> InvokeAsync()
	{
		if (!HttpContext.User.Has(PermissionTo.ViewPrivateUserData))
		{
			return new ContentViewComponentResult("No access to this resource");
		}

		DefaultPageSize = 100;
		DefaultSort = "-TimeStamp";

		string user = HttpContext.Request.QueryStringValue("User");

		var logsQuery = db.UserMaintenanceLogs
			.Select(m => new UserMaintenanceLogEntry
			{
				User = m.User!.UserName,
				Editor = m.Editor!.UserName,
				TimeStamp = m.TimeStamp,
				Log = m.Log
			});

		if (!string.IsNullOrWhiteSpace(user))
		{
			logsQuery = logsQuery.Where(l => l.User == user);
		}

		Logs = await logsQuery.SortedPageOf(GetPaging());

		return View();
	}

	public record UserMaintenanceLogEntry
	{
		[Sortable]
		public string User { get; init; } = "";

		[Sortable]
		public string Editor { get; init; } = "";

		[Sortable]
		public DateTime TimeStamp { get; init; }

		[Sortable]
		public string Log { get; init; } = "";
	}
}

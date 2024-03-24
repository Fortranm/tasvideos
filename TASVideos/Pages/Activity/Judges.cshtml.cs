﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class JudgesModel(ApplicationDbContext db) : BasePageModel
{
	public List<SubmissionEntryModel> Submissions { get; set; } = [];

	[FromRoute]
	public string UserName { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		if (string.IsNullOrWhiteSpace(UserName))
		{
			return NotFound();
		}

		var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
		if (user is null)
		{
			return NotFound();
		}

		Submissions = await db.Submissions
			.ThatHaveBeenJudgedBy(UserName)
			.Select(s => new SubmissionEntryModel(
				s.Id,
				s.CreateTimestamp,
				s.Title,
				s.Status))
			.ToListAsync();

		return Page();
	}

	public record SubmissionEntryModel(int Id, DateTime CreateTimestamp, string Title, SubmissionStatus Status);
}

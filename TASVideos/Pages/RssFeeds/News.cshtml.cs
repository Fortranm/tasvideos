﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.RssFeeds.Models;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class NewsModel : BasePageModel
{
	private const int NewsTopicId = 8694; // Unfortunately, there is not an easy way to not hard code this
	private readonly ApplicationDbContext _db;

	public NewsModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public List<RssNews> News { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		News = await _db.ForumPosts
			.ForTopic(NewsTopicId)
			.ByMostRecent()
			.Select(p => new RssNews
			{
				PostId = p.Id,
				PubDate = p.LastUpdateTimestamp,
				Subject = p.Subject ?? "",
				Text = p.Text
			})
			.Take(10)
			.ToListAsync();
		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}

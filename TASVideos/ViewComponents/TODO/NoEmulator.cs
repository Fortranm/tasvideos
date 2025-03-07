﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.NoEmulator)]
public class NoEmulator : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public NoEmulator(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var model = new MissingModel
		{
			Publications = await _db.Publications
				.Where(p => string.IsNullOrEmpty(p.EmulatorVersion))
				.OrderBy(p => p.Id)
				.Select(p => new MissingModel.Entry(p.Id, p.Title))
				.ToListAsync(),
			Submissions = await _db.Submissions
				.Where(s => string.IsNullOrEmpty(s.EmulatorVersion))
				.OrderBy(s => s.Id)
				.Select(s => new MissingModel.Entry(s.Id, s.Title))
				.ToListAsync()
		};
		return View(model);
	}
}

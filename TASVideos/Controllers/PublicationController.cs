﻿using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class PublicationController : BaseController
	{
		private readonly PublicationTasks _publicationTasks;
		private readonly RatingsTasks _ratingsTasks;

		public PublicationController(
			PublicationTasks publicationTasks,
			RatingsTasks ratingTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
			_ratingsTasks = ratingTasks;
		}

		[AllowAnonymous]
		public IActionResult Index()
		{
			return RedirectToAction(nameof(List));
		}

		[AllowAnonymous]
		public async Task<IActionResult> List(string query)
		{
			var tokenLookup = await _publicationTasks.GetMovieTokenData();

			var tokens = query
				.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim(' '))
				.Select(s => s.ToLower())
				.ToList();

			var searchModel = new PublicationSearchModel
			{
				Tiers = tokenLookup.Tiers.Where(t => tokens.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => tokens.Contains(s)),
				ShowObsoleted = tokens.Contains("obs"),
				Years = tokenLookup.Years.Where(y => tokens.Contains("y" + y)),
				Tags = tokenLookup.Tags.Where(t => tokens.Contains(t)),
				Flags = tokenLookup.Flags.Where(f => tokens.Contains(f)),
				Authors = tokens
					.Where(t => t.ToLower().Contains("author"))
					.Select(t => t.ToLower().Replace("author", ""))
					.Select(t => int.TryParse(t, out var temp) ? temp : (int?)null)
					.Where(t => t.HasValue)
					.Select(t => t.Value)
					.ToList()
			};

			// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
			if (searchModel.IsEmpty)
			{
				return Redirect("Movies");
			}

			var model = (await _publicationTasks
				.GetMovieList(searchModel))
				.ToList();

			var ratings = await _ratingsTasks.GetOverallRatingsForPublications(model.Select(m => m.Id));

			foreach (var rating in ratings)
			{
				model.First(m => m.Id == rating.Key).OverallRating = rating.Value ?? 0; // TODO: don't null coalesce, make model rating nullable and handle in view
			}

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> View(int id)
		{
			var model = await _publicationTasks.GetPublicationForDisplay(id);
			if (model == null)
			{
				return NotFound();
			}

			model.OverallRating = await _ratingsTasks.GetOverallRatingForPublication(id)
				?? 0; // TODO: Don't null coalesce, handle on in view properly

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Download(int id)
		{
			(var fileBytes, var fileName) = await _publicationTasks.GetPublicationMovieFile(id);
			if (fileBytes.Length > 0)
			{
				return File(fileBytes, MediaTypeNames.Application.Octet, $"{fileName}.zip");
			}

			return BadRequest();
		}

		[RequirePermission(PermissionTo.EditPublicationMetaData)]
		public async Task<IActionResult> Edit(int id)
		{
			var model = await _publicationTasks.GetPublicationForEdit(id);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		[RequirePermission(PermissionTo.EditPublicationMetaData)]
		public async Task<IActionResult> Edit(PublicationEditModel model)
		{
			if (!ModelState.IsValid)
			{
				model.AvailableMoviesForObsoletedBy =
					await _publicationTasks.GetAvailableMoviesForObsoletedBy(model.Id, model.SystemCode);

				return View(model);
			}

			await _publicationTasks.UpdatePublication(model);
			return RedirectToAction(nameof(View), new { model.Id });
		}

		[RequirePermission(PermissionTo.CatalogMovies)]
		public async Task<IActionResult> Catalog(int id)
		{
			var model = await _publicationTasks.Catalog(id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		[RequirePermission(PermissionTo.CatalogMovies)]
		public async Task<IActionResult> Catalog(PublicationCatalogModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model); // TODO: repopulate dropdowns
			}

			await _publicationTasks.UpdateCatalog(model);
			return RedirectToAction(nameof(View), new { model.Id });
		}
	}
}

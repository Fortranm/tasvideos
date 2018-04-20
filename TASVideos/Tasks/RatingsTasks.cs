﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class RatingsTasks
	{
		private const string MovieRatingKey = "OverallRatingForMovie-";

		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public RatingsTasks(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		/// <summary>
		/// Returns a detailed list of all ratings for a <see cref="Publication"/>
		/// with the given <see cref="publicationId"/>
		/// If no <see cref="Publication"/> is found, then null is returned
		/// </summary>
		public async Task<PublicationRatingsViewModel> GetRatingsForPublication(int publicationId)
		{
			var publication = await _db.Publications
				.Include(p => p.PublicationRatings)
				.ThenInclude(r => r.User)
				.SingleOrDefaultAsync(p => p.Id == publicationId);

			if (publication == null)
			{
				return null;
			}

			var model = new PublicationRatingsViewModel
			{
				PublicationId = publication.Id,
				PublicationTitle = publication.Title,
				Ratings = publication.PublicationRatings
					.GroupBy(
						key => new { key.PublicationId, key.User.UserName, key.User.PublicRatings },
						grp => new { grp.Type, grp.Value })
					.Select(g => new PublicationRatingsViewModel.RatingEntry
					{
						UserName = g.Key.UserName,
						IsPublic = g.Key.PublicRatings,
						Entertainment = g.FirstOrDefault(v => v.Type == PublicationRatingType.Entertainment)?.Value,
						TechQuality = g.FirstOrDefault(v => v.Type == PublicationRatingType.TechQuality)?.Value
					})
					.ToList()
			};

			model.AverageEntertainmentRating = Math.Round(model.Ratings
				.Where(r => r.Entertainment.HasValue)
				.Select(r => r.Entertainment.Value).Average(), 2);

			model.AverageTechRating = Math.Round(model.Ratings
				.Where(r => r.TechQuality.HasValue)
				.Select(r => r.TechQuality.Value).Average(), 2);

			// Entertainment counts 2:1 over Tech
			model.OverallRating = Math.Round(model.Ratings
				.Where(r => r.Entertainment.HasValue)
				.Select(r => r.Entertainment.Value)
				.Concat(model.Ratings
					.Where(r => r.Entertainment.HasValue)
					.Select(r => r.Entertainment.Value))
				.Concat(model.Ratings
					.Where(r => r.TechQuality.HasValue)
					.Select(r => r.TechQuality.Value))
				.Average(), 2);

			_cache.Set(MovieRatingKey + publicationId, model.OverallRating);

			return model;
		}

		// TODO: what if an invalid publicationId?
		// TODO: what if there are no ratings for a publication?
		/// <summary>
		/// Calculates the overall rating for a <see cref="Publication"/>
		/// with the given <see cref="publicationId"/>
		/// </summary>
		public async Task<double> GetOverallRatingForPublication(int publicationId)
		{
			string cacheKey = MovieRatingKey + publicationId;
			if (_cache.TryGetValue(cacheKey, out double rating))
			{
				return rating;
			}

			var ratings = await _db.PublicationRatings
				.Where(pr => pr.PublicationId == publicationId)
				.ToListAsync();

			var entRatings = ratings
				.Where(r => r.Type == PublicationRatingType.Entertainment)
				.Select(r => r.Value)
				.ToList();

			var techRatings = ratings
				.Where(r => r.Type == PublicationRatingType.TechQuality)
				.Select(r => r.Value)
				.ToList();

			// TODO: calculate this in one place
			// Entertainmnet counts 2:1 over Tech

			double overallRating = 0;
			if (entRatings.Count + techRatings.Count > 0)
			{
				overallRating = entRatings
					.Concat(entRatings)
					.Concat(techRatings)
					.Average();
			}

			_cache.Set(cacheKey, overallRating);

			return overallRating;
		}

		/// <summary>
		/// Returns the overall rating for the <see cref="Publication"/> with the given <see cref="publicationIds"/>
		/// </summary>
		/// <exception cref="ArgumentException">If <see cref="publicationIds"/> is null</exception>
		public async Task<IDictionary<int, double>> GetOverallRatingsForPublications(IEnumerable<int> publicationIds)
		{
			if (publicationIds == null)
			{
				throw new ArgumentException($"{nameof(publicationIds)} can not be null");
			}

			var ratings = new Dictionary<int, double>();

			using (await _db.Database.BeginTransactionAsync())
			{
				foreach (var id in publicationIds)
				{
					var cacheKey = MovieRatingKey + id;
					if (_cache.TryGetValue(cacheKey, out double rating))
					{
						ratings.Add(id, rating);
					}
					else
					{
						rating = await GetOverallRatingForPublication(id);
						_cache.Set(cacheKey, rating);
						ratings.Add(id, rating);
					}
				}
			}

			return ratings;
		}
	}
}

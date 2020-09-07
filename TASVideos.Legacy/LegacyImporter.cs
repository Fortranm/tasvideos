﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using TASVideos.Data;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Forum.Entity;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy
{
	public static class LegacyImporter
	{
		private static readonly Dictionary<string, long> ImportDurations = new Dictionary<string, long>();

		public static void RunLegacyImport(
			IWebHostEnvironment env,
			ApplicationDbContext context,
			string connectionStr,
			NesVideosSiteContext legacySiteContext,
			NesVideosForumContext legacyForumContext)
		{
			// Since we are using this database in a read-only way, set no tracking globally
			// To speed up query executions
			legacySiteContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
			legacyForumContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

			var stopwatch = Stopwatch.StartNew();

			Run("Tags", () => TagImporter.Import(connectionStr, context, legacySiteContext));
			Run("Roms", () => RomImporter.Import(connectionStr, context, legacySiteContext));
			Run("Games", () => GameImporter.Import(connectionStr, context, legacySiteContext));
			Run("GameGroup", () => GameGroupImporter.Import(connectionStr, context, legacySiteContext));
			Run("GameGenre", () => GameGenreImport.Import(connectionStr, context, legacySiteContext));
			Run("RamAddresses", () => RamAddressImporter.Import(connectionStr, context, legacySiteContext));

			Run("Users", () => UserImporter.Import(connectionStr, context, legacySiteContext, legacyForumContext));
			Run("Award", () => AwardImporter.Import(connectionStr, context, legacySiteContext));

			Run("Forum Categories", () => ForumCategoriesImporter.Import(connectionStr, context, legacyForumContext));
			Run("Forums", () => ForumImporter.Import(connectionStr, context, legacyForumContext));
			Run("Forum Topics", () => ForumTopicImporter.Import(connectionStr, context, legacyForumContext));
			Run("Forum Posts", () => ForumPostsImporter.Import(connectionStr, context, legacyForumContext));
			Run("Forum Private Messages", () => ForumPrivateMessagesImporter.Import(connectionStr, context, legacyForumContext));
			Run("Forum Polls", () => ForumPollImporter.Import(connectionStr, context, legacyForumContext));

			// We don't want to copy these to other environments, as they can cause users to get unwanted emails
			if (env.IsProduction())
			{
				Run("Forum Topic Watch", () => ForumTopicWatchImporter.Import(connectionStr, context, legacyForumContext));
			}

			Run("Wiki", () => WikiImporter.Import(connectionStr, context, legacySiteContext));
			Run("WikiCleanup", () => WikiPageCleanup.Fix(context, legacySiteContext));
			Run("Submissions", () => SubmissionImporter.Import(connectionStr, context, legacySiteContext));
			Run("Submissions Framerate", () => SubmissionFrameRateImporter.Import(context));
			Run("Publications", () => PublicationImporter.Import(connectionStr, context, legacySiteContext));
			Run("Publication Ratings", () => PublicationRatingImporter.Import(connectionStr, context, legacySiteContext));
			Run("Publication Flags", () => PublicationFlagImporter.Import(connectionStr, context, legacySiteContext));

			Run("User files", () => UserFileImporter.Import(connectionStr, context, legacySiteContext));

			var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
			stopwatch.Stop();
			Console.WriteLine($"Import finished. Total time: {elapsedMilliseconds / 1000.0} seconds");
			Console.WriteLine("Import breakdown:");
			foreach (var entry in ImportDurations)
			{
				Console.WriteLine($"{entry.Key}: {entry.Value}");
			}
		}

		private static void Run(string name, Action import)
		{
			var stopwatch = Stopwatch.StartNew();
			try
			{
				import();
			}
			catch (Exception ex)
			{
				throw new ImportException(name, ex);
			}

			ImportDurations.Add($"{name} import", stopwatch.ElapsedMilliseconds);
			stopwatch.Stop();
		}

		public class ImportException : Exception
		{
			public ImportException(string importStep, Exception innerException)
				: base($"Exception at import step: {importStep}", innerException)
			{
				ImportStep = importStep;
			}

			public string ImportStep { get; }
		}
	}
}

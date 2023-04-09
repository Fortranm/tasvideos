﻿using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

public interface ITrackable
{
	DateTime CreateTimestamp { get; set; }
	DateTime LastUpdateTimestamp { get; set; }
}

public class BaseEntity : ITrackable
{
	[ExcludeFromHistory]
	public DateTime CreateTimestamp { get; set; }

	[ExcludeFromHistory]
	public DateTime LastUpdateTimestamp { get; set; }
}

public static class TrackableQueryableExtensions
{
	public static IQueryable<T> OldestToNewest<T>(this IQueryable<T> list)
		where T : ITrackable
	{
		return list.OrderBy(t => t.CreateTimestamp);
	}

	public static IQueryable<T> ByMostRecent<T>(this IQueryable<T> list)
		where T : ITrackable
	{
		return list.OrderByDescending(t => t.CreateTimestamp);
	}

	public static IQueryable<T> Since<T>(this IQueryable<T> list, DateTime target)
		where T : ITrackable
	{
		return list.Where(t => t.CreateTimestamp >= target);
	}
}

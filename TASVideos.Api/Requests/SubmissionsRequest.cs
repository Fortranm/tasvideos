﻿namespace TASVideos.Api.Requests;

/// <summary>
/// Represents the filtering criteria for the submissions endpoint.
/// </summary>
public class SubmissionsRequest : ApiRequest, ISubmissionFilter
{
	/// <summary>
	/// Gets the statuses to filter by.
	/// </summary>
	public string? Statuses { get; init; }

	/// <summary>
	/// Gets user's name to filter by.
	/// </summary>
	public string? User { get; init; }

	/// <summary>
	/// Gets the start year to filter by.
	/// </summary>
	public int? StartYear { get; init; }

	/// <summary>
	/// Gets the end year to filter by.
	/// </summary>
	public int? EndYear { get; init; }

	/// <summary>
	/// Gets the system codes to filter by.
	/// </summary>
	public string? Systems { get; init; }

	/// <summary>
	/// Gets the Game Ids to filter by.
	/// </summary>
	public string? Games { get; init; }

	/// <summary>
	/// Gets the start type of the movie. 0 = Power On, 1 = Sram, 2 = Savestate
	/// </summary>
	public int? StartType { get; init; }

	IEnumerable<int> ISubmissionFilter.Years => StartYear.YearRange(EndYear);

	IEnumerable<SubmissionStatus> ISubmissionFilter.StatusFilter => !string.IsNullOrWhiteSpace(Statuses)
		? Statuses
			.SplitWithEmpty(",")
			.Where(s => Enum.TryParse(s, out SubmissionStatus _))
			.Select(s =>
				{
					Enum.TryParse(s, out SubmissionStatus x);
					return x;
				})
		: [];

	IEnumerable<string> ISubmissionFilter.Systems => Systems.CsvToStrings();
	IEnumerable<int> ISubmissionFilter.GameIds => Games.CsvToInts();
}

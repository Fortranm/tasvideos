﻿using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TASVideos.Pages.Publications.Models;

public class PublicationRateModel
{
	public sealed class RatingString : ValidationAttribute
	{
		public static double? AsRatingDouble(string? ratingString)
		{
			NumberFormatInfo customFormat = new CultureInfo("en-US").NumberFormat;
			customFormat.NumberDecimalSeparator = ".";
			var result = double.TryParse(ratingString, NumberStyles.AllowDecimalPoint, customFormat, out double ratingNumber);
			if (!result)
			{
				customFormat.NumberDecimalSeparator = ",";
				result = double.TryParse(ratingString, NumberStyles.AllowDecimalPoint, customFormat, out ratingNumber);
				if (!result)
				{
					return null;
				}
			}

			ratingNumber = Math.Round(ratingNumber, 1, MidpointRounding.AwayFromZero);

			return ratingNumber;
		}

		public override bool IsValid(object? value)
		{
			var ratingString = value as string;
			if (string.IsNullOrWhiteSpace(ratingString))
			{
				return true;
			}

			var ratingNumber = AsRatingDouble(ratingString);
			if (ratingNumber is >= 0.0 and <= 10.0)
			{
				return true;
			}

			return false;
		}
	}

	public string Title { get; set; } = "";

	[Display(Name = "Rating")]
	[RatingString(ErrorMessage = "{0} must be between 0 and 10.")]
	public string? Rating { get; set; }

	public bool Unrated { get; set; }
}

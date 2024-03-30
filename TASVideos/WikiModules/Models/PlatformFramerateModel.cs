﻿namespace TASVideos.WikiModules;

public class PlatformFramerateModel
{
	[Display(Name = "System")]
	public string SystemCode { get; init; } = "";

	[Display(Name = "Region")]
	public string RegionCode { get; init; } = "";

	[Display(Name = "Framerate")]
	public double FrameRate { get; init; }

	[Display(Name = "Preliminary or approximate")]
	public bool Preliminary { get; init; }
}

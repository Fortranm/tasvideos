﻿using TASVideos.Core.Services.ExternalMediaPublisher.Distributors;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class ExternalDependenciesModel : BasePageModel
{
	private readonly IGoogleAuthService _googleAuthService;
	private readonly TwitterDistributorV2 _twitter;
	private readonly AppSettings _settings;

	public ExternalDependenciesModel(
		IGoogleAuthService googleAuthService,
		TwitterDistributorV2 twitter,
		AppSettings settings)
	{
		_googleAuthService = googleAuthService;
		_twitter = twitter;
		_settings = settings;
	}

	public ExternalDependenciesViewModel Statuses { get; set; } = new();

	public async Task OnGet()
	{
		Statuses.YoutubeEnabled = _googleAuthService.IsYoutubeEnabled();
		if (Statuses.YoutubeEnabled)
		{
			try
			{
				Statuses.YoutubeAccessSuccessful = !string.IsNullOrWhiteSpace(await _googleAuthService.GetYoutubeAccessToken());
			}
			catch
			{
				// Do nothing;
			}
		}

		Statuses.EmailEnabled = _settings.Email.IsEnabled();
		Statuses.IrcEnabled = _settings.Irc.IsEnabled();
		Statuses.SecureIrcEnabled = _settings.Irc.IsSecureChannelEnabled();
		Statuses.DiscordEnabled = _settings.Discord.IsEnabled();
		Statuses.DiscordPrivateChannelEnabled = _settings.Discord.IsPrivateChannelEnabled();
		Statuses.TwitterEnabled = _twitter.IsEnabled();
	}

	public class ExternalDependenciesViewModel
	{
		public bool YoutubeEnabled { get; set; }
		public bool? YoutubeAccessSuccessful { get; set; }
		public bool EmailEnabled { get; set; }
		public bool IrcEnabled { get; set; }
		public bool SecureIrcEnabled { get; set; }
		public bool DiscordEnabled { get; set; }
		public bool DiscordPrivateChannelEnabled { get; set; }
		public bool TwitterEnabled { get; set; }
	}
}

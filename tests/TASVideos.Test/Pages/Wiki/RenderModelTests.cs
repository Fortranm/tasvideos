﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages.Wiki;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class RenderModelTests : BasePageModelTests
{
	private readonly Mock<IWikiPages> _mockWikiPages;
	private readonly RenderModel _model;

	public RenderModelTests()
	{
		_mockWikiPages = new Mock<IWikiPages>();
		var db = TestDbContext.Create();
		_model = new RenderModel(_mockWikiPages.Object, db, NullLogger<RenderModel>.Instance)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	public async Task Render_NullUrl_Redirects(string url)
	{
		var result = await _model.OnGet(url);
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Wiki/PageNotFound", redirect.PageName);
	}

	[TestMethod]
	public async Task Render_ExistingPage_FindsPage()
	{
		const string existingPage = "Test";
		_mockWikiPages
			.Setup(m => m.Page(existingPage, null))
			.ReturnsAsync(new WikiResult { PageName = existingPage });

		var result = await _model.OnGet(existingPage);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task Render_WhenUrlEncoded_FindsPage()
	{
		const string existingPage = "Foo/Bar";
		_mockWikiPages
			.Setup(m => m.Page(existingPage, null))
			.ReturnsAsync(new WikiResult { PageName = existingPage });
		var encoded = System.Net.WebUtility.UrlEncode(existingPage);

		var result = await _model.OnGet(encoded);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}
}

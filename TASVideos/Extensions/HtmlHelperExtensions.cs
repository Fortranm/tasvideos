﻿using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TASVideos.Extensions;

public static class HtmlHelperExtensions
{
	public static IHtmlContent DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
	{
		if (html is null)
		{
			throw new ArgumentNullException(nameof(html));
		}

		if (expression is null)
		{
			throw new ArgumentNullException(nameof(expression));
		}

		var expressionProvider = html.ViewContext.HttpContext.RequestServices.GetRequiredService<ModelExpressionProvider>();
		var modelExpression = expressionProvider.CreateModelExpression(html.ViewData, expression);

		return new HtmlString(modelExpression.Metadata.Description);
	}

	public static string ToYesNo(this bool val)
	{
		return val ? "Yes" : "No";
	}
}

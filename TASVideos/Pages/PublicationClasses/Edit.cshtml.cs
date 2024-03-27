﻿namespace TASVideos.Pages.PublicationClasses;

[RequirePermission(PermissionTo.ClassMaintenance)]
public class EditModel(IClassService classService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public PublicationClass PublicationClass { get; set; } = new();
	public bool InUse { get; set; } = true;

	public async Task<IActionResult> OnGet()
	{
		var publicationClass = await classService.GetById(Id);
		if (publicationClass is null)
		{
			return NotFound();
		}

		PublicationClass = publicationClass;
		InUse = await classService.InUse(Id);
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await classService.Edit(Id, PublicationClass);
		switch (result)
		{
			default:
			case ClassEditResult.Success:
				SuccessStatusMessage("Tag successfully updated.");
				return BasePageRedirect("Index");
			case ClassEditResult.NotFound:
				return NotFound();
			case ClassEditResult.DuplicateName:
				ModelState.AddModelError($"{nameof(PublicationClass)}.{nameof(PublicationClass.Name)}", $"{nameof(PublicationClass.Name)} {PublicationClass.Name} already exists");
				ClearStatusMessage();
				return Page();
			case ClassEditResult.Fail:
				ErrorStatusMessage($"Unable to delete Tag {Id}, the tag may have already been deleted or updated.");
				return Page();
		}
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var result = await classService.Delete(Id);
		switch (result)
		{
			case ClassDeleteResult.InUse:
				ErrorStatusMessage($"Unable to delete PublicationClass {Id}, the publicationClass is in use by at least 1 publication.");
				break;
			case ClassDeleteResult.Success:
				SuccessStatusMessage($"PublicationClass {Id}, deleted successfully.");
				break;
			case ClassDeleteResult.NotFound:
				ErrorStatusMessage($"PublicationClass {Id}, not found.");
				break;
			case ClassDeleteResult.Fail:
				ErrorStatusMessage($"Unable to delete PublicationClass {Id}, the publicationClass may have already been deleted or updated.");
				break;
		}

		return BasePageRedirect("Index");
	}
}

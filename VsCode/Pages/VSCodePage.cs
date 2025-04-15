using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;

namespace CmdPalVsCode;

internal sealed partial class VSCodePage : ListPage
{
    public VSCodePage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\VsCodeIcon.png");
        Title = "Visual Studio Code";
        Name = "Open";

        ShowDetails = true;
    }

    public override IListItem[] GetItems()
    {
        var items = new List<ListItem>();


        IsLoading = true;
        foreach (var workspace in VSCodeHandler.GetWorkspaces())
        {
            // add instance to the list
            var command = new OpenVSCodeCommand(workspace.Instance.ExecutablePath, workspace.Path);

            Details details = new Details()
            {
                Title = workspace.GetName(),
                HeroImage = workspace.Instance.GetIcon(),
                Metadata = new List<DetailsElement>(){
                    new DetailsElement()
                    {
                        Key = "Type",
                        Data = new DetailsTags() { Tags = new List<Tag>() { new Tag(workspace.GetVSType()) }.ToArray() }
                    },
                    new DetailsElement()
                    {
                        Key = "Path",
                        Data = new DetailsLink() { Text = workspace.Path },
                    }
                }.ToArray(),
            };


            items.Add(new ListItem(command) { Title = details.Title, Subtitle = workspace.Path, Details = details, Icon = workspace.Instance.GetIcon() });
        }

        IsLoading = false;

        return items.ToArray();
    }
}

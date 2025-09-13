using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blogtify.Client;

public partial class CustomComponentBase : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JSRuntime.InvokeVoidAsync("Prism.highlightAll");
        await JSRuntime.InvokeVoidAsync("addCopyButtons");
    }
}

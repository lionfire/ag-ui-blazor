using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// The draft seam: <see cref="MudMessageInput.InitialDraft"/> in,
/// <see cref="MudMessageInput.OnDraftChanged"/> out.
/// </summary>
/// <remarks>
/// On Blazor Server this component is destroyed and re-created on every conversation
/// switch and every circuit reconnect, so unsent text is lost unless the consumer holds it
/// outside the component. These two parameters are that seam; the component itself stores
/// nothing across instances, which is what keeps it a UI control rather than a store.
/// </remarks>
public class MudMessageInputDraftTests : BunitContext
{
    public MudMessageInputDraftTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void InitialDraft_seeds_the_message()
    {
        var cut = Render<MudMessageInput>(p => p.Add(x => x.InitialDraft, "half a message"));

        cut.Instance.Message.Should().Be("half a message");
    }

    [Fact]
    public void No_InitialDraft_leaves_the_message_empty()
    {
        var cut = Render<MudMessageInput>();

        cut.Instance.Message.Should().BeEmpty();
    }

    [Fact]
    public async Task A_re_supplied_InitialDraft_never_overwrites_what_the_user_has_typed()
    {
        // Applied in OnInitialized only. A parameter refresh mid-edit that reset the field
        // to a stale draft would be a worse defect than the one the seam exists to fix.
        var cut = Render<MudMessageInput>(p => p.Add(x => x.InitialDraft, "first"));
        await cut.InvokeAsync(() => cut.Instance.SetMessage("the user typed this"));

        cut.Render(p => p.Add(x => x.InitialDraft, "second"));

        cut.Instance.Message.Should().Be("the user typed this");
    }

    [Fact]
    public async Task Sending_reports_an_empty_draft_so_a_stored_draft_cannot_outlive_it()
    {
        var reported = new List<string>();
        var cut = Render<MudMessageInput>(p => p
            .Add(x => x.InitialDraft, "why is the sweep")
            .Add(x => x.OnDraftChanged, (string text) => reported.Add(text)));

        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        reported.Should().ContainSingle().Which.Should().BeEmpty();
        cut.Instance.Message.Should().BeEmpty();
    }

    [Fact]
    public async Task A_blank_send_reports_nothing_and_changes_nothing()
    {
        var reported = new List<string>();
        var cut = Render<MudMessageInput>(p => p
            .Add(x => x.OnDraftChanged, (string text) => reported.Add(text)));

        await cut.InvokeAsync(() => cut.Instance.SendMessage());

        reported.Should().BeEmpty();
    }
}

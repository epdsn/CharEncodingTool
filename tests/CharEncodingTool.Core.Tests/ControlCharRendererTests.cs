using CharEncodingTool.Core.Services;

namespace CharEncodingTool.Core.Tests;

public class ControlCharRendererTests
{
    [Fact]
    public void Render_Null_RendersAsSymbolForNull()
    {
        Assert.Equal("␀", ControlCharRenderer.Render("\0"));
    }

    [Fact]
    public void Render_Tab_RendersAsSymbolForTab()
    {
        // U+0009 (HT) → U+2409 (␉)
        Assert.Equal("␉", ControlCharRenderer.Render("\t"));
    }

    [Fact]
    public void Render_Lf_RendersAsSymbolForLf()
    {
        // U+000A (LF) → U+240A (␊)
        Assert.Equal("␊", ControlCharRenderer.Render("\n"));
    }

    [Fact]
    public void Render_Del_RendersAsSymbolForDel()
    {
        Assert.Equal("␡", ControlCharRenderer.Render("\x7F"));
    }

    [Fact]
    public void Render_PrintableChar_Unchanged()
    {
        Assert.Equal("A", ControlCharRenderer.Render("A"));
    }

    [Fact]
    public void Render_MixedString()
    {
        Assert.Equal("A␀B␊C", ControlCharRenderer.Render("A\0B\nC"));
    }
}

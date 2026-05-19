using System.Text;

namespace CharEncodingTool.Core.Services;

public static class ControlCharRenderer
{
    public static string Render(string input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            sb.Append(RenderChar(c));
        }
        return sb.ToString();
    }

    public static string RenderChar(char c)
    {
        // C0 control set: U+0000-U+001F → Unicode Control Pictures U+2400-U+241F
        if (c <= '\x1F') return char.ConvertFromUtf32(0x2400 + c);
        // DEL → U+2421 "Symbol for Delete"
        if (c == '\x7F') return "␡";
        return c.ToString();
    }
}

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace SmokeTests;

public static class HtmlDocumentExtensions
{
    public static async Task<IDocument> ParseHtmlAsync(this HttpResponseMessage response)
    {
        var html = await response.Content.ReadAsStringAsync();
        var parser = new HtmlParser();
        return await parser.ParseDocumentAsync(html);
    }
}

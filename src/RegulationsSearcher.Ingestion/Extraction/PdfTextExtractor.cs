using UglyToad.PdfPig;

namespace RegulationsSearcher.Ingestion.Extraction;

public sealed class PdfTextExtractor
{
    public IReadOnlyList<PdfPageText> ExtractPages(string pdfPath)
    {
        using var document = PdfDocument.Open(pdfPath);

        return document.GetPages()
            .OrderBy(page => page.Number)
            .Select(page => new PdfPageText(page.Number, page.Text))
            .ToList();
    }
}

public sealed record PdfPageText(int PageNumber, string Text);

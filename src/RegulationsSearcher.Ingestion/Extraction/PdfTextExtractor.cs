using UglyToad.PdfPig;

namespace RegulationsSearcher.Ingestion.Extraction;

public sealed class PdfTextExtractor
{
    public IReadOnlyList<PdfPageText> ExtractPages(string pdfPath)
    {
        if (!File.Exists(pdfPath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfPath}", pdfPath);
        }

        try
        {
            using var document = PdfDocument.Open(pdfPath);

            return document.GetPages()
                .OrderBy(page => page.Number)
                .Select(page => new PdfPageText(page.Number, page.Text))
                .ToList();
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            throw new InvalidOperationException($"Failed to parse PDF: {pdfPath}", ex);
        }
    }
}

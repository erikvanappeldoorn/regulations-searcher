namespace RegulationsSearcher.Ingestion.Indexing;

public static class ChunkIdGenerator
{
    public static string GenerateId(string documentName, int chunkIndex) =>
        $"{SanitizeForKey(documentName)}-{chunkIndex}";

    private static string SanitizeForKey(string documentName)
    {
        var characters = documentName.Select(character => IsValidKeyCharacter(character) ? character : '_').ToArray();
        return new string(characters);
    }

    private static bool IsValidKeyCharacter(char character) =>
        (character >= 'a' && character <= 'z')
        || (character >= 'A' && character <= 'Z')
        || (character >= '0' && character <= '9')
        || character is '_' or '-' or '=';
}

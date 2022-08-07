namespace FileChunker;

public class AppSettings
{
	//TODO: make non nullable
	public string? FilePath { get; set; }
	public int? ChunkSize { get; set; }
}

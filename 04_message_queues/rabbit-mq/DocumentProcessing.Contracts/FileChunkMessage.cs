namespace DocumentProcessing.Contracts;

public class FileChunkMessage
{
    public string FileId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public int TotalChunks { get; set; }

    public byte[] Data { get; set; } = Array.Empty<byte>();
}

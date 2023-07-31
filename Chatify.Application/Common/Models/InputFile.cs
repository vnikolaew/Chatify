namespace Chatify.Application.Common.Models;

public class InputFile
{
    public Stream Data { get; set; } = default!;
    
    public string FileName { get; set; }
    
    public long SizeInBytes => Data.Length;
}
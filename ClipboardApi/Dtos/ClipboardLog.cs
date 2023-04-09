namespace ClipboardApi.Dtos;

public record ClipboardLog(Type Type, string Content);

public enum Type
{
    Text, File, Folder, Diverse
}
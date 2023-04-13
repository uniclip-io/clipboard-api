namespace ClipboardApi.Dtos;

public record FileContent(
    string UserId,
    string ContentId,
    string Type
);
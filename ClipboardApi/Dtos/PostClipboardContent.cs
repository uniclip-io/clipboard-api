namespace ClipboardApi.Dtos;

public record PostClipboardContent
(
    Guid UserId,
    string Type,
    string Content
);
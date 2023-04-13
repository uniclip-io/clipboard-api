namespace ClipboardApi.Dtos;

public record PostClipboardContent
(
    string UserId,
    string Type,
    string Content
);
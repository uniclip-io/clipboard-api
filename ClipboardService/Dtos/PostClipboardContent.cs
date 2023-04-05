namespace ClipboardService.Dtos;

public record PostClipboardContent
(
    Guid userId,
    string contentType,
    string content
);
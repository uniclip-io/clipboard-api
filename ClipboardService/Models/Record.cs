namespace ClipboardService.Models;

public record Record
(
    Guid Id,
    DateTime Date,
    string ContentType,
    string Content
);

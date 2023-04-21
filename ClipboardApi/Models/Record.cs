namespace ClipboardApi.Models;

public record Record
(
    string UserId,
    Guid Id,
    DateTime Date,
    string Type,
    string Content
);
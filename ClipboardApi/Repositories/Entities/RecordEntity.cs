namespace ClipboardApi.Repositories.Entities;

public record RecordEntity
(
    Guid Id,
    Guid ClipboardId,
    DateTime Date,
    string ContentType,
    string Content
);
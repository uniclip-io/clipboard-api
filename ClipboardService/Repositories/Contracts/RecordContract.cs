namespace ClipboardService.Repositories.Contracts;

public record RecordContract
(
    Guid Id,
    Guid ClipboardId,
    DateTime Date,
    string ContentType,
    string Content
);

namespace ClipboardApi.Models;

public record Clipboard
(
    Guid Id,
    Guid UserId,
    List<Record> Records
);
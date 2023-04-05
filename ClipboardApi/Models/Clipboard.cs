namespace ClipboardApi.Models;

public record Clipboard
(
    Guid Id,
    List<Record> Records
);
namespace ClipboardService.Repositories.Contracts;

public record ClipboardContract
(
    Guid Id,
    Guid UserId,
    List<Guid> Records
);

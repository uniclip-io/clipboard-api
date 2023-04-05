using ClipboardApi.Models;
using ClipboardApi.Repositories;

namespace ClipboardApi.Services;

public class ClipboardService
{
    private readonly ClipboardRepository _clipboardRepository;
    private readonly RecordRepository _recordRepository;

    public ClipboardService(ClipboardRepository clipboardRepository, RecordRepository recordRepository)
    {
        _clipboardRepository = clipboardRepository;
        _recordRepository = recordRepository;
    }

    public async Task<Clipboard> CreateClipboard(Guid userId)
    {
        var clipboardContract = await _clipboardRepository.CreateClipboardForUser(userId);
        return new Clipboard(clipboardContract.Id, new List<Record>());
    }

    public async Task<Clipboard?> GetClipboardByUserId(Guid userId)
    {
        var clipboardContract = await _clipboardRepository.GetClipboardByUserId(userId);

        if (clipboardContract == null) return null;

        var recordContracts = await _recordRepository.GetRecordsByClipboardId(clipboardContract.Id);
        var records = recordContracts.Select(r => new Record(r.Id, r.Date, r.ContentType, r.Content)).ToList();

        return new Clipboard(clipboardContract.Id, records);
    }

    public async Task<Record> AddContentToClipboard(Guid clipboardId, string contentType, string content)
    {
        var recordContract = await _recordRepository.AddRecordToClipboard(clipboardId, contentType, content);

        return new Record(recordContract.Id, recordContract.Date, recordContract.ContentType, recordContract.Content);
    }
}
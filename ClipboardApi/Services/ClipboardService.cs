using ClipboardApi.Models;
using ClipboardApi.Repositories;
using Type = ClipboardApi.Dtos;

namespace ClipboardApi.Services;

public class ClipboardService
{
    private readonly ClipboardRepository _clipboardRepository;
    private readonly RecordRepository _recordRepository;
    private readonly RabbitMqService _rabbitMqService;

    public ClipboardService(ClipboardRepository clipboardRepository, RecordRepository recordRepository, RabbitMqService rabbitMqService)
    {
        _clipboardRepository = clipboardRepository;
        _recordRepository = recordRepository;
        _rabbitMqService = rabbitMqService;

        _rabbitMqService.OnFileUploaded(async fileContent =>
        {
            var userId = Guid.Parse(fileContent.UserId);
            var clipboard = await GetClipboardByUserId(userId) ?? await CreateClipboard(userId);
            await AddContentToClipboard(clipboard.Id, fileContent.Type, fileContent.ContentId);
        });
    }

    public async Task<Clipboard> CreateClipboard(Guid userId)
    {
        var clipboardContract = await _clipboardRepository.CreateClipboardForUser(userId);
        return new Clipboard(clipboardContract.Id, userId, new List<Record>());
    }

    public async Task<Clipboard?> GetClipboardByUserId(Guid userId)
    {
        var clipboardContract = await _clipboardRepository.GetClipboardByUserId(userId);

        if (clipboardContract == null)
        {
            return null;
        }

        var recordContracts = await _recordRepository.GetRecordsByClipboardId(clipboardContract.Id);
        var records = recordContracts.Select(r => new Record(r.Id, r.Date, r.Type, r.Content)).ToList();

        return new Clipboard(clipboardContract.Id, userId, records);
    }

    public async Task<Record> AddContentToClipboard(Guid clipboardId, string type, string content)
    {
        var recordContract = await _recordRepository.AddRecordToClipboard(clipboardId, type, content);
        var record = new Record(recordContract.Id, recordContract.Date, recordContract.Type, recordContract.Content);

        _rabbitMqService.PublishRecord(record);
        
        return record;
    }
}
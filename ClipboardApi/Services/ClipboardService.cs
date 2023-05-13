using ClipboardApi.Models;
using ClipboardApi.Repositories;
using Type = ClipboardApi.Dtos;

namespace ClipboardApi.Services;

public class ClipboardService
{
    private readonly ClipboardRepository _clipboardRepository;
    private readonly RecordRepository _recordRepository;
    private readonly RabbitMqService _rabbitMqService;
    private readonly EncryptionService _encryptionService;

    public ClipboardService(ClipboardRepository clipboardRepository, RecordRepository recordRepository, RabbitMqService rabbitMqService, EncryptionService encryptionService)
    {
        _clipboardRepository = clipboardRepository;
        _recordRepository = recordRepository;
        _rabbitMqService = rabbitMqService;
        _encryptionService = encryptionService;

        _rabbitMqService.OnFileUploaded(async fileContent =>
        {
            var userId = fileContent.UserId;
            var clipboard = await GetClipboardByUserId(userId) ?? await CreateClipboard(userId);
            await AddContentToClipboard(userId, clipboard.Id, fileContent.Type, fileContent.ContentId);
        });
    }

    public async Task<Clipboard> CreateClipboard(string userId)
    {
        var clipboardContract = await _clipboardRepository.CreateClipboardForUser(userId);
        return new Clipboard(clipboardContract.Id, userId, new List<Record>());
    }

    public async Task<Clipboard?> GetClipboardByUserId(string userId)
    {
        var clipboardContract = await _clipboardRepository.GetClipboardByUserId(userId);

        if (clipboardContract == null)
        {
            return null;
        }

        var recordContracts = await _recordRepository.GetRecordsByClipboardId(clipboardContract.Id);
        var records = recordContracts.Select(r => new Record(userId, r.Id, r.Date, r.Type, _encryptionService.Decrypt(r.Content))).ToList();

        return new Clipboard(clipboardContract.Id, userId, records);
    }

    public async Task<Record> AddContentToClipboard(string userId, Guid clipboardId, string type, string content)
    {
        var encrypted = _encryptionService.Encrypt(content);
        var recordContract = await _recordRepository.AddRecordToClipboard(clipboardId, type, encrypted);
        var record = new Record(userId, recordContract.Id, recordContract.Date, recordContract.Type, content);

        _rabbitMqService.PublishRecord(record);
        
        return record;
    }

    public async Task<bool> RemoveContentFromClipboard(Guid recordId)
    {
        return await _recordRepository.RemoveContentFromClipboard(recordId);
    }
}
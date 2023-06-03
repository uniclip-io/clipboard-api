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
            var clipboardId = await GetClipboardByUserId(userId) ?? await CreateClipboard(userId);
            await AddContentToClipboard(userId, clipboardId, fileContent.Type, fileContent.ContentId);
        });
    }

    public async Task<Guid> CreateClipboard(string userId)
    {
        var clipboardContract = await _clipboardRepository.CreateClipboardForUser(userId);
        return clipboardContract.Id;
    }

    public async Task<Guid?> GetClipboardByUserId(string userId)
    {
        var clipboardContract = await _clipboardRepository.GetClipboardByUserId(userId);
        return clipboardContract?.Id;
    }

    public async Task<List<Record>?> GetRecordsByUserId(string userId)
    {
        var clipboardId = await GetClipboardByUserId(userId);

        if (clipboardId == null)
        {
            return null;
        }
        
        var recordEntities = await _recordRepository.GetRecordsByClipboardId(clipboardId.Value);
        return recordEntities.Select(r => new Record(userId, r.Id, r.Date, r.Type, _encryptionService.Decrypt(r.Content))).ToList();
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
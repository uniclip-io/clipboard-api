using ClipboardApi.Repositories.Entities;
using MongoDB.Driver;

namespace ClipboardApi.Repositories;

public class RecordRepository
{
    private readonly IMongoCollection<RecordEntity> _records;

    public RecordRepository(string connectionString)
    {
        var mongoClient = new MongoClient(connectionString);
        var database = mongoClient.GetDatabase("clipboard-api");
        _records = database.GetCollection<RecordEntity>("records");
    }

    public async Task<List<RecordEntity>> GetRecordsByClipboardId(Guid clipboardId)
    {
        var devices = await _records.Find(r => r.ClipboardId == clipboardId).ToListAsync();
        devices.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
        return devices;
    }

    public async Task<RecordEntity> AddRecordToClipboard(Guid clipboardId, string contentType, string content)
    {
        var recordContract = new RecordEntity(Guid.NewGuid(), clipboardId, DateTime.Now, contentType, content);
        await _records.InsertOneAsync(recordContract);
        return recordContract;
    }
}
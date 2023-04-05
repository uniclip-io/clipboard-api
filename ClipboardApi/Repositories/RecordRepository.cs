using ClipboardApi.Repositories.Contracts;
using MongoDB.Driver;

namespace ClipboardApi.Repositories;

public class RecordRepository
{
    private readonly IMongoCollection<RecordContract> _records;

    public RecordRepository(string connectionString)
    {
        var mongoClient = new MongoClient(connectionString);
        var database = mongoClient.GetDatabase("clipboard-api");
        _records = database.GetCollection<RecordContract>("records");
    }

    public async Task<List<RecordContract>> GetRecordsByClipboardId(Guid clipboardId)
    {
        var devices = await _records.Find(r => r.ClipboardId == clipboardId).ToListAsync();
        devices.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
        return devices;
    }

    public async Task<RecordContract> AddRecordToClipboard(Guid clipboardId, string contentType, string content)
    {
        var recordContract = new RecordContract(Guid.NewGuid(), clipboardId, DateTime.Now, contentType, content);
        await _records.InsertOneAsync(recordContract);
        return recordContract;
    }
}
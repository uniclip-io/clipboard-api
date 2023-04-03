using ClipboardService.Repositories.Contracts;
using MongoDB.Driver;

namespace ClipboardService.Repositories;

public class RecordRepository
{
    private readonly IMongoCollection<RecordContract> _records;

    public RecordRepository(string connectionString)
    {
        var mongoClient = new MongoClient(connectionString);
        var database = mongoClient.GetDatabase("clipboard-service");
        _records = database.GetCollection<RecordContract>("records");
    }
    
    public async Task<List<RecordContract>> GetRecordsByClipboardId(Guid clipboardId)
    {
        var query = await _records.FindAsync(r => r.ClipboardId == clipboardId);

        var records = await query.ToListAsync();
        records.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
        
        return records;
    }

    public async Task<RecordContract> AddRecordToClipboard(Guid clipboardId, string contentType, string content)
    {
        var recordContract = new RecordContract(Guid.NewGuid(), clipboardId, DateTime.Now, contentType, content);

        await _records.InsertOneAsync(recordContract);

        return recordContract;
    }
}
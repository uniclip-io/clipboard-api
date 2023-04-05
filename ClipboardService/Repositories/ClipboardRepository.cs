using ClipboardService.Repositories.Contracts;
using MongoDB.Driver;

namespace ClipboardService.Repositories;

public class ClipboardRepository
{
    private readonly IMongoCollection<ClipboardContract> _clipboards;

    public ClipboardRepository(string connectionString)
    {
        var mongoClient = new MongoClient(connectionString);
        var database = mongoClient.GetDatabase("clipboard-service");
        _clipboards = database.GetCollection<ClipboardContract>("clipboards");
    }

    public async Task<ClipboardContract> CreateClipboardForUser(Guid userId)
    {
        var found = await GetClipboardByUserId(userId);

        if (found != null) throw new ArgumentException("User already has a clipboard.");

        var clipboardContract = new ClipboardContract(Guid.NewGuid(), userId);
        await _clipboards.InsertOneAsync(clipboardContract);
        return clipboardContract;
    }

    public async Task<ClipboardContract> GetClipboardByUserId(Guid userId)
    {
        return await _clipboards.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }
}
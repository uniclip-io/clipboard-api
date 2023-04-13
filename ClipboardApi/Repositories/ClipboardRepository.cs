using ClipboardApi.Repositories.Entities;
using MongoDB.Driver;

namespace ClipboardApi.Repositories;

public class ClipboardRepository
{
    private readonly IMongoCollection<ClipboardEntity> _clipboards;

    public ClipboardRepository(string connectionString)
    {
        var mongoClient = new MongoClient(connectionString);
        var database = mongoClient.GetDatabase("clipboard-api");
        _clipboards = database.GetCollection<ClipboardEntity>("clipboards");
    }

    public async Task<ClipboardEntity> CreateClipboardForUser(string userId)
    {
        var found = await GetClipboardByUserId(userId);

        if (found != null)
        {
            throw new ArgumentException("User already has a clipboard.");
        }

        var clipboardContract = new ClipboardEntity(Guid.NewGuid(), userId);
        await _clipboards.InsertOneAsync(clipboardContract);
        return clipboardContract;
    }

    public async Task<ClipboardEntity> GetClipboardByUserId(string userId)
    {
        return await _clipboards.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }
}
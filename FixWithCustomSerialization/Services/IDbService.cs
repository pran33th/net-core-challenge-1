using FixWithCustomSerialization.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using FixWithCustomSerialization.Controllers;

namespace FixWithCustomSerialization.Services
{
    public class IDBService
    {
        private readonly IMongoCollection<MediaFile> _mediaCollection;

        public IDBService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _mediaCollection = database.GetCollection<MediaFile>(mongoDBSettings.Value.CollectionName);
        }

        public async Task<MediaFile?> GetMediaAsync(string id)
        {
            return await _mediaCollection.Find(x => x.Name == id).SingleAsync();
        }

        public async Task UpdateMediaAsync(MediaFile newMediaFile)
        {
            await _mediaCollection.InsertOneAsync(newMediaFile);
        }

    }

}
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using veterinariweb.Models;

namespace veterinariweb.Service.MongoDB
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(IOptions<MongoDBSettings> settings)
        {
            var config = settings.Value;

            var client = new MongoClient(config.ConnectionString);

            _database = client.GetDatabase(config.DatabaseName);
        }

        public IMongoDatabase Database => _database;

        // comprueba la conexión con Mongo
        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                await _database.RunCommandAsync<BsonDocument>(
                    new BsonDocument("ping", 1));

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}

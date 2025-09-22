using ControleEstoque.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace ControleEstoque.Api.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Cliente> Clientes => _database.GetCollection<Cliente>("Clientes");
        public IMongoCollection<User> Usuarios => _database.GetCollection<User>("Usuarios");
        public IMongoCollection<Produto> Produtos => _database.GetCollection<Produto>("Produtos");
        public IMongoCollection<Pedido> Pedidos => _database.GetCollection<Pedido>("Pedidos");
    }
}
using ControleEstoque.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ControleEstoque.Api.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            // Transforma a connection string em configurações que podemos modificar
            var mongoUrl = new MongoUrl(settings.Value.ConnectionString);
            var clientSettings = MongoClientSettings.FromUrl(mongoUrl);

            // Esta é a configuração chave que resolve o problema
            clientSettings.SslSettings = new SslSettings() { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };

            // Cria o cliente com as novas configurações
            var client = new MongoClient(clientSettings);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Cliente> Clientes => _database.GetCollection<Cliente>("Customer");
        public IMongoCollection<User> Usuarios => _database.GetCollection<User>("User");
        public IMongoCollection<Produto> Produtos => _database.GetCollection<Produto>("Products");
        public IMongoCollection<Pedido> Pedidos => _database.GetCollection<Pedido>("Orders");
    }
}
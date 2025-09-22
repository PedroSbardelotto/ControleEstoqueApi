using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ControleEstoque.Api.Models
{
    public class Pedido
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }
        public required string NomeProduto { get; set; }
        public required int QuantidadeProduto { get; set; }
        public required string NomeCliente { get; set; }
    }
}
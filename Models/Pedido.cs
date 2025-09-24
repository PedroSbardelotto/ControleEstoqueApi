using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ControleEstoque.Api.Models
{
    public class Pedido
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string NomeProduto { get; set; }
        public int QuantidadeProduto { get; set; }
        public string NomeCliente { get; set; }
    }
}
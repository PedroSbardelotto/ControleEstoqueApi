using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ControleEstoque.Api.Models
{
    public class Cliente
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }
        public required string Nome { get; set; }
        public required string CNPJ { get; set; }
        public required string Email { get; set; }
        public required string Endereco { get; set; }
    }
}
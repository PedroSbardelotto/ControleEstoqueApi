using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ControleEstoque.Api.Models
{
    public class Cliente
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Nome { get; set; }
        public string CNPJ { get; set; }
        public string Email { get; set; }
        public string Endereco { get; set; }
    }
}
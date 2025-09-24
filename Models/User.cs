using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ControleEstoque.Api.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        public string? Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Tipo { get; set; }
        public bool Status { get; set; } // "bool" é ótimo para Ativo/Inativo
    }
}

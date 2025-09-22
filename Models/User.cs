using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ControleEstoque.Api.Models
{
  public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        public required string Id { get; set; }
     public required string Nome { get; set; }
     public required string Email { get; set; }
     public required string Tipo { get; set; }
     public required bool Status { get; set; } // "bool" é ótimo para Ativo/Inativo
    }
}

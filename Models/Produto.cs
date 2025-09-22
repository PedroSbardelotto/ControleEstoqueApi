using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ControleEstoque.Api.Models
{
    public class Produto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public required string Id { get; set; }
        public required string Nome { get; set; }
        public required int Quantidade { get; set; }
        public required string Tipo { get; set; }
        public required decimal Preco { get; set; }

        public string Status
        {
            get
            {
                return Quantidade > 0 ? "Em Estoque" : "Sem Estoque";
            }
        }
    }
}
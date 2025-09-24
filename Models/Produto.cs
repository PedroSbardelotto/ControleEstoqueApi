using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ControleEstoque.Api.Models
{
    public class Produto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string? Id { get; set; }
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public string Tipo { get; set; }
        public decimal Preco { get; set; }

        public string Status
        {
            get
            {
                return Quantidade > 0 ? "Em Estoque" : "Sem Estoque";
            }
        }
    }
}
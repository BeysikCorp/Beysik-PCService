using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Beysik_PCService.Models
{
    public class ProductDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string ProductsCollectionName { get; set; } = null!;
    }

    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; } = null!;

        [BsonElement("Brand")]
        public string Brand { get; set; } = null!;

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("Size")]
        public string Size { get; set; } = null!;

        [BsonElement("Description")]
        public string Description { get; set; } = null!;

        [BsonElement("Category")]
        public string Category { get; set; } = null!;

        [BsonElement("Stock")]
        public int Stock { get; set; }

        [BsonElement("ImageUrl")]
        public string ImageUrl { get; set; } = null!;
    }
}

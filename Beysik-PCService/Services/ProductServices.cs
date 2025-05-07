using Beysik_PCService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Beysik_PCService.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _productsCollection;

        public ProductService(
            IOptions<ProductDatabaseSettings> productDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                productDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                productDatabaseSettings.Value.DatabaseName);

            _productsCollection = mongoDatabase.GetCollection<Product>(
                productDatabaseSettings.Value.ProductsCollectionName);
        }

        public async Task<List<Product>> GetAsync() =>
            await _productsCollection.Find(_ => true).ToListAsync();

        public async Task<Product?> GetAsync(string id) =>
            await _productsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Product newProduct) =>
            await _productsCollection.InsertOneAsync(newProduct);

        public async Task UpdateAsync(string id, Product updatedProduct) =>
            await _productsCollection.ReplaceOneAsync(x => x.Id == id, updatedProduct);

        public async Task RemoveAsync(string id) =>
            await _productsCollection.DeleteOneAsync(x => x.Id == id);

    }
}

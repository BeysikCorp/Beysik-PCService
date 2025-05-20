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
        await _productsCollection.Find(p => p.IsActive).ToListAsync();

        public async Task<Product?> GetAsync(string id) =>
            await _productsCollection.Find(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();

        public async Task CreateAsync(Product newProduct) =>
            await _productsCollection.InsertOneAsync(newProduct);

        public async Task UpdateAsync(string id, Product updatedProduct) =>
            await _productsCollection.ReplaceOneAsync(x => x.Id == id, updatedProduct);

        public async Task RemoveAsync(string id)
        {
            var update = Builders<Product>.Update.Set(p => p.IsActive, false);
            await _productsCollection.UpdateOneAsync(x => x.Id == id, update);
        }
        public async Task<bool> ReduceStockAsync(string productId, int quantity)
        {
            var product = await GetAsync(productId);
            if (product == null || product.Stock < quantity)
                return false;

            product.Stock -= quantity;
            await UpdateAsync(product.Id, product);
            return true;
        }
    }
}

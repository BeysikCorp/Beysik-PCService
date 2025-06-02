using Beysik_PCService.Models;
using Beysik_Common;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;
using static Beysik_Common.RabbitMqConsumerService;

namespace Beysik_PCService.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _productsCollection;
        private readonly RabbitMqHelper _rabbitMqHelper;
        private readonly RabbitMqEventAggregator _rabbitMqEventAggregator;

        public ProductService(
            RabbitMqEventAggregator eventAggregator, RabbitMqHelper rabbitMq,
            IOptions<ProductDatabaseSettings> productDatabaseSettings)
        {
            _rabbitMqEventAggregator = eventAggregator;
            _rabbitMqHelper = rabbitMq;
            var mongoClient = new MongoClient(
                productDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                productDatabaseSettings.Value.DatabaseName);

            _productsCollection = mongoDatabase.GetCollection<Product>(
                productDatabaseSettings.Value.ProductsCollectionName);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e == null || string.IsNullOrEmpty(e.Message))
            {
                return;
            }
            List<string>? message = e.Message.Split('.').ToList();
            if (e.Message.Contains("order.created"))
            {
                string productId = message[0];
                int quantity = int.Parse(message[1]);
                ReduceStockAsync(productId, quantity).Wait();
                _rabbitMqHelper.PublishMessage(
                    $"{productId}.{quantity}.order.allocated",
                    "order.toorder",
                    "order.api.frompc",
                    ExchangeType.Topic).Wait();
            }
            if (e.Message.Contains("order.cancelled"))
            {
                string productId = message[0];
                int quantity = int.Parse(message[1]);
                IncreaseStockAsync(productId, quantity).Wait();
            }

        }

        public async Task<List<Product>> GetAsync()
        {
            // Retrieves all active products from the collection
            return await _productsCollection.Find(p => p.IsActive).ToListAsync();
        }

        public async Task<Product?> GetAsync(string id)
        {
            // Retrieves a single active product by its Id
            return await _productsCollection.Find(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();
        }

        public async Task<Product?> GetByTagAsync(string tag)
        {
            // Retrieves the first product that contains the specified tag
            return await _productsCollection.Find(x => x.Tags.Contains(tag)).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Product newProduct)
        {
            // Inserts a new product into the collection
            await _productsCollection.InsertOneAsync(newProduct);
        }

        public async Task UpdateAsync(string id, Product updatedProduct)
        {
            // Replaces the product with the specified Id with the updated product
            await _productsCollection.ReplaceOneAsync(x => x.Id == id, updatedProduct);
        }

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
        public async Task<bool> IncreaseStockAsync(string productId, int quantity)
        {
            var product = await GetAsync(productId);
            if (product == null)
                return false;
            product.Stock += quantity;
            await UpdateAsync(product.Id, product);
            return true;
        }
    }
}
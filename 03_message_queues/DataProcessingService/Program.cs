using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CommonLogic;

namespace DataProcessingService
{
    public class Program
    {
        private const string QueueName = "file_processing_queue";
        private static readonly List<ChunkFileFactory> chunkFileFactories = new();

        static async Task Main(string[] args)
        {
            string outputFolderPath = DirectionService.SetupFolder("Output");

            await using var channel = await CreateRabbitMqChannelAsync();

            Console.WriteLine("Listening for messages from the queue...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, eventArgs) =>
            {
                await ProcessMessageAsync(eventArgs, outputFolderPath, channel);
            };

            await channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine("Press [Enter] to exit.");
            Console.ReadLine();
        }

        private static async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs, string outputFolderPath, IChannel channel)
        {
            Chunk chunk = Chunk.FromMessageBytes(eventArgs.Body.ToArray());
            Console.WriteLine($"Received chunk: {chunk.ToString()}");

            ChunkFileFactory chunkFileFactory = GetProperFactory(chunk);
            chunkFileFactory.AddChunk(chunk);

            if (chunkFileFactory.CanCreateFromChunks())
            {
                ChunkFile chunkFile = chunkFileFactory.CreateFromChunks();
                chunkFileFactories.Remove(chunkFileFactory);
                await MatelializeFile(outputFolderPath, chunkFile);
            }

            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        }

        private static ChunkFileFactory GetProperFactory(Chunk chunk)
        {
            var chunkFileFactory = chunkFileFactories.FirstOrDefault(cff => cff.FileName == chunk.FileName);
            if (chunkFileFactory == null)
            {
                chunkFileFactory = new ChunkFileFactory(chunk.FileName);
                chunkFileFactories.Add(chunkFileFactory);
            }
            return chunkFileFactory;
        }

        private static async Task MatelializeFile(string outputFolderPath, ChunkFile chunkFile)
        {
            Console.WriteLine($"All chunks received for file '{chunkFile.FileName}'. Materializing...");

            var outputPath = Path.Combine(outputFolderPath, chunkFile.FileName);
            await File.WriteAllBytesAsync(outputPath, chunkFile.Bytes);

            Console.WriteLine($"File '{chunkFile.FileName}' has been successfully saved to '{outputPath}'.");
        }

        private static async Task<IChannel> CreateRabbitMqChannelAsync()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    ClientProvidedName = "MainProcessingService:FileProcessor"
                };

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                await EnsureQueueExists(channel);
                return channel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create RabbitMQ channel: {ex.Message}");
                throw;
            }
        }

        private static async Task EnsureQueueExists(IChannel channel)
        {
            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }
    }
}

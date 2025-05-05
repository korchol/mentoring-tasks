using CommonLogic;
using RabbitMQ.Client;

namespace DataCaptureService
{
    public class Program
    {
        private const string QueueName = "file_processing_queue";

        static void Main(string[] args)
        {
            using var watcher = new FileSystemWatcher(DirectionService.SetupFolder("Input"))
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "*.*"
            };

            watcher.Created += async (sender, e) =>
            {
                var chunkFile = await ChunkFileFactory.CreateFromFilePathAsync(e.FullPath);
                await SendFileToQueueAsync(chunkFile);
            };

            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Data Capture Service is running. Press [Enter] to exit...");
            Console.ReadLine();
        }

        private static async Task SendFileToQueueAsync(ChunkFile chunkFile)
        {
            Console.WriteLine($"Preparing to send file '{chunkFile.FileName}' in {chunkFile.ChunksCount} chunks...");

            await using var channel = await CreateRabbitMqChannelAsync();

            foreach (Chunk chunk in chunkFile.GetChunks())
            {
                await PublishAsync(channel, chunk.ToMessageBytes());
                Console.WriteLine($"Sent to queue: {chunk.ToString()}");
            }

            Console.WriteLine($"All chunks of file '{chunkFile.FileName}' have been sent to the queue.");
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

        private static async Task PublishAsync(IChannel channel, byte[] message)
        {
            var props = new BasicProperties
            {
                ContentType = "text/plain",
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: QueueName,
                mandatory: false,
                basicProperties: props,
                body: new ReadOnlyMemory<byte>(message)
            );
        }
    }
}
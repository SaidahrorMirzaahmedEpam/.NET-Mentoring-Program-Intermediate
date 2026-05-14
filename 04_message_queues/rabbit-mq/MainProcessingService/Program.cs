using System.Text;
using System.Text.Json;
using DocumentProcessing.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const string QueueName = "document_chunks_queue";

var tempFolder = Path.Combine(AppContext.BaseDirectory, "Temp");
var outputFolder = Path.Combine(AppContext.BaseDirectory, "Output");

Directory.CreateDirectory(tempFolder);
Directory.CreateDirectory(outputFolder);

Console.WriteLine("MainProcessingService started.");
Console.WriteLine($"Temporary chunks folder: {tempFolder}");
Console.WriteLine($"Output folder: {outputFolder}");

var rabbitMqFactory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};

await using var connection = await rabbitMqFactory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: QueueName,
    durable: true,
    exclusive: false,
    autoDelete: false);

await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (_, eventArgs) =>
{
    try
    {
        var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        var message = JsonSerializer.Deserialize<FileChunkMessage>(json);

        if (message is null)
        {
            Console.WriteLine("Received an empty or invalid message.");
            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            return;
        }

        await SaveChunkAsync(message, tempFolder);
        Console.WriteLine($"Received chunk {message.ChunkIndex + 1}/{message.TotalChunks} for {message.FileName}.");

        if (AllChunksReceived(message, tempFolder))
        {
            await RebuildFileAsync(message, tempFolder, outputFolder);
        }

        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error receiving message: {ex.Message}");
        await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true);
    }
};

await channel.BasicConsumeAsync(
    queue: QueueName,
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Waiting for chunks. Press Enter to stop the service.");
Console.ReadLine();

static async Task SaveChunkAsync(FileChunkMessage message, string tempFolder)
{
    var fileTempFolder = Path.Combine(tempFolder, message.FileId);
    Directory.CreateDirectory(fileTempFolder);

    var chunkPath = Path.Combine(fileTempFolder, $"{message.ChunkIndex}.chunk");
    await File.WriteAllBytesAsync(chunkPath, message.Data);
}

static bool AllChunksReceived(FileChunkMessage message, string tempFolder)
{
    var fileTempFolder = Path.Combine(tempFolder, message.FileId);

    for (var index = 0; index < message.TotalChunks; index++)
    {
        var chunkPath = Path.Combine(fileTempFolder, $"{index}.chunk");
        if (!File.Exists(chunkPath))
        {
            return false;
        }
    }

    return true;
}

static async Task RebuildFileAsync(FileChunkMessage message, string tempFolder, string outputFolder)
{
    var fileTempFolder = Path.Combine(tempFolder, message.FileId);
    var outputPath = Path.Combine(outputFolder, message.FileName);

    if (File.Exists(outputPath))
    {
        File.Delete(outputPath);
    }

    await using var outputStream = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write);

    for (var index = 0; index < message.TotalChunks; index++)
    {
        var chunkPath = Path.Combine(fileTempFolder, $"{index}.chunk");
        var chunkData = await File.ReadAllBytesAsync(chunkPath);
        await outputStream.WriteAsync(chunkData);
    }

    Directory.Delete(fileTempFolder, recursive: true);
    Console.WriteLine($"Rebuilt file: {outputPath}");
}

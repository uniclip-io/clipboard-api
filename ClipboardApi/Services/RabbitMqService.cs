using ClipboardApi.Dtos;
using System.Text;
using ClipboardApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace ClipboardApi.Services;

public class RabbitMqService : IDisposable
{
    private const string ClipboardQueueName = "clipboard-queue";
    private const string ClipboardExchangeName = "clipboard-exchange";
    private const string ContentQueueName = "content-queue";
    private const string ContentExchangeName = "content-exchange";

    private readonly IConnection _connection;
    private readonly IModel _clipboardChannel;
    private readonly IModel _contentChannel;

    public RabbitMqService(string username, string password, string uri)
    {
        var factory = new ConnectionFactory
        {
            UserName = username,
            Password = password,
            Uri = new Uri(uri)
        };
        _connection = factory.CreateConnection();

        _clipboardChannel = _connection.CreateModel();
        _clipboardChannel.QueueDeclare(ClipboardQueueName, true, false, false, null);
        _clipboardChannel.ExchangeDeclare(ClipboardExchangeName, ExchangeType.Topic, true, false, null);
        _clipboardChannel.QueueBind(ClipboardQueueName, ClipboardExchangeName, "record.created");

        _contentChannel = _connection.CreateModel();
        _contentChannel.QueueDeclare(ContentQueueName, true, false, false, null);
        _clipboardChannel.ExchangeDeclare(ContentExchangeName, ExchangeType.Topic, true, false, null);
        _clipboardChannel.QueueBind(ContentQueueName, ContentExchangeName, "file.uploaded");
    }

    public void OnFileUploaded(Action<FileContent> messageHandler)
    {
        var consumer = new EventingBasicConsumer(_contentChannel);
        consumer.Received += (_, args) =>
        {
            var raw = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonConvert.DeserializeObject<FileContent>(raw);

            if (message != null)
            {
                messageHandler(message);
            }
        };
        _contentChannel.BasicConsume(ContentQueueName, true, consumer);
    }

    public void PublishRecord(Record record)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new LowercaseContractResolver()
        };

        var json = JsonConvert.SerializeObject(record, settings);
        var message = Encoding.UTF8.GetBytes(json);

        _clipboardChannel.BasicPublish(ClipboardExchangeName, "record.created", null, message);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _clipboardChannel.Close();
        _contentChannel.Close();
        _connection.Close();
    }
}

internal class LowercaseContractResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        return propertyName.ToLower();
    }
}
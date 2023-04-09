using ClipboardApi.Dtos;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace ClipboardApi.Services;

public class RabbitMqService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMqService(string hostName)
    {
        _queueName = "clipboard-queue";
        const string exchangeName = "clipboard-message";

        var factory = new ConnectionFactory() { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(_queueName, true, false, false, null);
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true, false, null);
        _channel.QueueBind(_queueName, exchangeName, "clipboard");
    }

    public void OnClipboard(Action<ClipboardLog> messageHandler)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, args) =>
        {
            var raw = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonConvert.DeserializeObject<ClipboardLog>(raw);

            // if (message != null)
            // {
                messageHandler(message);
            // }
        };

        _channel.BasicConsume(_queueName, true, consumer);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _channel.Close();
        _connection.Close();
    }
}
using System.Collections.Concurrent;
using System.Text;
using NuGet.Protocol;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Publisher.Client
{
    public class RpcClient : IDisposable
    {
        private const string QUEUE_NAME = "rpc_queue";

        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

        public RpcClient()
        {
            Console.WriteLine("Started RpcClient()");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",
                Password = "pass",
                VirtualHost = "/"
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            // declare a server-named queue
            replyQueueName = channel.QueueDeclare().QueueName;
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                    return;
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                tcs.TrySetResult(response);
            };

            channel.BasicConsume(consumer: consumer,
                                 queue: replyQueueName,
                                 autoAck: true);
            Console.WriteLine("Completed RpcClient()");
        }

        public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Started CallAsync");
            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var tcs = new TaskCompletionSource<string>();
            callbackMapper.TryAdd(correlationId, tcs);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: QUEUE_NAME,
                                 basicProperties: props,
                                 body: messageBytes);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
            Console.WriteLine(tcs.Task);
            Console.WriteLine("tcs.ToJson " + tcs.ToJson());
            Console.WriteLine("Completed CallAsync");
            return tcs.Task;
        }

        public void Dispose()
        {
            connection.Close();
        }
    }

    public class Rpc
    {
        public async Task<string> Main(string s)
        {
            Console.WriteLine("RPC Client");
            Console.WriteLine("Exiting");
            return await InvokeAsync(s);
        }

        private async Task<string> InvokeAsync(string s)
        {
            using var rpcClient = new RpcClient();

            Console.WriteLine(" [x] Requesting fib({0})", s);
            return await rpcClient.CallAsync(s);
        }
    }
}
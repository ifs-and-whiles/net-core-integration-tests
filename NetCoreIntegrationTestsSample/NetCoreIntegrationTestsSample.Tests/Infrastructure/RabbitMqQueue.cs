using System;
using System.IO;
using System.Reflection;
using System.Text;
using RabbitMQ.Client;

namespace NetCoreIntegrationTestsSample.Tests.Infrastructure
{
    public class RabbitMqQueue
    {
        private readonly IConnection _connection;
        
       public RabbitMqQueue(
          string hostUrl,
          string user,
          string password)
       {
          var hostUri = new Uri(hostUrl);
          var connectionFactory = new ConnectionFactory
          {
             HostName = hostUri.Host,
             UserName = user,
             Password = password,
             Port = hostUri.Port
          };
 
          _connection = connectionFactory.CreateConnection();
       }

      public void PurgeQueues(params string[] queues)
      {
         foreach (var queue in queues)
         {
            PurgeQueue(queue);
         }
      }

      public void PurgeQueue(string queue)
      {
         // RabbitMQ throws exception when queue does not exist. There is no possibility to check that queue exists.
         try
         {
            using var channel = _connection.CreateModel();
            channel.QueuePurge(queue);
         }
         catch (Exception e)
         { }
      }

      public void DeleteQueue(string queueName)
      {
         using var channel = _connection.CreateModel();
         
         channel.ExchangeDelete(queueName);
         channel.QueueDelete(queueName);
      }
      
      public void ConfigureListeningQueue(string queueName, Type messageType)
      {
         using var channel = _connection.CreateModel();
         
         var expectedMessageExchangeName = $"{messageType.Namespace}:{GetTypeName(messageType)}";
         
         channel.QueueDeclare(queueName, false, false);
         channel.ExchangeDeclare($"{expectedMessageExchangeName}", "fanout", true);
         channel.QueueBind(queueName, expectedMessageExchangeName, routingKey: "", arguments: null);
      }

      public string ReadRawMessageFromQueue(string queue)
      {
         using var channel = _connection.CreateModel();
         var rabbitGetResult = channel.BasicGet(queue, true);

         // The queue does not contain messages
         if (rabbitGetResult == null)
            return null;

         var body = rabbitGetResult.Body;
         
         using var stream = new MemoryStream(body.Span.ToArray());
         using var streamReader = new StreamReader(stream);

         return streamReader.ReadToEnd();
      }

      public void Dispose()
      {
         _connection?.Dispose();
      }
      
      private string GetTypeName(Type type)
      {
         if (type.MemberType == MemberTypes.NestedType)
         {
            return string.Concat(GetTypeName(type.DeclaringType), "-", type.Name);
         }
         return type.Name;
      }
    }
}
using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQChat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please specify chat room name:");
            var chatRoomName = Console.ReadLine();

            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var exchangeName = "chatroom";
            var queueName = Guid.NewGuid().ToString();

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, true, true, true);
            channel.QueueBind(queueName, exchangeName, chatRoomName);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, eventargs) =>
            {
                var text = Encoding.UTF8.GetString(eventargs.Body.ToArray());
                Console.WriteLine(text);
            };

            channel.BasicConsume(queueName, true, consumer);

            var input = Console.ReadLine();

            while(input!="")
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                channel.BasicPublish(exchangeName, chatRoomName, null, bytes);
                input = Console.ReadLine();
            }

            channel.Close();
            connection.Close();
        }
    }
}

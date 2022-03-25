using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttSubscriber
{
    internal class Client
    {
        static MqttFactory mqttFactory = new MqttFactory();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Client");
            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                try
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("test.mosquitto.org", 1883).Build();

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    Console.WriteLine("Client broker connection successful.");

                    await SubscribeTopic(mqttClient);

                    mqttClient.UseApplicationMessageReceivedHandler(e => {
                        Console.WriteLine("Topic : " + e.ApplicationMessage.Topic + " Recieved message: " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                    });

                    await PublishMessage(mqttClient);

                }
                catch (Exception)
                {
                    Console.WriteLine("Cannot connect to server");
                }
            }
        }

        private static async Task SubscribeTopic(IMqttClient mqttClient)
        {
            try
            {
                var mqttSubscriberOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                                      .WithTopicFilter(f => { f.WithTopic("NODEServer"); }).Build();

                await mqttClient.SubscribeAsync(mqttSubscriberOptions, CancellationToken.None);

                Console.WriteLine("Subscribed to NODEServer");
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot subscripte to NODEServer");
                throw;
            }
        }

        private static async Task PublishMessage(IMqttClient mqttClient)
        {
            if (mqttClient.IsConnected)
            {
                while (true)
                {
                    var message = "";
                    Console.WriteLine("Enter a message to publish or -1 to exit");
                    message = Console.ReadLine();
                    if (message == "-1") break;
                    var messageBuilder = new MqttApplicationMessageBuilder().WithTopic("NODEClient").WithPayload(message).Build();
                    await mqttClient.PublishAsync(messageBuilder, CancellationToken.None);
                    Console.WriteLine("Message published.");
                }
            }
            else
            {
                Console.WriteLine("Not connected");
            }
        }
    }
}

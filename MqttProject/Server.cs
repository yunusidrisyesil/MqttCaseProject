using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client;
using System.Text;

namespace MqttProject
{
    internal class Server
    {
        static MqttFactory mqttFactory = new MqttFactory();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Server");
            try
            {
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("test.mosquitto.org", 1883).Build();

                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    Console.WriteLine("Server broker connection successful.");

                    await SubscribeTopic(mqttClient);

                    mqttClient.UseApplicationMessageReceivedHandler(e =>
                    {
                        Console.WriteLine("Topic : " + e.ApplicationMessage.Topic + " Recieved message: " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                    });

                    await PublishMessage(mqttClient);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot connect to broker");
                throw;
            }
        }

        public static async Task PublishMessage(IMqttClient mqttClient)
        {
            if (mqttClient.IsConnected)
            {
                while (true)
                {
                    var message = "";
                    Console.WriteLine("Enter a message to publish or -1 to exit");
                    message = Console.ReadLine();
                    if (message == "-1") break;
                    var messageBuilder = new MqttApplicationMessageBuilder().WithTopic("NODEServer").WithPayload(message).Build();
                    await mqttClient.PublishAsync(messageBuilder, CancellationToken.None);
                    Console.WriteLine("Message published.");
                }
            }
            else
            {
                Console.WriteLine("Not connected");
            }
        }

        public static async Task SubscribeTopic(IMqttClient mqttClient)
        {
            if (mqttClient.IsConnected)
            {
                try
                {
                    var mqttSubscriberOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                                            .WithTopicFilter(f => { f.WithTopic("NODEClient"); }).Build();

                    await mqttClient.SubscribeAsync(mqttSubscriberOptions, CancellationToken.None);

                    Console.WriteLine("Subscribed to NODEClient");
                }
                catch (Exception)
                {
                    Console.WriteLine("Cannot subscribe to NODEClient");
                    throw;
                }
            }
            else
            {
                Console.WriteLine("Not connected");
            }


        }
    }

}


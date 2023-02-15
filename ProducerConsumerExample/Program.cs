using System.Threading.Channels;

namespace ProducerConsumerExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var messagingService = new MessagingService();

            // Quickly add some messages faster than the consumer can perform the work on them
            for (var i = 0; i < 3; i++)
            {
                var message = new Message{Text = i.ToString()};
                messagingService.SendMessage(message);

                Console.WriteLine($"Send - {message}");
            }

            Console.WriteLine();
            Console.WriteLine("Enter Messages");
            var inputText = "";
            while (inputText != "z")
            {
                inputText = Console.ReadLine();
                var m = new Message() { Text = inputText };
                messagingService.SendMessage(m);
            }
        }
    }
    
    public class MessagingService
    {
        private readonly Channel<Message> _channel = Channel.CreateUnbounded<Message>();

        public MessagingService()
        {
            // This Task will run until the program is shut down
            Task.Run(Worker);
        }

        /// <summary> This just puts the Message in a queue and immediately returns without waiting on the work to occur </summary>
        public void SendMessage(Message message) => _channel.Writer.TryWrite(message);
        
        /// <summary> This is a Worker and the Consumer of SendMessage</summary>
        public async Task Worker()
        {
            // WaitToRead does not do any work unless there is something in the Channel. It won't run a process to 100% cpu usage.
            while (await _channel.Reader.WaitToReadAsync())
            {
                while (_channel.Reader.TryRead(out Message message))
                {
                    // Simulating that the Work takes quite a bit of time
                    await Task.Delay(1000);
                    Console.WriteLine($"Completed - {message}");
                }
            }
        }
    }

    public class Message
    {
        public string? Text { get; set; }
        public override string ToString() => Text;
    }
}
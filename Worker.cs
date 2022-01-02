using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using System.Device.Pwm;
using Iot.Device.ServoMotor;
using Microsoft.Extensions.Configuration;

namespace catpi_v2_worker
{
    public class Worker : BackgroundService
    {
        private const string runFeederQueueName = "run-feeder";
        private const int servoPin = 18;

        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            Console.WriteLine("catPi V2 worker message received");

            try
            {
                // Turn motor
                var motor = new ServoMotor(
                    PwmChannel.Create(0, 0, 100, 0.10),
                    360,
                    500,
                    2500
                );
                motor.Start();
                await Task.Delay(750);
                motor.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            // Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("catPi V2 processor starting...");

            _client = new ServiceBusClient(_configuration["ServiceBusConnection"]);
            _processor = _client.CreateProcessor(runFeederQueueName, new ServiceBusProcessorOptions());

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync();

            Console.WriteLine("catPi V2 processor started - awaiting messages!");

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("catPi V2 processor stopping...");

            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}

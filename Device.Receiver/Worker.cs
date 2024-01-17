using System.Text.Json;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Device.Receiver
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deviceCs = _configuration.GetConnectionString("deviceCs");
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceCs);

            var dbCs = _configuration.GetConnectionString("dbCs");

            Console.WriteLine("Device started. Waiting messages... ");

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await deviceClient.ReceiveAsync();
                if (message is not null)
                {
                    var messageData = JsonSerializer.Deserialize<Lesson>(Encoding.ASCII.GetString(message.GetBytes()));

                    using var dbConnection = new SqlConnection(dbCs);
                    string query = $"SELECT [Id],[Name],[Surname] FROM [Pazitto-Teachers] WHERE [Id] = @teacherId";
                    var teacherData = await dbConnection.QueryFirstAsync<Teacher>(query, new { messageData.TeacherId });

                    var dataToDisplay = $"""
                        Docente: {teacherData.Name} {teacherData.Surname} 
                        Lezione: {messageData.Matter}
                        Aula: {messageData.Room} (Device name)
                        Orario: {messageData.StartTime} - {messageData.EndTime}
                        """;

                    Console.WriteLine(dataToDisplay);

                    await deviceClient.CompleteAsync(message);
                    await Task.Delay(5000, stoppingToken);

                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}

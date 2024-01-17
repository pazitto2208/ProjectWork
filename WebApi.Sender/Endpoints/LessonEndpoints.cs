using System.Text.Json;
using System.Text;
using Models;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Azure.Devices;

namespace WebApi.Sender.Endpoints
{
    public static class LessonEndpoints
    {
        public static IEndpointRouteBuilder MapLessonEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/lessons");

            group.MapPost("/", SendLesson)
                .WithName(nameof(SendLesson))
                .WithOpenApi();

            group.MapGet("/", GetLessons)
                .WithName(nameof(GetLessons))
                .Produces<List<Lesson>>()
                .WithOpenApi();

            group.MapGet("/{teacherId}", GetLessonsByTeacherId)
                .WithName(nameof(GetLessonsByTeacherId))
                .Produces<List<Lesson>>()
                .WithOpenApi();

            return endpoints;
        }

        private static async Task<IResult> SendLesson(Lesson lessonData, IConfiguration configuration)
        {
            var dbCs = configuration.GetConnectionString("dbCs");
            var iotHubCs = configuration.GetConnectionString("iotHubCs");

            if (lessonData is not null)
            {
                // invio dato all'iot hub 
                var serviceClient = ServiceClient.CreateFromConnectionString(iotHubCs);
                var messageEncoded = new Message(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(lessonData)));
                await serviceClient.SendAsync(lessonData.Room, messageEncoded);

                // Inserisci i dati nel database
                using var dbConnection = new SqlConnection(dbCs);
                string query = "INSERT INTO [dbo].[Pazitto-Lessons] ([Room],[TeacherId],[StartTime],[EndTime],[Matter]) VALUES (@Room,@TeacherId,@StartTime,@EndTime,@Matter)";
                // await dbConnection.ExecuteAsync(query, lessonData);
            }

            return Results.NoContent();
        }

        private static async Task<IEnumerable<Lesson>> GetLessons(IConfiguration configuration)
        {
            var dbCs = configuration.GetConnectionString("dbCs");
            using var dbConnection = new SqlConnection(dbCs);
            string query = "SELECT [Id],[Room],[TeacherId],[Matter],[StartTime],[EndTime] FROM [Pazitto-Lessons]";
            return await dbConnection.QueryAsync<Lesson>(query);
        }

        private static async Task<IEnumerable<Lesson>> GetLessonsByTeacherId(IConfiguration configuration, int teacherId)
        {
            var dbCs = configuration.GetConnectionString("dbCs");
            using var dbConnection = new SqlConnection(dbCs);
            string query = $"SELECT [Id],[Room],[TeacherId],[Matter],[StartTime],[EndTime] FROM [Pazitto-Lessons] WHERE [TeacherId] = @teacherId";
            return await dbConnection.QueryAsync<Lesson>(query, new { teacherId });
        }
    }
}

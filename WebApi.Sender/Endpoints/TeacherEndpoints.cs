using Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace WebApi.Sender.Endpoints
{
    public static class TeacherEndpoints
    {
        public static IEndpointRouteBuilder MapTeacherEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/teachers");

            group.MapPost("/", SendTeacher)
                .WithName(nameof(SendTeacher))
                .WithOpenApi();


            group.MapGet("/", GetTeachers)
                .WithName(nameof(GetTeachers))
                .Produces<List<Teacher>>()
                .WithOpenApi();

            return endpoints;
        }

        private static async Task<IResult> SendTeacher(Teacher teacherData, IConfiguration configuration)
        {
            var dbCs = configuration.GetConnectionString("dbCs");
            using var dbConnection = new SqlConnection(dbCs);
            string query = "INSERT INTO [dbo].[Pazitto-Teachers] ([Name],[Surname]) VALUES (@Name,@Surname)";

            await dbConnection.ExecuteAsync(query, teacherData);

            return Results.NoContent();
        }

        private static async Task<IEnumerable<Teacher>> GetTeachers(IConfiguration configuration)
        {
            var dbCs = configuration.GetConnectionString("dbCs");
            using var dbConnection = new SqlConnection(dbCs);
            string query = "SELECT [Id],[Name],[Surname] FROM [Pazitto-Teachers]";
            return await dbConnection.QueryAsync<Teacher>(query);
        }

    }
}

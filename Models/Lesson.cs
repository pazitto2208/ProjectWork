namespace Models
{
    public record Lesson(int Id, string Room, int TeacherId, string Matter, string StartTime, string EndTime)
    {
    }
}

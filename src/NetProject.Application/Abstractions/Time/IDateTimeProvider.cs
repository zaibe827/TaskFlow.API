namespace NetProject.Application.Abstractions.Time;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}


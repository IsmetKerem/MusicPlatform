namespace MusicPlatform.Business.Services.Abstract;

public interface IStreamService
{
    string? ResolvePhysicalPath(string fileName);

    Task LogListeningAsync(int userId, int songId, int listenedSeconds);
}
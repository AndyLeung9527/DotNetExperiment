namespace MyLib;

public interface IMyRepository
{
    Task<object?> FindAsync(string id, CancellationToken cancellationToken);

    Task UpdateAsync(object obj, CancellationToken cancellationToken);
}

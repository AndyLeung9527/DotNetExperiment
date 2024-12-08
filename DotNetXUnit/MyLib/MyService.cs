namespace MyLib;

public class MyService
{
    private readonly IMyRepository _myRepository;

    public MyService(IMyRepository myRepository)
    {
        _myRepository = myRepository;
    }

    public async Task<bool> ChangeAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "ID cannot be empty");

        var obj = await _myRepository.FindAsync(id, cancellationToken);
        if (obj == null) return false;

        await _myRepository.UpdateAsync(obj, cancellationToken);
        return true;
    }
}

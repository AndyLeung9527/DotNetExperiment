namespace DotNetObserver;

public class CustomObserver : IObserver<CustomObj>
{
    private readonly string _instanceName;
    private IDisposable? _unsubscriber;

    public CustomObserver(string name)
    {
        _instanceName = name;
    }

    public virtual void OnCompleted()
    {
        Console.WriteLine($"{_instanceName} Completed");
        Unsubscribe();
    }

    public virtual void OnError(Exception error)
    {
        Console.WriteLine($"{_instanceName} error: {error.Message}");
    }

    public virtual void OnNext(CustomObj value)
    {
        Console.WriteLine($"{_instanceName} get value: {value.Value}");
    }

    public virtual void Subscribe(IObservable<CustomObj>? observable)
    {
        if (observable is not null)
        {
            _unsubscriber = observable.Subscribe(this);
        }
    }

    public virtual void Unsubscribe()
    {
        if (_unsubscriber is not null)
        {
            _unsubscriber.Dispose();
            _unsubscriber = null;
        }
    }
}

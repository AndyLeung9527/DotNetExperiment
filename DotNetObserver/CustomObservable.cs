namespace DotNetObserver;

public class CustomObservable : IObservable<CustomObj>
{
    private List<IObserver<CustomObj>> _observers;

    public CustomObservable()
    {
        _observers = new();
    }

    public IDisposable Subscribe(IObserver<CustomObj> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }

        return new Unsubscriber(_observers, observer);
    }

    public void Notify(CustomObj? obj)
    {
        foreach (var observer in _observers)
        {
            if (obj is null)
            {
                observer.OnError(new Exception("Null object"));
            }
            else
            {
                observer.OnNext(obj);
            }
        }
    }

    public void Complete()
    {
        var observers = _observers.ToArray();// OnCompleted()会调用Dispose(), 导致_observers的变化, 直接用foreach会报错, 所以先ToArray()复制一份进行遍历
        foreach (var observer in observers)
        {
            observer.OnCompleted();
        }

        _observers.Clear();
    }

    private class Unsubscriber : IDisposable
    {
        private List<IObserver<CustomObj>> _observers;
        private IObserver<CustomObj>? _observer;

        public Unsubscriber(List<IObserver<CustomObj>> observers, IObserver<CustomObj>? observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }
}

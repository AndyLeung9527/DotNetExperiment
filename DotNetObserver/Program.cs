namespace DotNetObserver;

internal class Program
{
    static void Main(string[] args)
    {
        CustomObservable observable = new();

        CustomObserver observer1 = new("Observer 1");
        observer1.Subscribe(observable);

        CustomObserver observer2 = new("Observer 2");
        observer2.Subscribe(observable);

        observable.Notify(new CustomObj("First step"));
        observer1.Unsubscribe();
        observable.Notify(new CustomObj("Second step"));
        observable.Notify(null);
        observable.Complete();

        Console.ReadLine();
    }
}

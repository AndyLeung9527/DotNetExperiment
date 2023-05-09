namespace DotNetClass;

public class Person
{
    private string _name = "original_name";
    public int Age { get; set; } = 18;

    public string GetInfo(int id) => $"Id:{id},Name:{_name},Age:{Age}";
}
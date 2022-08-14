// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

var o = new Outer();

Console.WriteLine(o);

var c = o with { };

Console.WriteLine(c);

o.Inner.Description = "Changed";

Console.WriteLine(o);

Console.WriteLine(c);

record struct Outer()
{
    public string Value1 { get; set; } = null;
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; set; } = "This is a description";
    public Inner Inner { get; init; } = new Inner();
}

record struct Inner()
{
    public string Value2 { get; set; } = null;
    public int Id { get; init; } = 1;
    public string Description { get; set; } = "This is a description";
}
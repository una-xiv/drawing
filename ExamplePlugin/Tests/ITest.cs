namespace ExamplePlugin.Tests;

public interface ITest
{
    public string Name { get; }

    public void OnActivate();

    public void Render();
}
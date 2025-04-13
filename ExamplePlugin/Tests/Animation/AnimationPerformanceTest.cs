namespace ExamplePlugin.Tests.Animation;

internal class AnimationPerformanceTest : SimpleUdtTest
{
    public override    string Name        => "Performance Test";
    public override    string Category    => "Animation";
    protected override string UdtFileName => "tests.animation.animation_performance_test.xml";

    protected override void OnDocumentLoaded()
    {
        foreach (var node in Document!.RootNode!.QuerySelectorAll(".box")) {
            node.OnClick += _ => { };
        }
    }
}
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;

namespace MyLib.Tests;

public class AutoFakeItEasyAttribute : AutoDataAttribute
{
    public AutoFakeItEasyAttribute()
        : base(CreateFixture)
    {

    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoFakeItEasyCustomization());
        fixture.Customizations.Add(new CancellationTokenGenerator());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}


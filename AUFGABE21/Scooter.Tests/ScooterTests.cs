using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class ScooterTests
{
    [Fact]
    public void Ctor_NewScooter_HasFullBatteryAndLocked()
    {
        var s = new Scooter();

        s.BatteryLevel.ShouldBe(100);
        s.IsLocked.ShouldBeTrue();
        s.ScooterId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Ctor_DistinctScooters_HaveDistinctIds()
    {
        var a = new Scooter();
        var b = new Scooter();
        var c = new Scooter();

        new[] { a.ScooterId, b.ScooterId, c.ScooterId }.Distinct().Count().ShouldBe(3);
    }

    [Fact]
    public void Scooter_StateIsReadableWithoutMutation()
    {
        var s = new Scooter();

        for (var i = 0; i < 10; i++)
        {
            _ = s.ScooterId;
            _ = s.BatteryLevel;
            _ = s.IsLocked;
        }

        s.BatteryLevel.ShouldBe(100);
        s.IsLocked.ShouldBeTrue();
    }
}

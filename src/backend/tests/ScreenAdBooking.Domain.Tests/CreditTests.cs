namespace ScreenAdBooking.Domain.Tests;

public sealed class CreditTests
{
    private static readonly Guid CompanyId = Guid.CreateVersion7();

    [Fact]
    public void Creates_a_movement()
    {
        var movement = new CreditMovement(CompanyId, CreditMovementType.TopUp, 500, " Starter pack ", TestDates.Now);

        Assert.NotEqual(Guid.Empty, movement.Id);
        Assert.Equal(CompanyId, movement.CompanyId);
        Assert.Equal(CreditMovementType.TopUp, movement.Type);
        Assert.Equal(500, movement.Credits);
        Assert.Equal("Starter pack", movement.Label);
        Assert.Equal(TestDates.Now, movement.CreatedAtUtc);
    }

    [Fact]
    public void Rejects_invalid_movements()
    {
        Assert.Equal("companyId", Assert.Throws<ArgumentException>(
            () => new CreditMovement(Guid.Empty, CreditMovementType.TopUp, 1, "Pack", TestDates.Now)).ParamName);
        Assert.Equal("type", Assert.Throws<ArgumentOutOfRangeException>(
            () => new CreditMovement(CompanyId, (CreditMovementType)9, 1, "Pack", TestDates.Now)).ParamName);
        Assert.Equal("credits", Assert.Throws<ArgumentOutOfRangeException>(
            () => new CreditMovement(CompanyId, CreditMovementType.TopUp, 0, "Pack", TestDates.Now)).ParamName);
        Assert.Equal("label", Assert.Throws<ArgumentException>(
            () => new CreditMovement(CompanyId, CreditMovementType.TopUp, 1, " ", TestDates.Now)).ParamName);
        Assert.Equal("createdAtUtc", Assert.Throws<ArgumentException>(
            () => new CreditMovement(CompanyId, CreditMovementType.TopUp, 1, "Pack", DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);
    }

    [Fact]
    public void Balance_of_no_movement_is_zero()
    {
        Assert.Equal(new CreditBalance(0, 0), CreditBalance.From([]));
    }

    [Fact]
    public void Balance_follows_top_up_hold_charge_and_release()
    {
        var balance = CreditBalance.From([
            Movement(CreditMovementType.TopUp, 2000),
            Movement(CreditMovementType.Hold, 300),
            Movement(CreditMovementType.Hold, 200),
            Movement(CreditMovementType.Charge, 300),
            Movement(CreditMovementType.Release, 200),
            Movement(CreditMovementType.Hold, 100),
        ]);

        Assert.Equal(new CreditBalance(1600, 100), balance);
    }

    [Theory]
    [InlineData("starter", 500)]
    [InlineData(" Standard ", 2000)]
    [InlineData("LARGE", 5000)]
    public void Finds_a_pack_ignoring_case_and_spaces(string code, int credits)
    {
        Assert.Equal(credits, CreditPacks.Find(code)?.Credits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("huge")]
    public void Unknown_pack_is_null(string? code)
    {
        Assert.Null(CreditPacks.Find(code));
    }

    private static CreditMovement Movement(CreditMovementType type, int credits) =>
        new(CompanyId, type, credits, type.ToString(), TestDates.Now);
}

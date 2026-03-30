namespace ReactorRouter.Tests.Navigation;

public class GuardResultTests
{
    [Fact]
    public void Allow_IsCorrectSubtype()
    {
        GuardResult result = new GuardResult.Allow();
        result.Should().BeOfType<GuardResult.Allow>();
    }

    [Fact]
    public void Block_IsCorrectSubtype()
    {
        GuardResult result = new GuardResult.Block();
        result.Should().BeOfType<GuardResult.Block>();
    }

    [Fact]
    public void Redirect_StoresPath()
    {
        var result = new GuardResult.Redirect("/login");
        result.Path.Should().Be("/login");
    }

    [Fact]
    public void PatternMatching_Allow()
    {
        GuardResult result = new GuardResult.Allow();
        var isAllow = result is GuardResult.Allow;
        isAllow.Should().BeTrue();
    }

    [Fact]
    public void PatternMatching_Block()
    {
        GuardResult result = new GuardResult.Block();
        var isBlock = result is GuardResult.Block;
        isBlock.Should().BeTrue();
    }

    [Fact]
    public void PatternMatching_Redirect_ExtractsPath()
    {
        GuardResult result = new GuardResult.Redirect("/upgrade");

        if (result is GuardResult.Redirect { Path: var path })
            path.Should().Be("/upgrade");
        else
            Assert.Fail("Expected Redirect");
    }

    [Fact]
    public void Allow_And_Allow_AreEqual()
    {
        // records have structural equality
        var a = new GuardResult.Allow();
        var b = new GuardResult.Allow();
        a.Should().Be(b);
    }

    [Fact]
    public void Redirect_SamePath_AreEqual()
    {
        var a = new GuardResult.Redirect("/login");
        var b = new GuardResult.Redirect("/login");
        a.Should().Be(b);
    }

    [Fact]
    public void Redirect_DifferentPaths_AreNotEqual()
    {
        var a = new GuardResult.Redirect("/login");
        var b = new GuardResult.Redirect("/signup");
        a.Should().NotBe(b);
    }
}

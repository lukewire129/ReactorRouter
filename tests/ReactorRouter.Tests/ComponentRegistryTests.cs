using ReactorRouter;
using MauiReactor;

namespace ReactorRouter.Tests;

// ComponentRegistry is static global state — run these tests serially to avoid race conditions
[Collection("ComponentRegistry")]
public class ComponentRegistryTests : IDisposable
{
    // Reset registry state between tests
    public void Dispose() => ComponentRegistry.Clear();

    [Fact]
    public void GetFactory_ReturnsNull_ForUnregisteredType()
    {
        var factory = ComponentRegistry.GetFactory(typeof(string));

        factory.Should().BeNull();
    }

    [Fact]
    public void Register_NonGeneric_StoresFactory()
    {
        var called = false;
        ComponentRegistry.Register(typeof(string), () => { called = true; return null!; });

        var factory = ComponentRegistry.GetFactory(typeof(string));

        factory.Should().NotBeNull();
        factory!.Invoke();
        called.Should().BeTrue();
    }

    [Fact]
    public void Register_NonGeneric_OverwritesExistingFactory()
    {
        ComponentRegistry.Register(typeof(string), () => null!);

        var secondCalled = false;
        ComponentRegistry.Register(typeof(string), () => { secondCalled = true; return null!; });

        var factory = ComponentRegistry.GetFactory(typeof(string));
        factory!.Invoke();

        secondCalled.Should().BeTrue();
    }

    [Fact]
    public void Clear_RemovesAllRegistrations()
    {
        ComponentRegistry.Register(typeof(string), () => null!);
        ComponentRegistry.Register(typeof(int), () => null!);

        ComponentRegistry.Clear();

        ComponentRegistry.GetFactory(typeof(string)).Should().BeNull();
        ComponentRegistry.GetFactory(typeof(int)).Should().BeNull();
    }

    [Fact]
    public void Register_MultipleTypes_AllRetrievable()
    {
        ComponentRegistry.Register(typeof(string), () => null!);
        ComponentRegistry.Register(typeof(int), () => null!);
        ComponentRegistry.Register(typeof(double), () => null!);

        ComponentRegistry.GetFactory(typeof(string)).Should().NotBeNull();
        ComponentRegistry.GetFactory(typeof(int)).Should().NotBeNull();
        ComponentRegistry.GetFactory(typeof(double)).Should().NotBeNull();
    }
}

[Collection("ComponentRegistry")]
public class ReactorRouterConfigurationTests : IDisposable
{
    public void Dispose() => ComponentRegistry.Clear();

    [Fact]
    public void Routes_AutoRegistersAllComponentTypes()
    {
        var config = new ReactorRouterConfiguration();

        config.Routes(
            new RouteDefinition("/", typeof(FakeRootLayout),
                new RouteDefinition("child", typeof(FakeChildPage))
            )
        );

        ComponentRegistry.GetFactory(typeof(FakeRootLayout)).Should().NotBeNull();
        ComponentRegistry.GetFactory(typeof(FakeChildPage)).Should().NotBeNull();
    }

    [Fact]
    public void Routes_DoesNotOverwriteExistingRegistration()
    {
        Func<VisualNode> customFactory = () => null!;
        ComponentRegistry.Register(typeof(FakeRootLayout), customFactory);

        var config = new ReactorRouterConfiguration();
        config.Routes(new RouteDefinition("/", typeof(FakeRootLayout)));

        // The stored factory should still be the original custom one (same reference)
        ComponentRegistry.GetFactory(typeof(FakeRootLayout)).Should().BeSameAs(customFactory);
    }

    [Fact]
    public void Routes_StoresRouteDefinitions()
    {
        var routes = new RouteDefinition[] { new("/", typeof(FakeRootLayout)) };
        var config = new ReactorRouterConfiguration();

        config.Routes(routes);

        // RouteDefinitions is internal — accessible via InternalsVisibleTo
        config.RouteDefinitions.Should().HaveCount(1);
        config.RouteDefinitions[0].ComponentType.Should().Be(typeof(FakeRootLayout));
    }

    [Fact]
    public void InitialPath_DefaultsToSlash()
    {
        var config = new ReactorRouterConfiguration();

        // InitialRoute is internal — accessible via InternalsVisibleTo
        config.InitialRoute.Should().Be("/");
    }

    [Fact]
    public void InitialPath_SetsCustomPath()
    {
        var config = new ReactorRouterConfiguration();

        config.InitialPath("/dashboard");

        config.InitialRoute.Should().Be("/dashboard");
    }
}

// Minimal stub types for testing (no VisualNode needed for registry tests)
file class FakeRootLayout;
file class FakeChildPage;

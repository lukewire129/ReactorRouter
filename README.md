# ReactorRouter
[![NuGet](https://img.shields.io/nuget/v/ReactorRouter.svg)](https://www.nuget.org/packages/ReactorRouter/) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/ReactorRouter.svg)](https://www.nuget.org/packages/ReactorRouter/)

ReactorRouter is a lightweight routing library for .NET MAUI Reactor applications. It provides a declarative way to handle navigation, nested routes, and route transitions.

## Features

- **Declarative Routing**: Define your route hierarchy in a single place.
- **Nested Routes**: Supports deeply nested layouts with `Outlet`.
- **Transitions**: Built-in support for page transitions (Fade, Slide, etc.).
- **Navigation**: Simple API for programmatic navigation.

## Installation

Add the `ReactorRouter` project or NuGet package to your MAUI solution.

## Getting Started

### 1. Define Routes & Initialize

ReactorRouter supports two ways to configure your routes: using the **App Builder** (cleaner, recommended for larger apps) or the **Component API** (quick start).

#### Option A: App Builder (Recommended)

Configure routes in `MauiProgram.cs`. This keeps your routing logic centralized and separate from your UI code.

**MauiProgram.cs**
```csharp
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiReactorApp<App>()
    .UseReactorRouter(r =>
    {
        r.Routes(
            new RouteDefinition("/", typeof(RootLayout),
                new RouteDefinition("dashboard", typeof(DashboardLayout),
                    RouteDefinition.Index(typeof(HomePage)),
                    new RouteDefinition("settings", typeof(SettingsPage))
                        { Transition = TransitionType.SlideLeft }
                ),
                new RouteDefinition("login", typeof(LoginPage))
                    { Transition = TransitionType.Fade },
                new RouteDefinition("*", typeof(NotFoundPage)) // Fallback
            )
        );
        r.InitialPath("/dashboard");
    })
    .Build();
```

**App.cs**
Then, simply instantiate `Router` without arguments in your main component.

```csharp
class App : Component
{
    public override VisualNode Render()
        => ContentPage(new Router());
}
```

#### Option B: Component API (Quick Start)

Define routes directly in your main component using the fluent API.

```csharp
// Define routes
private static readonly RouteDefinition[] Routes =
[
    new RouteDefinition("/", typeof(RootLayout),
        new RouteDefinition("dashboard", typeof(DashboardLayout),
            RouteDefinition.Index(typeof(HomePage)),
            new RouteDefinition("settings", typeof(SettingsPage))
        ),
        new RouteDefinition("login", typeof(LoginPage)),
        new RouteDefinition("*", typeof(NotFoundPage))
    )
];

// Initialize in Render
class MainPage : Component
{
    public override VisualNode Render()
        => ContentPage(
            new Router()
                .Routes(Routes)
                .InitialPath("/dashboard")
        );
}
```

### 2. Use Outlet for Nested Content

In your layout components (like `RootLayout` or `DashboardLayout`), use `Outlet` to determine where child routes should be rendered.

```csharp
class DashboardLayout : Component
{
    public override VisualNode Render()
        => Grid("50, *", "200, *",
            // Sidebar
            new Sidebar().GridRowSpan(2),

            // Top Bar
            new TopBar().GridColumn(1),

            // Content Area - Child routes render here
            new Outlet()
                .Transition(TransitionType.Fade)
                .Duration(300)
                .GridRow(1)
                .GridColumn(1)
        );
}
```

### 4. Navigation

You can navigate using the `Link` component or the `NavigationService`.

**Using Link:**

```csharp
new Link().To("/dashboard/settings")
// Or with custom content
new Link().To("/login")
    .Child(Label("Go to Login"))
```

**Programmatic Navigation:**

```csharp
NavigationService.Instance.NavigateTo("/profile/123");
```

## Route Parameters

You can define parameters in your route path using `:paramName` and access query strings with `?key=value`.

ReactorRouter supports **two approaches** for reading route/query parameters depending on how your component is structured.

---

### Approach 1: `[Param] IParameter<RouterContext>` (Field Injection)

Use this when your component does **not** need a custom constructor. The `RouterContext` is injected automatically as a field parameter.

```csharp
// Route definition
new RouteDefinition("profile/:userId", typeof(ProfilePage))

// Component
public partial class ProfilePage : Component
{
    [Param] IParameter<RouterContext> _ctx;

    public override VisualNode Render()
    {
        var userId = _ctx.Value.Params.GetOrDefault("userId", "unknown");
        var tab = _ctx.Value.Query.GetOrDefault("tab", "overview");
        // ...
    }
}
```

---

### Approach 2: `RouteHooks` (Static API, Constructor-Friendly)

Use this when your component **requires a custom constructor**. Since `[Param]` is field-level injection and cannot be used inside a constructor, `RouteHooks` provides a static API that can be called anywhere ? including constructors.

```csharp
// Route definition
new RouteDefinition("profile/:userId", typeof(ProfilePage))

// Component
public class ProfilePage : Component
{
    private readonly string _userId;
    private readonly string _tab;

    public ProfilePage()
    {
        _userId = RouteHooks.UseRouteParams().GetOrDefault("userId", "unknown");
        _tab = RouteHooks.UseRouteQuery().GetOrDefault("tab", "overview");
    }

    public override VisualNode Render()
    {
        // use _userId, _tab ...
    }
}
```

---

> **Which one should I use?**
> - No custom constructor needed �� use `[Param] IParameter<RouterContext>` (less boilerplate)
> - Custom constructor required �� use `RouteHooks` (more flexible)

## Conclusion

ReactorRouter simplifies routing in .NET MAUI applications, allowing developers to create intuitive navigation experiences with minimal effort. For more information, check the [documentation](https://github.com/user-attachments/assets/32ee9ae8-65f8-4e9d-b998-6ed2879aea19).



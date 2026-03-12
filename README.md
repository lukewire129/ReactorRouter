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

### 1. Define Your Routes

Create a route definition array. You can nest routes to create layouts.

```csharp
private static readonly RouteDefinition[] Routes =
[
    new RouteDefinition("/", typeof(RootLayout),
        // Child routes render inside RootLayout's Outlet
        new RouteDefinition("dashboard", typeof(DashboardLayout),
            RouteDefinition.Index(typeof(HomePage)),
            new RouteDefinition("settings", typeof(SettingsPage))
                { Transition = TransitionType.SlideLeft },
            new RouteDefinition("profile/:userId", typeof(ProfilePage))
                { Transition = TransitionType.Fade }
        ),
        new RouteDefinition("login", typeof(LoginPage))
            { Transition = TransitionType.Fade },
        new RouteDefinition("*", typeof(NotFoundPage)) // Fallback route
    )
];
```

### 2. Initialize the Router

In your main entry component (e.g., `MainPage`), initialize the `Router` with your routes.

```csharp
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

### 3. Use Outlet for Nested Content

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

You can define parameters in your route path using `:paramName`. These are passed to the component constructor if it accepts them.

```csharp
// Route definition
new RouteDefinition("profile/:userId", typeof(ProfilePage))

// Component
class ProfilePage : Component
{
    private string _userId;
    public ProfilePage(string userId) => _userId = userId;
    // ...
}
```


https://github.com/user-attachments/assets/32ee9ae8-65f8-4e9d-b998-6ed2879aea19



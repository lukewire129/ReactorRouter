# ReactorRouter

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

## License

MIT

`ReactorRouter`�� .NET MAUI�� �淮 ������ ����� ���̺귯���Դϴ�. ������Ʈ ������� ��θ� �����ϰ�, ��ø �����(`Outlet`), ��ũ(`Link`) �� ���� �׺���̼� ���񽺸� �����մϴ�. �� README�� ���̺귯�� ����, �ֿ� API, ���� ����, ����/��Ű¡, �׽�Ʈ, �⿩ ������� ���������� �����մϴ�.

����
- �Ұ�
- ���� ����(Quickstart)
- �ֿ� API ���۷���
- ���Ʈ ���ø�(�Ķ���� �� ���ϵ�ī��)
- ���� �ڵ�
- ��ȯ/�ִϸ��̼�
- ���� �� ��Ű¡(NuGet)
- �׽�Ʈ
- �⿩ ���̵�
- ���̼���

## �Ұ�

`ReactorRouter`�� MAUI �ۿ��� ������ UI ���Ͽ� �ڿ������� ��Ƶ�� ����� API�� �����մϴ�. ������ �����մϴ�:

- ��� ��� ������Ʈ ������
- ��ø �����(Outlet)
- UI ��ũ(Link) �� �ڵ� ��� �׺���̼�
- ���� ��� ��ȯ ����(TransitionType)

������Ʈ ����(�ֿ� ��ġ):

- ���̺귯��: `src/ReactorRouter`
- ����: `samples/ReactorRouterSample`
- �׽�Ʈ: `tests/ReactorRouter.Tests`

## ���� ���� (Quickstart)

1. �ַ�ǿ� `src/ReactorRouter/ReactorRouter.csproj`�� �����ϰų� NuGet ��Ű���� �����մϴ�.
2. `MauiProgram`���� �׺���̼� ���� ��� �� �ʱ�ȭ:

```csharp
// samples/ReactorRouterSample/MauiProgram.cs (��)
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        // ... ���� MAUI ����

        // ReactorRouter ���� ���� ���
        builder.Services.AddSingleton<NavigationService>();
        // �ʿ� �� ���Ʈ/���� �δ� ���

        return builder.Build();
    }
}
```

3. �� �������� `Router` ��ġ:

```csharp
var routes = new[] {
    new Route("/", () => new HomePage()),
    new Route("/login", () => new LoginPage()),
    new Route("/dashboard/*", () => new DashboardLayout()),
    new Route("/profile/:id", pathParams => new ProfilePage(pathParams["id"])),
    new Route("*", () => new NotFoundPage()),
};

new Router().Routes(routes);
```

## �ֿ� ������Ʈ �� API

�Ʒ��� ���̺귯���� �ֿ� Ÿ�԰� å���Դϴ�. �ڼ��� �ñ״�ó�� �ҽ� �ڵ�(`src/ReactorRouter/Components` �� `src/ReactorRouter/Navigation`)�� �����ϼ���.

- `Router` (`src/ReactorRouter/Components/Router.cs`)
  - ���Ʈ ���̺��� �����ϰ� ���� ��ο� �´� ������Ʈ�� �������մϴ�.
  - �޼���: `Routes(Route[] routes)`, ���Ʈ ��Ī ���� API

- `Route`
  - ��� ���ø��� ������Ʈ ���丮�� �����մϴ�. �Ķ���� ���ε� ���� ����.
  - ������Ƽ: `Path`, `Factory` �Ǵ� `FactoryWithParams`

- `Outlet` (`src/ReactorRouter/Components/Outlet.cs`)
  - �θ� ���Ʈ ���ο��� ���� ���Ʈ�� �������ϱ� ���� �ڸ�ǥ�����Դϴ�.

- `Link` (`src/ReactorRouter/Components/Link.cs`)
  - ��/Ŭ�� �� ������ ��η� �׺���̼��� Ʈ�����մϴ�.
  - ��� ��: `new Link().To("/dashboard/settings")`

- `NavigationService` (`src/ReactorRouter/Navigation/NavigationService.cs`)
  - �������� �׺���̼��� Ʈ�����մϴ�.
  - �޼���: `NavigateTo(string path, bool replace = false)`, `GetCurrentPath()` ��

- ���� �� ��ȯ ����
  - `RouterState`, `OutletState`, `TransitionType` ���� ��� ���¿� ��ȯ ������ �����մϴ�.

## ���Ʈ ���ø�

���� ������ ���ø� ����(���� Ȯ�� �ʿ�):

- ���� ���: `/home`, `/login`
- �Ķ����: `/users/:id` -> `:id` ����
- ���ϵ�ī��: `/dashboard/*` -> ���� ��� ��ü ��Ī

���Ʈ ��Ī ������ Ŀ���͸������Ϸ��� `Router` ���� ��Ī �Լ��� Ȯ���ϰų� ���Ʈ ��� �� ���ø� �ļ��� �߰��ϼ���.

## ����

1) ���Ʈ ���� �� Router ��ġ

```csharp
var routes = new[] {
    new Route("/", () => new HomePage()),
    new Route("/login", () => new LoginPage()),
    new Route("/dashboard", () => new DashboardLayout()),
    new Route("/profile/:id", params => new ProfilePage(params["id"])),
    new Route("*", () => new NotFoundPage()),
};

new Router().Routes(routes);
```

2) DashboardLayout���� Outlet ��� (��ø �����)

```csharp
public class DashboardLayout : Component
{
    public override VisualNode Render()
    {
        return ContentView(
            Column(
                new Sidebar(),
                new Outlet() // ���� ���Ʈ ������
            )
        );
    }
}
```

3) Link�� ���α׷��� �׺���̼�

```csharp
// UI ��ũ
new Link().To("/dashboard/settings");

// �ڵ忡�� �̵�
NavigationService.Instance.NavigateTo("/profile/123");
```

## ��ȯ(Transition) �� �ִϸ��̼�

`TransitionType`�� ����� �⺻ ��ȯ(��: None, Fade, Slide)�� ������ �� �ֽ��ϴ�. ��ü�� �ִϸ��̼� ������ MAUI�� �ִϸ��̼� API�� Ȱ���Ͽ� �÷������� Ȯ���ϼ���.

## ���� �� ��Ű¡

- ���̺귯�� ����:

```bash
dotnet build src/ReactorRouter/ReactorRouter.csproj -f net9.0
```

- NuGet ��Ű���� ����:

```bash
dotnet pack src/ReactorRouter/ReactorRouter.csproj -c Release
# ��Ű�� ���ε�
dotnet nuget push bin/Release/ReactorRouter.*.nupkg --source <your-nuget-feed>
```

> ����: ��ũ�ε� �� Ÿ�� �����ӿ�ũ�� ������Ʈ ����(`ReactorRouter.csproj`)�� Ȯ���ϼ���. ���� ���� `.NET 10 (net10.0)`�� Ÿ������ �����Ǿ� �ֽ��ϴ�.

## �׽�Ʈ

- ���� �׽�Ʈ:

```bash
dotnet test tests/ReactorRouter.Tests/ReactorRouter.Tests.csproj
```

- ����/���� �׽�Ʈ: `samples/ReactorRouterSample`�� �÷������� ������ UI ������ Ȯ���ϼ���.

## �⿩ ���̵�

- �̽��� ���� ����� �ּ���(����/����).
- ������ ���� ������ PR�� �����ϼ���.
- �ڵ� ��Ÿ���� ���� �ڵ庣�̽��� ��������(.NET/C# ������).
- �ֿ� ��������� README/���� �� �׽�Ʈ�� �Բ� ������Ʈ�ϼ���.

## ��ũ���� / ���̾�׷�

���� ȭ�� ĸó�� ��Ű��ó ���̾�׷��� �߰��Ϸ��� `docs/images/`�� ������ �ø��� �� ������ ��ο� ������ �߰��ϼ���.

## ���̼���

��Ʈ�� `LICENSE` ������ Ȯ���ϼ���.

---

�� ���� API ���۷���(�� Ÿ���� �ñ״�ó, �Ķ���� ����)�� ���� ȭ�� ĸó�� �߰��ϱ� ���ϸ� �˷��ּ���.
namespace ReactorRouter.Internals;

internal static class TypeResolver
{
    internal static Type ResolveLatest(Type type)
    {
        var fullName = type.FullName;
        if (fullName == null) return type;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = assemblies.Length - 1; i >= 0; i--)
        {
            if (assemblies[i].IsDynamic) continue;

            var resolved = assemblies[i].GetType(fullName);
            if (resolved != null && resolved != type)
                return resolved;
        }
        return type;
    }
}

namespace ReactorRouter
{
    public static class RouteHooks
    {
        public static RouteParams UseRouteParams() =>
            NavigationService.Instance.CurrentParams;

        public static RouteQuery UseRouteQuery() =>
            NavigationService.Instance.CurrentQuery;
    }
}

namespace TryNextPost.Domain.Enums
{
    public static class EmployeePermissionCode
    {
        public const string WalletRecharge = "Wallet.Recharge";
        public const string WalletViewBalance = "Wallet.ViewBalance";
        public const string OrdersView = "Orders.View";
        public const string OrdersCreate = "Orders.Create";
        public const string ShipmentsView = "Shipments.View";
        public const string ShipmentsCreate = "Shipments.Create";

        public static readonly IReadOnlyList<string> All = new[]
        {
            WalletRecharge,
            WalletViewBalance,
            OrdersView,
            OrdersCreate,
            ShipmentsView,
            ShipmentsCreate
        };

        public static bool IsValid(string code) =>
            All.Contains(code, StringComparer.OrdinalIgnoreCase);
    }
}

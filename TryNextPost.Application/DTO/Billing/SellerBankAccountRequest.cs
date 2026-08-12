namespace TryNextPost.Application.DTO.Billing
{
    public class SellerBankAccountRequest
    {
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? AccountType { get; set; }
        public bool IsPrimary { get; set; }
    }
}

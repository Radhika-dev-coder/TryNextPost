namespace TryNextPost.Application.DTO.Billing
{
    public class SellerBankAccountResponse
    {
        public long SellerBankAccountId { get; set; }
        public long SellerId { get; set; }
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? AccountType { get; set; }
        public bool IsPrimary { get; set; }
    }
}

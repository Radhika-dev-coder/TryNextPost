using TryNextPost.Application.DTO.Billing;

namespace TryNextPost.Application.IServices.Interface.IBilling
{
    public interface IBillingService
    {
        Task<ShipmentChargesListResponse> GetShipmentChargesAsync(string userId, ShipmentChargesFilterRequest filter);

        Task<CodRemittanceSummaryResponse> GetCodSummaryAsync(string userId);
        Task<CodRemittanceListResponse> GetCodRemittancesAsync(string userId, CodRemittanceFilterRequest filter);

        Task<List<SellerBankAccountResponse>> GetBankAccountsAsync(string userId);
        Task<SellerBankAccountResponse> CreateBankAccountAsync(string userId, SellerBankAccountRequest request);
        Task<SellerBankAccountResponse> UpdateBankAccountAsync(string userId, long id, SellerBankAccountRequest request);
        Task DeleteBankAccountAsync(string userId, long id);

        Task<InvoiceListResponse> GetInvoicesAsync(string userId, InvoiceFilterRequest filter);
        Task<(byte[] Content, string FileName)> DownloadInvoiceCsvAsync(string userId, long invoiceId);
    }
}

using System.Globalization;
using System.Text;
using TryNextPost.Application.DTO.Billing;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IBilling;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Billing
{
    public class BillingService : IBillingService
    {
        private readonly ISellerContextService _sellerContextService;
        private readonly IShipmentChargesRepository _shipmentChargesRepository;
        private readonly ICODSettlementRepository _codSettlementRepository;
        private readonly ISellerBankAccountRepository _bankAccountRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IWalletRechargeRepository _walletRechargeRepository;

        public BillingService(
            ISellerContextService sellerContextService,
            IShipmentChargesRepository shipmentChargesRepository,
            ICODSettlementRepository codSettlementRepository,
            ISellerBankAccountRepository bankAccountRepository,
            IInvoiceRepository invoiceRepository,
            IWalletRechargeRepository walletRechargeRepository)
        {
            _sellerContextService = sellerContextService;
            _shipmentChargesRepository = shipmentChargesRepository;
            _codSettlementRepository = codSettlementRepository;
            _bankAccountRepository = bankAccountRepository;
            _invoiceRepository = invoiceRepository;
            _walletRechargeRepository = walletRechargeRepository;
        }

        public async Task<ShipmentChargesListResponse> GetShipmentChargesAsync(
            string userId,
            ShipmentChargesFilterRequest filter)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);
            var awbs = ParseAwbs(filter.AwbNumbers);

            var (items, total) = await _shipmentChargesRepository.GetFilteredForSellerAsync(
                seller.SellerId,
                filter.FromDate,
                filter.ToDate,
                awbs,
                page,
                pageSize);

            return new ShipmentChargesListResponse
            {
                Items = items.Select(MapCharges).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CodRemittanceSummaryResponse> GetCodSummaryAsync(string userId)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            await SyncPendingCodSettlementsAsync(seller.SellerId, userId);

            var remitted = await _codSettlementRepository.SumByStatusAsync(seller.SellerId, SettlementStatus.Settled);
            var due = await _codSettlementRepository.SumByStatusAsync(seller.SellerId, SettlementStatus.Pending);
            var last = await _codSettlementRepository.GetLastSettledAmountAsync(seller.SellerId);

            return new CodRemittanceSummaryResponse
            {
                RemittedTillDate = remitted,
                LastRemittance = last,
                NextRemittanceExpected = due,
                TotalRemittanceDue = due
            };
        }

        public async Task<CodRemittanceListResponse> GetCodRemittancesAsync(
            string userId,
            CodRemittanceFilterRequest filter)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            await SyncPendingCodSettlementsAsync(seller.SellerId, userId);

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);
            var status = ParseSettlementStatus(filter.Status);

            var (items, total) = await _codSettlementRepository.GetFilteredAsync(
                seller.SellerId,
                status,
                filter.FromDate,
                filter.ToDate,
                page,
                pageSize);

            return new CodRemittanceListResponse
            {
                Items = items.Select(MapCod).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<SellerBankAccountResponse>> GetBankAccountsAsync(string userId)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            var accounts = await _bankAccountRepository.GetBySellerIdAsync(seller.SellerId);
            return accounts.Select(MapBank).ToList();
        }

        public async Task<SellerBankAccountResponse> CreateBankAccountAsync(
            string userId,
            SellerBankAccountRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            ValidateBankRequest(request);

            if (request.IsPrimary)
                await ClearPrimaryAsync(seller.SellerId);

            var entity = new SellerBankAccount
            {
                SellerId = seller.SellerId,
                AccountHolderName = request.AccountHolderName.Trim(),
                AccountNumber = request.AccountNumber.Trim(),
                IfscCode = request.IfscCode.Trim().ToUpperInvariant(),
                BankName = request.BankName?.Trim(),
                BranchName = request.BranchName?.Trim(),
                AccountType = request.AccountType?.Trim(),
                IsPrimary = request.IsPrimary,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _bankAccountRepository.AddAsync(entity);
            await _bankAccountRepository.SaveChangesAsync();
            return MapBank(entity);
        }

        public async Task<SellerBankAccountResponse> UpdateBankAccountAsync(
            string userId,
            long id,
            SellerBankAccountRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            ValidateBankRequest(request);

            var entity = await _bankAccountRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(SystemMessage.CodBankDetailsNotFound);

            if (entity.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            if (request.IsPrimary)
                await ClearPrimaryAsync(seller.SellerId);

            entity.AccountHolderName = request.AccountHolderName.Trim();
            entity.AccountNumber = request.AccountNumber.Trim();
            entity.IfscCode = request.IfscCode.Trim().ToUpperInvariant();
            entity.BankName = request.BankName?.Trim();
            entity.BranchName = request.BranchName?.Trim();
            entity.AccountType = request.AccountType?.Trim();
            entity.IsPrimary = request.IsPrimary;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            await _bankAccountRepository.UpdateAsync(entity);
            await _bankAccountRepository.SaveChangesAsync();
            return MapBank(entity);
        }

        public async Task DeleteBankAccountAsync(string userId, long id)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var entity = await _bankAccountRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(SystemMessage.CodBankDetailsNotFound);

            if (entity.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            await _bankAccountRepository.UpdateAsync(entity);
            await _bankAccountRepository.SaveChangesAsync();
        }

        public async Task<InvoiceListResponse> GetInvoicesAsync(string userId, InvoiceFilterRequest filter)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            await EnsureMonthlyInvoicesAsync(seller.SellerId, userId);

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);

            var (items, total) = await _invoiceRepository.GetFilteredAsync(
                seller.SellerId,
                filter.FromDate,
                filter.ToDate,
                page,
                pageSize);

            return new InvoiceListResponse
            {
                Items = items.Select(MapInvoice).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<(byte[] Content, string FileName)> DownloadInvoiceCsvAsync(string userId, long invoiceId)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId)
                ?? throw new KeyNotFoundException(SystemMessage.InvoiceNotFound);

            if (invoice.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            var sb = new StringBuilder();
            sb.AppendLine("InvoiceNumber,ServiceType,InvoiceDate,PeriodFrom,PeriodTo,ShippingCharges,Recharges,TotalAmount");
            sb.Append(Csv(invoice.InvoiceNumber)).Append(',');
            sb.Append(Csv(invoice.ServiceType)).Append(',');
            sb.Append(Csv(invoice.InvoiceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))).Append(',');
            sb.Append(Csv(invoice.PeriodFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))).Append(',');
            sb.Append(Csv(invoice.PeriodTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))).Append(',');
            sb.Append(Csv(invoice.ShippingChargesAmount.ToString(CultureInfo.InvariantCulture))).Append(',');
            sb.Append(Csv(invoice.RechargeAmount.ToString(CultureInfo.InvariantCulture))).Append(',');
            sb.AppendLine(Csv(invoice.Amount.ToString(CultureInfo.InvariantCulture)));

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"{invoice.InvoiceNumber}.csv";
            return (bytes, fileName);
        }

        private async Task SyncPendingCodSettlementsAsync(long sellerId, string userId)
        {
            var unsettled = await _codSettlementRepository.GetUnsettledDeliveredCodShipmentsAsync(sellerId);
            if (unsettled.Count == 0)
                return;

            var rows = unsettled.Select(u => new CODSettlement
            {
                ShipmentId = u.ShipmentId,
                SellerId = sellerId,
                CodAmount = u.CodAmount,
                CollectedAmount = u.CodAmount,
                Status = SettlementStatus.Pending,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            }).ToList();

            await _codSettlementRepository.AddRangeAsync(rows);
            await _codSettlementRepository.SaveChangesAsync();
        }

        private async Task EnsureMonthlyInvoicesAsync(long sellerId, string userId)
        {
            var now = DateTime.UtcNow;
            // Generate for last 6 completed months + current month when there is activity.
            for (var i = 0; i < 6; i++)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var existing = await _invoiceRepository.GetBySellerAndPeriodAsync(sellerId, monthStart, monthEnd);
                if (existing.Count > 0)
                    continue;

                var shipping = await _shipmentChargesRepository.SumSellerChargeForPeriodAsync(
                    sellerId, monthStart, monthEnd);
                var recharges = await _walletRechargeRepository.SumPaidForSellerPeriodAsync(
                    sellerId, monthStart, monthEnd);

                var total = shipping + recharges;
                if (total <= 0)
                    continue;

                var serviceType = shipping > 0 && recharges > 0
                    ? InvoiceServiceType.Combined
                    : shipping > 0
                        ? InvoiceServiceType.ShippingCharges
                        : InvoiceServiceType.WalletRecharge;

                var invoice = new Invoice
                {
                    SellerId = sellerId,
                    InvoiceNumber = $"INV-{sellerId}-{monthStart:yyyyMM}",
                    ServiceType = serviceType,
                    InvoiceDate = monthEnd < now.Date ? monthEnd : now.Date,
                    PeriodFrom = monthStart,
                    PeriodTo = monthEnd,
                    ShippingChargesAmount = shipping,
                    RechargeAmount = recharges,
                    Amount = total,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _invoiceRepository.AddAsync(invoice);
            }

            await _invoiceRepository.SaveChangesAsync();
        }

        private async Task ClearPrimaryAsync(long sellerId)
        {
            var accounts = await _bankAccountRepository.GetBySellerIdAsync(sellerId);
            foreach (var account in accounts.Where(a => a.IsPrimary))
            {
                // GetBySellerId is AsNoTracking — re-fetch tracked entity for update.
                var tracked = await _bankAccountRepository.GetByIdAsync(account.SellerBankAccountId);
                if (tracked == null)
                    continue;
                tracked.IsPrimary = false;
                tracked.UpdatedAt = DateTime.UtcNow;
                await _bankAccountRepository.UpdateAsync(tracked);
            }
        }

        private static void ValidateBankRequest(SellerBankAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountHolderName))
                throw new InvalidOperationException(SystemMessage.CodBankAccountHolderRequired);
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new InvalidOperationException(SystemMessage.CodBankAccountNumberRequired);
            if (string.IsNullOrWhiteSpace(request.IfscCode))
                throw new InvalidOperationException(SystemMessage.CodBankIfscRequired);
        }

        private static SettlementStatus? ParseSettlementStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase))
                return null;

            return status.Trim().ToLowerInvariant() switch
            {
                "pending" => SettlementStatus.Pending,
                "settled" => SettlementStatus.Settled,
                "failed" => SettlementStatus.Failed,
                _ => null
            };
        }

        private static IReadOnlyList<string>? ParseAwbs(string? awbNumbers)
        {
            if (string.IsNullOrWhiteSpace(awbNumbers))
                return null;

            var list = awbNumbers
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return list.Count == 0 ? null : list;
        }

        private static ShipmentChargesListItemResponse MapCharges(ShipmentCharges c)
        {
            var shipment = c.Shipment;
            var weightKg = c.ChargeableWeightGrams / 1000m;
            var enteredKg = shipment?.Weight ?? weightKg;

            return new ShipmentChargesListItemResponse
            {
                ShipmentChargesId = c.ShipmentChargesId,
                ShipmentId = c.ShipmentId,
                ShipmentCreated = c.CreatedAt ?? shipment?.CreatedAt,
                Courier = shipment?.Courier?.CourierName
                    ?? shipment?.Courier?.CourierCode
                    ?? string.Empty,
                AwbNumber = shipment?.AwbNumber,
                Status = shipment?.Status.ToString() ?? string.Empty,
                FreightCharges = c.SellerCharge,
                CodCharges = c.CodCharge,
                EnteredWeightKg = enteredKg,
                AppliedWeightKg = weightKg,
                ExtraWeightCharges = 0,
                RtoCharges = 0,
                CodChargeReversed = 0,
                RtoExtraWeightCharges = 0,
                ShipmentInsuranceCharges = 0,
                TotalCharges = c.SellerCharge + c.CodCharge
            };
        }

        private static CodRemittanceListItemResponse MapCod(CODSettlement c)
        {
            var remittance = c.CollectedAmount > 0 ? c.CollectedAmount : c.CodAmount;
            return new CodRemittanceListItemResponse
            {
                RemittanceId = c.CodSettlementId,
                ShipmentId = c.ShipmentId,
                AwbNumber = c.Shipment?.AwbNumber,
                CodAmount = c.CodAmount,
                Status = c.Status.ToString(),
                StatusCode = (int)c.Status,
                PaymentDate = c.SettlementDate,
                FreightDeductions = 0,
                RemittanceAmount = remittance,
                ConvenienceFee = 0,
                PaymentRef = c.Status == SettlementStatus.Settled ? $"COD-{c.CodSettlementId}" : null,
                Remark = null
            };
        }

        private static SellerBankAccountResponse MapBank(SellerBankAccount a) => new()
        {
            SellerBankAccountId = a.SellerBankAccountId,
            SellerId = a.SellerId,
            AccountHolderName = a.AccountHolderName,
            AccountNumber = a.AccountNumber,
            IfscCode = a.IfscCode,
            BankName = a.BankName,
            BranchName = a.BranchName,
            AccountType = a.AccountType,
            IsPrimary = a.IsPrimary
        };

        private static InvoiceListItemResponse MapInvoice(Invoice i) => new()
        {
            InvoiceId = i.InvoiceId,
            InvoiceNumber = i.InvoiceNumber,
            ServiceType = i.ServiceType,
            InvoiceDate = i.InvoiceDate,
            InvoicePeriod = $"{i.PeriodFrom:dd MMM yyyy} - {i.PeriodTo:dd MMM yyyy}",
            InvoiceAmount = i.Amount,
            PeriodFrom = i.PeriodFrom,
            PeriodTo = i.PeriodTo
        };

        private static string Csv(string? value)
        {
            var v = value ?? string.Empty;
            if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }
    }
}

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using TryNextPost.Application.DTO.Billing;
using TryNextPost.Application.IServices.Class.RateCard;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IBilling;
using TryNextPost.Application.IServices.Interface.IRateCard;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Billing
{
    public class BillingService : IBillingService
    {
        private static readonly ConcurrentDictionary<long, DateTime> CodSyncLastRunUtc = new();
        private static readonly TimeSpan CodSyncMinInterval = TimeSpan.FromMinutes(5);

        private readonly ISellerContextService _sellerContextService;
        private readonly IShipmentChargesRepository _shipmentChargesRepository;
        private readonly ICODSettlementRepository _codSettlementRepository;
        private readonly ISellerBankAccountRepository _bankAccountRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IWalletRechargeRepository _walletRechargeRepository;
        private readonly IRateCalculationService _rateCalculationService;
        private readonly ICourierRepository _courierRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly ICourierRateCardRepository _rateCardRepository;
        private readonly IWeightDiscrepancyRepository _weightDiscrepancyRepository;
        private readonly ILogger<BillingService> _logger;

        public BillingService(
            ISellerContextService sellerContextService,
            IShipmentChargesRepository shipmentChargesRepository,
            ICODSettlementRepository codSettlementRepository,
            ISellerBankAccountRepository bankAccountRepository,
            IInvoiceRepository invoiceRepository,
            IWalletRechargeRepository walletRechargeRepository,
            IRateCalculationService rateCalculationService,
            ICourierRepository courierRepository,
            IZoneRepository zoneRepository,
            ICourierRateCardRepository rateCardRepository,
            IWeightDiscrepancyRepository weightDiscrepancyRepository,
            ILogger<BillingService> logger)
        {
            _sellerContextService = sellerContextService;
            _shipmentChargesRepository = shipmentChargesRepository;
            _codSettlementRepository = codSettlementRepository;
            _bankAccountRepository = bankAccountRepository;
            _invoiceRepository = invoiceRepository;
            _walletRechargeRepository = walletRechargeRepository;
            _rateCalculationService = rateCalculationService;
            _courierRepository = courierRepository;
            _zoneRepository = zoneRepository;
            _rateCardRepository = rateCardRepository;
            _weightDiscrepancyRepository = weightDiscrepancyRepository;
            _logger = logger;
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

            var shipmentIds = items.Select(i => i.ShipmentId).Distinct().ToList();
            var extraWeightByShipment = await _weightDiscrepancyRepository
                .GetAcceptedWeightChargesByShipmentIdsAsync(shipmentIds);
            var rtoChargesByShipment = await ComputeRtoChargesAsync(items);

            return new ShipmentChargesListResponse
            {
                Items = items.Select(c => MapCharges(c, extraWeightByShipment, rtoChargesByShipment)).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CodRemittanceSummaryResponse> GetCodSummaryAsync(string userId)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            // Sync only on summary (not list), and only when cheap checks say work may exist.
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
            // Do NOT sync-on-read here — FE also calls summary which handles sync. List stays fast.

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);
            var status = ParseSettlementStatus(filter.Status);

            var sw = Stopwatch.StartNew();
            var (items, total) = await _codSettlementRepository.GetFilteredAsync(
                seller.SellerId,
                status,
                filter.FromDate,
                filter.ToDate,
                page,
                pageSize);

            var shipmentIds = items.Select(i => i.ShipmentId).Distinct().ToList();
            var freightByShipment = await _shipmentChargesRepository
                .GetSellerChargesByShipmentIdsAsync(shipmentIds);
            _logger.LogInformation(
                "COD GetCodRemittancesAsync seller={SellerId} rows={Rows} totalMs={Ms}",
                seller.SellerId, items.Count, sw.ElapsedMilliseconds);

            return new CodRemittanceListResponse
            {
                Items = items.Select(c => MapCod(c, freightByShipment)).ToList(),
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

            var sw = Stopwatch.StartNew();
            await EnsureMonthlyInvoicesAsync(seller.SellerId, userId);
            var ensureMs = sw.ElapsedMilliseconds;

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);

            sw.Restart();
            var (items, total) = await _invoiceRepository.GetFilteredAsync(
                seller.SellerId,
                filter.FromDate,
                filter.ToDate,
                page,
                pageSize);
            _logger.LogInformation(
                "Invoice GetInvoicesAsync seller={SellerId} ensureMs={EnsureMs} listMs={ListMs} rows={Rows}",
                seller.SellerId, ensureMs, sw.ElapsedMilliseconds, items.Count);

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
            var now = DateTime.UtcNow;
            if (CodSyncLastRunUtc.TryGetValue(sellerId, out var lastRun)
                && now - lastRun < CodSyncMinInterval)
            {
                return;
            }

            // Cheap short-circuit: skip materializing rows when nothing to insert.
            if (!await _codSettlementRepository.HasUnsettledDeliveredCodShipmentsAsync(sellerId))
            {
                CodSyncLastRunUtc[sellerId] = now;
                return;
            }

            var sw = Stopwatch.StartNew();
            var unsettled = await _codSettlementRepository.GetUnsettledDeliveredCodShipmentsAsync(sellerId);
            if (unsettled.Count == 0)
            {
                CodSyncLastRunUtc[sellerId] = now;
                return;
            }

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
            CodSyncLastRunUtc[sellerId] = now;
            _logger.LogInformation(
                "COD SyncPending inserted={Count} seller={SellerId} ms={Ms}",
                rows.Count, sellerId, sw.ElapsedMilliseconds);
        }

        private async Task EnsureMonthlyInvoicesAsync(long sellerId, string userId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Fast path: current-month invoice already exists → no write-path work on list read.
            if (await _invoiceRepository.ExistsForSellerPeriodAsync(sellerId, monthStart, monthEnd))
                return;

            var shipping = await _shipmentChargesRepository.SumSellerChargeForPeriodAsync(
                sellerId, monthStart, monthEnd);
            var recharges = await _walletRechargeRepository.SumPaidForSellerPeriodAsync(
                sellerId, monthStart, monthEnd);

            var total = shipping + recharges;
            if (total <= 0)
                return;

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

        private static ShipmentChargesListItemResponse MapCharges(
            ShipmentCharges c,
            IReadOnlyDictionary<long, decimal> extraWeightByShipment,
            IReadOnlyDictionary<long, (decimal RtoCharge, decimal RtoExtraWeight)> rtoChargesByShipment)
        {
            var shipment = c.Shipment;
            var weightKg = c.ChargeableWeightGrams / 1000m;
            var enteredKg = shipment?.Weight ?? weightKg;

            extraWeightByShipment.TryGetValue(c.ShipmentId, out var extraWeightCharges);

            var isRto = shipment?.Status == ShipmentStatus.RTO;
            var codChargeReversed = isRto ? c.CodCharge : 0m;

            rtoChargesByShipment.TryGetValue(c.ShipmentId, out var rtoParts);
            var rtoCharges = isRto ? rtoParts.RtoCharge : 0m;
            var rtoExtraWeightCharges = isRto ? rtoParts.RtoExtraWeight + extraWeightCharges : 0m;
            var forwardExtraWeight = isRto ? 0m : extraWeightCharges;

            const decimal shipmentInsuranceCharges = 0m;

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
                ExtraWeightCharges = forwardExtraWeight,
                RtoCharges = rtoCharges,
                CodChargeReversed = codChargeReversed,
                RtoExtraWeightCharges = rtoExtraWeightCharges,
                ShipmentInsuranceCharges = shipmentInsuranceCharges,
                TotalCharges = c.SellerCharge + c.CodCharge + forwardExtraWeight
                    + rtoCharges + rtoExtraWeightCharges + shipmentInsuranceCharges
                    - codChargeReversed
            };
        }

        private async Task<Dictionary<long, (decimal RtoCharge, decimal RtoExtraWeight)>> ComputeRtoChargesAsync(
            List<ShipmentCharges> items)
        {
            var result = new Dictionary<long, (decimal RtoCharge, decimal RtoExtraWeight)>();
            var rtoItems = items
                .Where(c => c.Shipment?.Status == ShipmentStatus.RTO && c.Shipment.CourierId > 0)
                .ToList();

            if (rtoItems.Count == 0)
                return result;

            var zones = await _zoneRepository.GetAllZonesAsync();
            var zoneByCode = zones
                .Where(z => !string.IsNullOrWhiteSpace(z.ZoneCode))
                .ToDictionary(z => z.ZoneCode, z => z.ZoneId, StringComparer.OrdinalIgnoreCase);

            // One rate-card load per courier on the page — avoid N+1 FindRateAsync roundtrips.
            var cardsByCourier = new Dictionary<long, List<CourierRateCard>>();
            foreach (var courierId in rtoItems.Select(c => c.Shipment!.CourierId).Distinct())
            {
                cardsByCourier[courierId] = await _rateCardRepository.GetByCourierAsync(courierId);
            }

            foreach (var c in rtoItems)
            {
                var shipment = c.Shipment!;
                var originCode = c.OriginZoneCode?.Trim();
                var destCode = c.DestinationZoneCode?.Trim();

                if (string.IsNullOrWhiteSpace(originCode) || string.IsNullOrWhiteSpace(destCode))
                    continue;

                if (!zoneByCode.TryGetValue(destCode, out var returnFromZoneId) ||
                    !zoneByCode.TryGetValue(originCode, out var returnToZoneId))
                    continue;

                if (!cardsByCourier.TryGetValue(shipment.CourierId, out var cards))
                    continue;

                var card = cards
                    .Where(r =>
                        r.FromZoneId == returnFromZoneId
                        && r.ToZoneId == returnToZoneId
                        && r.WeightFromGrams <= c.ChargeableWeightGrams
                        && r.WeightToGrams >= c.ChargeableWeightGrams
                        && string.Equals(r.ServiceCode, "RTO", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.SellerCharge)
                    .FirstOrDefault();

                if (card == null)
                    continue;

                result[c.ShipmentId] = (card.SellerCharge, 0m);
            }

            return result;
        }

        private static CodRemittanceListItemResponse MapCod(
            CODSettlement c,
            IReadOnlyDictionary<long, decimal> freightByShipment)
        {
            var remittance = c.CollectedAmount > 0 ? c.CollectedAmount : c.CodAmount;
            freightByShipment.TryGetValue(c.ShipmentId, out var freightDeductions);

            // No seller/courier convenience-fee config exists yet — genuinely N/A.
            const decimal convenienceFee = 0m;

            return new CodRemittanceListItemResponse
            {
                RemittanceId = c.CodSettlementId,
                ShipmentId = c.ShipmentId,
                AwbNumber = c.Shipment?.AwbNumber,
                CodAmount = c.CodAmount,
                Status = c.Status.ToString(),
                StatusCode = (int)c.Status,
                PaymentDate = c.SettlementDate,
                FreightDeductions = freightDeductions,
                RemittanceAmount = remittance,
                ConvenienceFee = convenienceFee,
                PaymentRef = c.Status == SettlementStatus.Settled
                    ? c.PaymentReference ?? $"COD-{c.CodSettlementId}"
                    : null,
                Remark = c.Remark
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

        public async Task<PriceCalculatorResponse> CalculatePriceAsync(string userId, PriceCalculatorRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);

            if (string.IsNullOrWhiteSpace(request.OriginPincode)
          || string.IsNullOrWhiteSpace(request.DestinationPincode))
                throw new InvalidOperationException(SystemMessage.PriceCalculatorPincodeRequired);
            if (request.WeightGrams <= 0)
                throw new InvalidOperationException(SystemMessage.PriceCalculatorWeightInvalid);

            // Resolve zones once — not once per courier.
            var originZone = await _zoneRepository.GetZoneByPincodeAsync(request.OriginPincode);
            var destZone = await _zoneRepository.GetZoneByPincodeAsync(request.DestinationPincode);
            if (originZone == null || destZone == null)
                throw new InvalidOperationException(SystemMessage.PriceCalculatorNoRates);

            var couriers = await _courierRepository.GetActiveCouriersAsync();
            if (request.CourierId.HasValue)
                couriers = couriers
                    .Where(c => c.CourierId == request.CourierId.Value)
                    .ToList();
            var options = new List<PriceCalculatorOptionDto>();
            foreach (var courier in couriers)
            {
                var quotes = await _rateCalculationService.GetRatesForCourierZonesAsync(
                    courier.CourierId,
                    courier.CourierCode,
                    courier.CourierName,
                    originZone,
                    destZone,
                    request.WeightGrams,
                    request.VolumetricWeightGrams,
                    request.IsCod,
                    courier.CodChargeType,
                    courier.CodChargeValue,
                    request.CodAmount,
                    courier.SupportsCOD);
                foreach (var q in quotes)
                {
                    options.Add(new PriceCalculatorOptionDto
                    {
                        CourierId = courier.CourierId,
                        CourierCode = courier.CourierCode,
                        CourierName = courier.CourierName,
                        ServiceCode = q.ServiceCode,
                        ServiceName = q.ServiceName,
                        SellerCharge = q.SellerCharge,
                        CodCharge = q.CodCharge,
                        TotalCharge = q.TotalCharge,
                        EstimatedDays = q.EstimatedDays,
                        OriginZoneCode = q.OriginZoneCode,
                        DestinationZoneCode = q.DestinationZoneCode,
                        ChargeableWeightGrams = q.ChargeableWeightGrams
                    });
                }
            }
            if (options.Count == 0)
                throw new InvalidOperationException(SystemMessage.PriceCalculatorNoRates);
            return new PriceCalculatorResponse
            {
                OriginPincode = request.OriginPincode.Trim(),
                DestinationPincode = request.DestinationPincode.Trim(),
                ChargeableWeightGrams = options.First().ChargeableWeightGrams,
                Rates = options.OrderBy(o => o.TotalCharge).ToList()
            };
        }

        public async Task<RateChartResponse> GetRateChartAsync(string userId, RateChartRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(
           userId, EmployeePermissionCode.WalletViewBalance);

            var zones = await _zoneRepository.GetAllZonesAsync();
            if (zones == null || zones.Count == 0)
                throw new InvalidOperationException(SystemMessage.RateChartNoData);
            var fromZone = request.FromZoneId.HasValue
                ? zones.FirstOrDefault(z => z.ZoneId == request.FromZoneId.Value)
                : zones.First();
            if (fromZone == null)
                throw new InvalidOperationException(SystemMessage.RateChartNoData);

            var isRto = string.Equals(request.Direction, "rto", StringComparison.OrdinalIgnoreCase);
            var serviceCode = string.IsNullOrWhiteSpace(request.ServiceCode)
                ? (isRto ? "RTO" : "SURFACE")
                : request.ServiceCode.Trim().ToUpperInvariant();

            if (isRto && string.IsNullOrWhiteSpace(request.ServiceCode))
                serviceCode = "RTO";

            var couriers = await _courierRepository.GetActiveCouriersAsync();
            if (request.CourierId.HasValue && request.CourierId.Value > 0)
                couriers = couriers.Where(c => c.CourierId == request.CourierId.Value).ToList();
            var rows = new List<RateChartRowDto>();
            foreach (var courier in couriers)
            {
                var cards = await _rateCardRepository.GetByCourierAsync(courier.CourierId);
                cards = cards
                    .Where(c => c.FromZoneId == fromZone.ZoneId)
                    .Where(c => string.Equals(c.ServiceCode, serviceCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (request.WeightGrams.HasValue && request.WeightGrams.Value > 0)
                {
                    var w = request.WeightGrams.Value;
                    cards = cards
                        .Where(c => c.WeightFromGrams <= w && c.WeightToGrams >= w)
                        .ToList();
                }
                var groups = cards.GroupBy(c => new
                {
                    c.ServiceCode,
                    c.WeightFromGrams,
                    c.WeightToGrams
                });
                var codLabel = RateCalculationService.FormatCodLabel(courier.CodChargeType, courier.CodChargeValue);
                var codFlat = courier.CodChargeType == Domain.Enums.CodChargeType.Flat
                    ? courier.CodChargeValue
                    : 0m;
                foreach (var g in groups)
                {
                    var zoneRates = new Dictionary<string, decimal?>();
                    foreach (var z in zones)
                        zoneRates[z.ZoneCode] = null;
                    foreach (var card in g)
                    {
                        var toCode = card.ToZone?.ZoneCode
                            ?? zones.FirstOrDefault(z => z.ZoneId == card.ToZoneId)?.ZoneCode;
                        if (!string.IsNullOrEmpty(toCode))
                            zoneRates[toCode] = card.SellerCharge;
                    }
                    rows.Add(new RateChartRowDto
                    {
                        CourierId = courier.CourierId,
                        CourierName = courier.CourierName,
                        CourierCode = courier.CourierCode,
                        ServiceCode = g.Key.ServiceCode,
                        WeightFromGrams = g.Key.WeightFromGrams,
                        WeightToGrams = g.Key.WeightToGrams,
                        WeightLabel = $"{g.Key.WeightToGrams / 1000m:0.##} kg",
                        ZoneRates = zoneRates,
                        CodChargeType = (int)courier.CodChargeType,
                        CodChargeValue = courier.CodChargeValue,
                        CodChargeFlat = codFlat,
                        CodLabel = codLabel
                    });
                }
            }

            string? infoMessage = null;
            if (isRto && rows.Count == 0)
                infoMessage = SystemMessage.RateChartRtoNotConfigured;

            return new RateChartResponse
            {
                FromZoneId = fromZone.ZoneId,
                FromZoneCode = fromZone.ZoneCode,
                Zones = zones.Select(z => new RateChartZoneColumnDto
                {
                    ZoneId = z.ZoneId,
                    ZoneCode = z.ZoneCode,
                    ZoneLabel = z.ZoneName ?? z.ZoneCode
                }).ToList(),
                Rows = rows
                    .OrderBy(r => r.CourierName)
                    .ThenBy(r => r.WeightFromGrams)
                    .ToList(),
                InfoMessage = infoMessage
            };
        }
    }
}

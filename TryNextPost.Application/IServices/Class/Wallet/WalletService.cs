using System.Text.Json;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Wallet;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IPayment;
using TryNextPost.Application.IServices.Interface.IWallet;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Wallet
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletRechargeRepository _rechargeRepository;
        private readonly IRazorpayPaymentGateway _razorpay;
        private readonly RazorpaySettings _razorpaySettings;
        private readonly ISellerContextService _sellerContextService;
        private readonly ISellerRepository _sellerRepository;

        public WalletService(
            IWalletRepository walletRepository,
            IWalletRechargeRepository rechargeRepository,
            IRazorpayPaymentGateway razorpay,
            IOptions<RazorpaySettings> razorpaySettings,
            ISellerContextService sellerContextService,
            ISellerRepository sellerRepository)
        {
            _walletRepository = walletRepository;
            _rechargeRepository = rechargeRepository;
            _razorpay = razorpay;
            _razorpaySettings = razorpaySettings.Value;
            _sellerContextService = sellerContextService;
            _sellerRepository = sellerRepository;
        }

        public async Task<WalletBalanceResponse> GetOrCreateBalanceAsync(string userId)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletViewBalance);
            return await GetSellerWalletBalanceAsync(userId);
        }

        public async Task<WalletBalanceResponse> GetSellerWalletBalanceAsync(string userId)
        {
            var context = await _sellerContextService.ResolveAsync(userId);
            var wallet = await EnsureWalletAsync(context.SellerId, context.UserId);
            if (wallet.WalletId == 0)
                await _walletRepository.SaveChangesAsync();
            return Map(wallet);
        }

        public async Task<WalletBalanceResponse> CreditAsync(
            string userId,
            WalletCreditRequest request,
            string? performedBy = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("Target UserId is required.");

            if (request.Amount <= 0)
                throw new InvalidOperationException(SystemMessage.WalletAmountInvalid);

            var actor = string.IsNullOrWhiteSpace(performedBy) ? userId : performedBy;
            var seller = await _sellerRepository.GetByUserIdAsync(userId.Trim())
                ?? throw new KeyNotFoundException(SystemMessage.SellerNotFound);

            var wallet = await EnsureWalletAsync(seller.SellerId, seller.UserId);

            if (wallet.WalletId == 0)
                await _walletRepository.SaveChangesAsync();

            wallet.Balance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            wallet.UpdatedBy = actor;

            var txn = new Transaction
            {
                WalletId = wallet.WalletId,
                TxnType = TransactionType.Credit,
                Amount = request.Amount,
                TxnReference = $"CR-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Random.Shared.Next(1000, 9999)}",
                ReferenceId = request.ReferenceId,
                Description = request.Description ?? "Wallet credit (admin)",
                Status = TransactionStatus.Success,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actor
            };

            await _walletRepository.AddTransactionAsync(txn);
            await _walletRepository.SaveChangesAsync();
            return Map(wallet);
        }

        public async Task<WalletBalanceResponse> DebitForShipmentAsync(
            string userId,
            decimal amount,
            long shipmentId,
            string? awbNumber,
            string? performedBy)
        {
            if (amount <= 0)
                throw new InvalidOperationException(SystemMessage.WalletAmountInvalid);

            var context = await _sellerContextService.ResolveAsync(userId);
            var wallet = await EnsureWalletAsync(context.SellerId, context.UserId);

            if (wallet.WalletId == 0)
                await _walletRepository.SaveChangesAsync();

            if (wallet.Balance < amount)
                throw new InvalidOperationException(SystemMessage.WalletInsufficientBalance);

            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            wallet.UpdatedBy = performedBy ?? userId;

            var txn = new Transaction
            {
                WalletId = wallet.WalletId,
                TxnType = TransactionType.Debit,
                Amount = amount,
                TxnReference = $"SHIP-{shipmentId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                ReferenceId = shipmentId.ToString(),
                Description = string.IsNullOrWhiteSpace(awbNumber)
                    ? $"Shipment booking debit (ShipmentId={shipmentId})"
                    : $"Shipment booking debit AWB {awbNumber}",
                Status = TransactionStatus.Success,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedBy ?? userId
            };

            await _walletRepository.AddTransactionAsync(txn);
            await _walletRepository.SaveChangesAsync();
            return Map(wallet);
        }

        public async Task<WalletRechargeResponse> CreateRechargeAsync(string userId, WalletRechargeRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.WalletRecharge);

            if (request.Amount <= 0)
                throw new InvalidOperationException(SystemMessage.WalletAmountInvalid);

            var amountRupees = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
            var amountPaise = (int)(amountRupees * 100m);
            if (amountPaise <= 0)
                throw new InvalidOperationException(SystemMessage.WalletAmountInvalid);

            var context = await _sellerContextService.ResolveAsync(userId);
            var wallet = await EnsureWalletAsync(context.SellerId, context.UserId);
            if (wallet.WalletId == 0)
                await _walletRepository.SaveChangesAsync();

            var receipt = $"wr_{wallet.WalletId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            var notes = new Dictionary<string, string>
            {
                ["userId"] = userId,
                ["sellerId"] = context.SellerId.ToString(),
                ["walletId"] = wallet.WalletId.ToString()
            };

            var order = await _razorpay.CreateOrderAsync(amountPaise, receipt, notes);

            var recharge = new WalletRecharge
            {
                UserId = userId,
                WalletId = wallet.WalletId,
                Amount = amountRupees,
                AmountInPaise = amountPaise,
                Currency = "INR",
                GatewayOrderId = order.OrderId,
                Status = WalletRechargeStatus.Pending,
                Receipt = receipt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _rechargeRepository.AddAsync(recharge);
            await _rechargeRepository.SaveChangesAsync();

            return new WalletRechargeResponse
            {
                PaymentOrderId = recharge.WalletRechargeId,
                GatewayOrderId = recharge.GatewayOrderId,
                KeyId = _razorpaySettings.KeyId,
                Amount = recharge.Amount,
                AmountInPaise = recharge.AmountInPaise,
                Currency = recharge.Currency,
                Receipt = recharge.Receipt
            };
        }

        public async Task<PaymentWebhookResponse> HandleWebhookAsync(string rawBody, string? signature)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                throw new InvalidOperationException(SystemMessage.RequestBodyNull);

            if (string.IsNullOrWhiteSpace(signature) || !_razorpay.VerifyWebhookSignature(rawBody, signature))
                throw new UnauthorizedAccessException(SystemMessage.WalletWebhookInvalidSignature);

            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            var eventName = root.TryGetProperty("event", out var eventProp)
                ? eventProp.GetString() ?? string.Empty
                : string.Empty;

            if (!string.Equals(eventName, "payment.captured", StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentWebhookResponse
                {
                    Processed = false,
                    Event = eventName,
                    Message = SystemMessage.WalletWebhookIgnored
                };
            }

            if (!TryReadCapturedPayment(root, out var orderId, out var paymentId, out var amountPaise))
            {
                throw new InvalidOperationException("Razorpay webhook payload missing payment entity fields.");
            }

            var result = await CreditFromGatewayAsync(orderId, paymentId, amountPaise, performedBy: "razorpay-webhook");

            return new PaymentWebhookResponse
            {
                Processed = true,
                Event = eventName,
                GatewayOrderId = orderId,
                Message = result.AlreadyProcessed
                    ? SystemMessage.WalletPaymentAlreadyProcessed
                    : SystemMessage.WalletWebhookAccepted
            };
        }

        public async Task<VerifyPaymentResponse> VerifyPaymentAsync(string userId, VerifyPaymentRequest request)
        {
            if (!_razorpay.VerifyPaymentSignature(
                    request.RazorpayOrderId,
                    request.RazorpayPaymentId,
                    request.RazorpaySignature))
            {
                throw new InvalidOperationException("Invalid Razorpay payment signature.");
            }

            var context = await _sellerContextService.ResolveAsync(userId);
            var recharge = await _rechargeRepository.GetByGatewayOrderIdAsync(request.RazorpayOrderId)
                ?? throw new KeyNotFoundException(SystemMessage.WalletRechargeNotFound);

            var wallet = await _walletRepository.GetByIdAsync(recharge.WalletId)
                ?? throw new KeyNotFoundException(SystemMessage.WalletNotFound);

            if (wallet.SellerId != context.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            var result = await CreditFromGatewayAsync(
                request.RazorpayOrderId,
                request.RazorpayPaymentId,
                recharge.AmountInPaise,
                performedBy: userId);

            return result;
        }

        private async Task<VerifyPaymentResponse> CreditFromGatewayAsync(
            string gatewayOrderId,
            string gatewayPaymentId,
            int amountPaiseHint,
            string performedBy)
        {
            var recharge = await _rechargeRepository.GetByGatewayOrderIdAsync(gatewayOrderId)
                ?? throw new KeyNotFoundException(SystemMessage.WalletRechargeNotFound);

            var wallet = await _walletRepository.GetByIdAsync(recharge.WalletId)
                ?? throw new KeyNotFoundException(SystemMessage.WalletNotFound);

            if (recharge.Status == WalletRechargeStatus.Paid)
            {
                return new VerifyPaymentResponse
                {
                    PaymentOrderId = recharge.WalletRechargeId,
                    GatewayOrderId = recharge.GatewayOrderId,
                    GatewayPaymentId = recharge.GatewayPaymentId,
                    Status = WalletRechargeStatus.Paid.ToString(),
                    Amount = recharge.Amount,
                    WalletBalance = wallet.Balance,
                    AlreadyProcessed = true
                };
            }

            if (amountPaiseHint > 0 && amountPaiseHint != recharge.AmountInPaise)
            {
                throw new InvalidOperationException(
                    $"Payment amount mismatch for order {gatewayOrderId}.");
            }

            wallet.Balance += recharge.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            wallet.UpdatedBy = performedBy;

            recharge.Status = WalletRechargeStatus.Paid;
            recharge.GatewayPaymentId = gatewayPaymentId;
            recharge.UpdatedAt = DateTime.UtcNow;
            recharge.UpdatedBy = performedBy;

            var txn = new Transaction
            {
                WalletId = wallet.WalletId,
                TxnType = TransactionType.Credit,
                Amount = recharge.Amount,
                TxnReference = gatewayPaymentId,
                ReferenceId = recharge.WalletRechargeId.ToString(),
                Description = $"Wallet recharge via Razorpay ({gatewayOrderId})",
                Status = TransactionStatus.Success,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedBy
            };

            await _walletRepository.AddTransactionAsync(txn);
            await _rechargeRepository.UpdateAsync(recharge);
            await _walletRepository.SaveChangesAsync();

            return new VerifyPaymentResponse
            {
                PaymentOrderId = recharge.WalletRechargeId,
                GatewayOrderId = recharge.GatewayOrderId,
                GatewayPaymentId = recharge.GatewayPaymentId,
                Status = WalletRechargeStatus.Paid.ToString(),
                Amount = recharge.Amount,
                WalletBalance = wallet.Balance,
                AlreadyProcessed = false
            };
        }

        private static bool TryReadCapturedPayment(
            JsonElement root,
            out string orderId,
            out string paymentId,
            out int amountPaise)
        {
            orderId = string.Empty;
            paymentId = string.Empty;
            amountPaise = 0;

            if (!root.TryGetProperty("payload", out var payload)
                || !payload.TryGetProperty("payment", out var payment)
                || !payment.TryGetProperty("entity", out var entity))
            {
                return false;
            }

            paymentId = entity.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
            orderId = entity.TryGetProperty("order_id", out var orderProp) ? orderProp.GetString() ?? string.Empty : string.Empty;

            if (entity.TryGetProperty("amount", out var amountProp) && amountProp.TryGetInt32(out var paise))
                amountPaise = paise;

            return !string.IsNullOrWhiteSpace(paymentId) && !string.IsNullOrWhiteSpace(orderId);
        }

        private async Task<Domain.Entities.Wallet> EnsureWalletAsync(long sellerId, string actingUserId)
        {
            var wallet = await _walletRepository.GetBySellerIdAsync(sellerId);
            if (wallet != null)
                return wallet;

            var seller = await _sellerRepository.GetByIdAsync(sellerId);
            var ownerUserId = seller?.UserId ?? actingUserId;

            wallet = new Domain.Entities.Wallet
            {
                SellerId = sellerId,
                UserId = ownerUserId,
                Balance = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actingUserId
            };

            await _walletRepository.AddAsync(wallet);
            return wallet;
        }

        private static WalletBalanceResponse Map(Domain.Entities.Wallet wallet)
        {
            return new WalletBalanceResponse
            {
                WalletId = wallet.WalletId,
                SellerId = wallet.SellerId,
                UserId = wallet.UserId,
                Balance = wallet.Balance
            };
        }
    }
}

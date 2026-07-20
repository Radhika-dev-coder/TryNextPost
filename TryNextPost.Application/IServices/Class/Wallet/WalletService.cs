using System.Text.Json;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Wallet;
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

        public WalletService(
            IWalletRepository walletRepository,
            IWalletRechargeRepository rechargeRepository,
            IRazorpayPaymentGateway razorpay,
            IOptions<RazorpaySettings> razorpaySettings)
        {
            _walletRepository = walletRepository;
            _rechargeRepository = rechargeRepository;
            _razorpay = razorpay;
            _razorpaySettings = razorpaySettings.Value;
        }

        public async Task<WalletBalanceResponse> GetOrCreateBalanceAsync(string userId)
        {
            var wallet = await EnsureWalletAsync(userId);
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
            var wallet = await EnsureWalletAsync(userId.Trim());

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

        public async Task DebitForShipmentAsync(
            string userId,
            decimal amount,
            long shipmentId,
            string? awbNumber,
            string? performedBy)
        {
            if (amount <= 0)
                throw new InvalidOperationException(SystemMessage.WalletAmountInvalid);

            var wallet = await EnsureWalletAsync(userId);

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
            // Caller owns the outer SaveChanges when used inside confirm; still safe to save here
            // so debit is durable even if caller forgets — confirm will save again.
            await _walletRepository.SaveChangesAsync();
        }

        public async Task<WalletRechargeResponse> CreateRechargeAsync(string userId, WalletRechargeRequest request)
        {
            if (request.Amount <= 0)
                throw new InvalidOperationException(SystemMessage.WalletAmountInvalid);

            // Round to paise (2 decimal places) then convert — Razorpay amount is integer paise.
            var amountRupees = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
            var amountPaise = (int)(amountRupees * 100m);
            if (amountPaise <= 0)
                throw new InvalidOperationException(SystemMessage.WalletAmountInvalid);

            var wallet = await EnsureWalletAsync(userId);
            if (wallet.WalletId == 0)
                await _walletRepository.SaveChangesAsync();

            var receipt = $"wr_{wallet.WalletId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            var notes = new Dictionary<string, string>
            {
                ["userId"] = userId,
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

            var recharge = await _rechargeRepository.GetByGatewayOrderIdAsync(request.RazorpayOrderId)
                ?? throw new KeyNotFoundException(SystemMessage.WalletRechargeNotFound);

            if (!string.Equals(recharge.UserId, userId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            var result = await CreditFromGatewayAsync(
                request.RazorpayOrderId,
                request.RazorpayPaymentId,
                recharge.AmountInPaise,
                performedBy: userId);

            return result;
        }

        /// <summary>
        /// Idempotent credit: if already Paid, returns current balance without double-crediting.
        /// </summary>
        private async Task<VerifyPaymentResponse> CreditFromGatewayAsync(
            string gatewayOrderId,
            string gatewayPaymentId,
            int amountPaiseHint,
            string performedBy)
        {
            var recharge = await _rechargeRepository.GetByGatewayOrderIdAsync(gatewayOrderId)
                ?? throw new KeyNotFoundException(SystemMessage.WalletRechargeNotFound);

            var wallet = await _walletRepository.GetByUserIdAsync(recharge.UserId)
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

            // Prefer stored amount; fall back to webhook paise if present and matching order.
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
                // Unique index on TxnReference → second webhook/verify cannot double-insert.
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

        private async Task<Domain.Entities.Wallet> EnsureWalletAsync(string userId)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            if (wallet != null)
                return wallet;

            wallet = new Domain.Entities.Wallet
            {
                UserId = userId,
                Balance = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _walletRepository.AddAsync(wallet);
            return wallet;
        }

        private static WalletBalanceResponse Map(Domain.Entities.Wallet wallet)
        {
            return new WalletBalanceResponse
            {
                WalletId = wallet.WalletId,
                UserId = wallet.UserId,
                Balance = wallet.Balance
            };
        }
    }
}

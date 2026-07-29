using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Billing;
using TryNextPost.Application.DTO.Wallet;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IBilling;
using TryNextPost.Application.IServices.Interface.IWallet;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Billing
{
    public class CreditNoteService : ICreditNoteService
    {
        private readonly ICreditNoteRepository _repository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly ISellerContextService _sellerContextService;
        private readonly IWalletService _walletService;

                public CreditNoteService(
            ICreditNoteRepository repository,
            IInvoiceRepository invoiceRepository,
            ISellerRepository sellerRepository,
            ISellerContextService sellerContextService,
            IWalletService walletService)
        {
            _repository = repository;
            _invoiceRepository = invoiceRepository;
            _sellerRepository = sellerRepository;
            _sellerContextService = sellerContextService;
            _walletService = walletService;
        }

        public async Task<CreditNoteListItemResponse> CreateForAdminAsync(string adminUserId, CreditNoteCreateRequest request)
        {
            if (request.InvoiceId <= 0)
                throw new InvalidOperationException(SystemMessage.CreditNoteInvoiceRequired);
            if (request.Amount <= 0)
                throw new InvalidOperationException(SystemMessage.CreditNoteAmountInvalid);
            if (!Enum.IsDefined(typeof(CreditNoteReasonType), request.ReasonType))
                throw new InvalidOperationException(SystemMessage.CreditNoteReasonInvalid);
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId)
                ?? throw new KeyNotFoundException(SystemMessage.InvoiceNotFound);
            if (request.Amount > invoice.Amount)
                throw new InvalidOperationException(SystemMessage.CreditNoteAmountExceedsInvoice);
            var seller = await _sellerRepository.GetByIdAsync(invoice.SellerId)
                ?? throw new KeyNotFoundException(SystemMessage.TdsCertificateSellerNotFound);
            var now = DateTime.UtcNow;
            var seq = await _repository.CountForSellerInMonthAsync(
                seller.SellerId, now.Year, now.Month) + 1;
            var cnNumber = $"CN-{seller.SellerId}-{now:yyyyMM}-{seq:D3}";
            var period = $"{invoice.PeriodFrom:dd MMM yyyy} - {invoice.PeriodTo:dd MMM yyyy}";
            var reason = (CreditNoteReasonType)request.ReasonType;
            var entity = new CreditNote
            {
                SellerId = seller.SellerId,
                InvoiceId = invoice.InvoiceId,
                CreditNoteNumber = cnNumber,
                Amount = request.Amount,
                ReasonType = reason,
                Status = CreditNoteStatus.Issued,
                Remark = string.IsNullOrWhiteSpace(request.Remark) ? null : request.Remark.Trim(),
                Period = period,
                CreditNoteDate = now.Date,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = adminUserId
            };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            if (request.ApplyToWallet)
            {
                await _walletService.CreditAsync(
                    seller.UserId,
                    new WalletCreditRequest
                    {
                        UserId = seller.UserId,
                        Amount = request.Amount,
                        Description = $"Credit note {cnNumber}"
                            + (string.IsNullOrWhiteSpace(entity.Remark) ? "" : $": {entity.Remark}"),
                        ReferenceId = cnNumber
                    },
                    performedBy: adminUserId);
                entity.Status = CreditNoteStatus.Applied;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = adminUserId;
                await _repository.UpdateAsync(entity);
                await _repository.SaveChangesAsync();
            }
            var saved = await _repository.GetByIdAsync(entity.CreditNoteId, includeInvoice: true)
                ?? entity;
            return Map(saved);
        }

        public async Task<InvoiceListResponse> GetInvoicesForAdminAsync(long sellerId, InvoiceFilterRequest filter)
        {
            var seller = await _sellerRepository.GetByIdAsync(sellerId)
                ?? throw new KeyNotFoundException(SystemMessage.TdsCertificateSellerNotFound);

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
                Items = items.Select(i => new InvoiceListItemResponse
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    ServiceType = i.ServiceType.ToString(),
                    InvoiceDate = i.InvoiceDate,
                    InvoicePeriod = $"{i.PeriodFrom:dd MMM yyyy} - {i.PeriodTo:dd MMM yyyy}",
                    InvoiceAmount = i.Amount,
                    PeriodFrom = i.PeriodFrom,
                    PeriodTo = i.PeriodTo
                }).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<(byte[] Content, string FileName)> DownloadCsvForSellerAsync(string userId, long creditNoteId)
        {
            await _sellerContextService.EnsurePermissionAsync(
            userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            var entity = await _repository.GetByIdAsync(creditNoteId, includeInvoice: true)
                ?? throw new KeyNotFoundException(SystemMessage.CreditNoteNotFound);
            if (entity.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);
            return BuildCsv(entity);
        }

        public async Task<CreditNoteListResponse> GetForAdminAsync(CreditNoteFilterRequest filter)
        {
            return await QueryAsync(filter.SellerId, filter);
        }

        public async Task<CreditNoteListResponse> GetForSellerAsync(string userId, CreditNoteFilterRequest filter)
        {
            await _sellerContextService.EnsurePermissionAsync(
                userId, EmployeePermissionCode.WalletViewBalance);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            return await QueryAsync(seller.SellerId, filter);
        }


        private async Task<CreditNoteListResponse> QueryAsync(
        long? sellerId,
          CreditNoteFilterRequest filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);
            CreditNoteStatus? status = null;
            if (!string.IsNullOrWhiteSpace(filter.Status)
                && !filter.Status.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                status = ParseStatus(filter.Status);
            }
            var (items, total) = await _repository.GetFilteredAsync(
                sellerId,
                filter.FromDate,
                filter.ToDate,
                status,
                page,
                pageSize);
            return new CreditNoteListResponse
            {
                Items = items.Select(Map).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
        private static (byte[] Content, string FileName) BuildCsv(CreditNote entity)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                "CreditNoteNumber,InvoiceNumber,ReasonType,CreditNoteDate,Period,Amount,Status,Remark");
            sb.Append(Csv(entity.CreditNoteNumber)).Append(',');
            sb.Append(Csv(entity.Invoice?.InvoiceNumber)).Append(',');
            sb.Append(Csv(ReasonName(entity.ReasonType))).Append(',');
            sb.Append(Csv(entity.CreditNoteDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))).Append(',');
            sb.Append(Csv(entity.Period)).Append(',');
            sb.Append(Csv(entity.Amount.ToString(CultureInfo.InvariantCulture))).Append(',');
            sb.Append(Csv(StatusName(entity.Status))).Append(',');
            sb.AppendLine(Csv(entity.Remark));
            return (Encoding.UTF8.GetBytes(sb.ToString()), $"{entity.CreditNoteNumber}.csv");
        }
        private static CreditNoteListItemResponse Map(CreditNote e) => new()
        {
            CreditNoteId = e.CreditNoteId,
            SellerId = e.SellerId,
            SellerName = e.Seller?.Company?.Name?.Trim()
         ?? $"Seller #{e.SellerId}",
            InvoiceId = e.InvoiceId,
            InvoiceNumber = e.Invoice?.InvoiceNumber,
            CreditNoteNumber = e.CreditNoteNumber,
            ServiceType = e.Invoice?.ServiceType ?? ReasonName(e.ReasonType),
            CreditNoteDate = e.CreditNoteDate,
            Period = e.Period,
            Amount = e.Amount,
            Status = (int)e.Status,
            StatusName = StatusName(e.Status),
            ReasonType = (int)e.ReasonType,
            ReasonTypeName = ReasonName(e.ReasonType),
            Remark = e.Remark
        };
        private static CreditNoteStatus ParseStatus(string status) => status.ToLowerInvariant() switch
        {
            "issued" or "1" => CreditNoteStatus.Issued,
            "applied" or "2" => CreditNoteStatus.Applied,
            "cancelled" or "3" => CreditNoteStatus.Cancelled,
            _ => throw new InvalidOperationException(SystemMessage.CreditNoteStatusInvalid)
        };
        private static string StatusName(CreditNoteStatus s) => s switch
        {
            CreditNoteStatus.Applied => "Applied",
            CreditNoteStatus.Cancelled => "Cancelled",
            _ => "Issued"
        };
        private static string ReasonName(CreditNoteReasonType r) => r switch
        {
            CreditNoteReasonType.InvoiceCorrection => "Invoice Correction",
            CreditNoteReasonType.RemittanceAdjustment => "Remittance Adjustment",
            CreditNoteReasonType.WeightDispute => "Weight Dispute",
            _ => "Other"
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

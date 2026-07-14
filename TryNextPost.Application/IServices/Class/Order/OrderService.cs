using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Order;
using TryNextPost.Application.IServices.Interface.IOrder;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Order
{
    public class OrderService : IOrderService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderService(ISellerRepository sellerRepository, IOrderRepository orderRepository)
        {
            _sellerRepository = sellerRepository;
            _orderRepository = orderRepository;
        }

        public async Task CancelOrderAsync(long orderId, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.IsActive == false)
                throw new InvalidOperationException(string.Format(SystemMessage.OrderNotFound));

            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null || order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.Unauthorized));

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(string.Format(SystemMessage.OrderCannotBeCancelled));

            


            order.IsActive = false;
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId;

            await _orderRepository.UpdateAsync(order);
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    item.IsActive = false;
                }
            }
            await _orderRepository.SaveChangesAsync();
        }

        public async Task<long> CreateForwardOrderAsync(CreateForwardOrderRequest request, string userId)
        {
            return await CreateOrderInternalAsync(request, userId, OrderTypeEnum.Forward, "");
        }

        public async Task<long> CreateReverseOrderAsync(CreateForwardOrderRequest request, string userId)
        {
            return await CreateOrderInternalAsync(request, userId, OrderTypeEnum.Reverse, "R-");
        }

        public async Task<long> CreateReverseQCOrderAsync(CreateForwardOrderRequest request, string userId)
        {
            return await CreateOrderInternalAsync(request, userId, OrderTypeEnum.ReverseQC, "QC-");
        }

        private async Task<long> CreateOrderInternalAsync(CreateForwardOrderRequest request, string userId,
        OrderTypeEnum orderType,
        string orderRefPrefix)
        {
            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null)
                throw new InvalidOperationException(SystemMessage.SellerNotFound);


            var orderRef = string.IsNullOrEmpty(request.OrderRef)
                ? orderRefPrefix + OrderService.GenerateOrderRef()
                : request.OrderRef;

            var volumetricWeight = (request.LengthCm * request.BreadthCm * request.HeightCm) / 5000 * 1000;
            var totalAmount = request.Items.Sum(i => i.Qty * i.Price);
            var finalPayableAmount = totalAmount - request.Discount + request.ShippingCharges + request.CodCharges + request.TaxAmount;

            if (request.IsCollectableAmountDifferent && request.CollectableAmount.HasValue)
                finalPayableAmount = request.CollectableAmount.Value;

            var order = new TryNextPost.Domain.Entities.Order
            {
                SellerId = seller.SellerId,
                OrderRef = orderRef,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                FinalPayableAmount = finalPayableAmount,

                OrderCategory = OrderCategoryEnum.B2C,
                PaymentMode = (PaymentMode)request.PaymentMode,
                OrderType = orderType,                         
                Status = OrderStatus.Pending,

                BillingAddressId = request.BillingAddressId,
                GstNumber = request.GstNumber,

                CustomerName = request.CustomerName,
                CustomerCompanyName = request.CustomerCompanyName,
                CustomerMobile = request.CustomerMobile,
                ShippingAddressLine1 = request.ShippingAddressLine1,
                ShippingAddressLine2 = request.ShippingAddressLine2,
                ShippingCity = request.ShippingCity,
                ShippingState = request.ShippingState,
                ShippingPincode = request.ShippingPincode,
                ShippingCountry = request.ShippingCountry,

                WeightGrams = request.WeightGrams,
                LengthCm = request.LengthCm,
                BreadthCm = request.BreadthCm,
                HeightCm = request.HeightCm,
                VolumetricWeightGrams = volumetricWeight,

                ShippingCharges = request.ShippingCharges,
                CodCharges = request.CodCharges,
                TaxAmount = request.TaxAmount,
                Discount = request.Discount,
                IsCollectableAmountDifferent = request.IsCollectableAmountDifferent,
                CollectableAmount = request.CollectableAmount,

                OrderItems = request.Items.Select(i => new OrderItem
                {
                    ProductName = i.ProductName,
                    Qty = i.Qty,
                    Price = i.Price,
                    Sku = i.Sku
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order.OrderId;
        }

        private static string GenerateOrderRef()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        }
        public async Task<List<OrderDetailResponse>> GetMyOrdersAsync(string userId)
        {
            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null)
                throw new InvalidOperationException(string.Format(SystemMessage.SellerNotFound));

            var order = await _orderRepository.GetBySellerIdAsync(seller.SellerId);

            return order.Select(MapToResponse).ToList();
        }

        public async Task<OrderDetailResponse> GetOrderByIdAsync(long OrderId, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(OrderId);
            if (order == null)
                throw new InvalidOperationException(string.Format(SystemMessage.OrderNotFound));

            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null || order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.Unauthorized));
            
            return MapToResponse(order);

        }

        private OrderDetailResponse MapToResponse(TryNextPost.Domain.Entities.Order order)
        {
            return new OrderDetailResponse
            {
                OrderId = order.OrderId,
                OrderRef = order.OrderRef,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                FinalPayableAmount = order.FinalPayableAmount,

                PaymentMode = (int)order.PaymentMode,
                OrderType = (int)order.OrderType,
                OrderCategory = (int)order.OrderCategory,
                Status = (int)order.Status,

                BillingAddressId = order.BillingAddressId,
                GstNumber = order.GstNumber,

                CustomerName = order.CustomerName,
                CustomerCompanyName = order.CustomerCompanyName,
                CustomerMobile = order.CustomerMobile,
                ShippingAddressLine1 = order.ShippingAddressLine1,
                ShippingAddressLine2 = order.ShippingAddressLine2,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingPincode = order.ShippingPincode,
                ShippingCountry = order.ShippingCountry,

                WeightGrams = order.WeightGrams,
                LengthCm = order.LengthCm,
                BreadthCm = order.BreadthCm,
                HeightCm = order.HeightCm,
                VolumetricWeightGrams = order.VolumetricWeightGrams,

                ShippingCharges = order.ShippingCharges,
                CodCharges = order.CodCharges,
                TaxAmount = order.TaxAmount,
                Discount = order.Discount,
                IsCollectableAmountDifferent = order.IsCollectableAmountDifferent,
                CollectableAmount = order.CollectableAmount,

                Items = order.OrderItems?.Select(i => new OrderItemDto
                {
                    ProductName = i.ProductName,
                    Qty = i.Qty,
                    Price = i.Price,
                    Sku = i.Sku
                }).ToList() ?? new List<OrderItemDto>()
            };
        }

        public async Task UpdateOrderAsync(long orderId, UpdateForwardOrderRequest request, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.IsActive == false)
                throw new InvalidOperationException(string.Format(SystemMessage.OrderNotFound));

            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null || order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.Unauthorized));

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(string.Format(SystemMessage.OrderCannotBeEdited));

            var volumetricWeight = (request.LengthCm * request.BreadthCm * request.HeightCm) / 5000 * 1000;
            var totalAmount = request.Items.Sum(i => i.Qty * i.Price);
            var finalPayableAmount = totalAmount - request.Discount + request.ShippingCharges + request.CodCharges + request.TaxAmount;

            if (request.IsCollectableAmountDifferent && request.CollectableAmount.HasValue)
                finalPayableAmount = request.CollectableAmount.Value;

            order.PaymentMode = (PaymentMode)request.PaymentMode;
            order.BillingAddressId = request.BillingAddressId;
            order.GstNumber = request.GstNumber;
            order.CustomerName = request.CustomerName;
            order.CustomerCompanyName = request.CustomerCompanyName;
            order.CustomerMobile = request.CustomerMobile;
            order.ShippingAddressLine1 = request.ShippingAddressLine1;
            order.ShippingAddressLine2 = request.ShippingAddressLine2;
            order.ShippingCity = request.ShippingCity;
            order.ShippingState = request.ShippingState;
            order.ShippingPincode = request.ShippingPincode;
            order.ShippingCountry = request.ShippingCountry;
            order.WeightGrams = request.WeightGrams;
            order.LengthCm = request.LengthCm;
            order.BreadthCm = request.BreadthCm;
            order.HeightCm = request.HeightCm;
            order.VolumetricWeightGrams = volumetricWeight;
            order.ShippingCharges = request.ShippingCharges;
            order.CodCharges = request.CodCharges;
            order.TaxAmount = request.TaxAmount;
            order.Discount = request.Discount;
            order.IsCollectableAmountDifferent = request.IsCollectableAmountDifferent;
            order.CollectableAmount = request.CollectableAmount;
            order.TotalAmount = totalAmount;
            order.FinalPayableAmount = finalPayableAmount;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId;

            order.OrderItems.Clear();
            order.OrderItems = request.Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                Qty = i.Qty,
                Price = i.Price,
                Sku = i.Sku
            }).ToList();

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
        }


    }
}

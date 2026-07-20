using Azure.Core;
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
        private readonly IAddressRepository _addressRepository;
        private readonly IShipmentRepository _shipmentRepository;

        public OrderService(
            ISellerRepository sellerRepository,
            IOrderRepository orderRepository,
            IAddressRepository addressRepository,
            IShipmentRepository shipmentRepository)
        {
            _sellerRepository = sellerRepository;
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
            _shipmentRepository = shipmentRepository;
        }

        public async Task CancelOrderAsync(long orderId, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.IsActive == false)
                throw new InvalidOperationException(SystemMessage.OrderNotFound);

            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null || order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(SystemMessage.OrderCannotBeCancelled);

            


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

        //public async Task<long> CreateReverseOrderAsync(CreateForwardOrderRequest request, string userId)
        //{
        //    return await CreateOrderInternalAsync(request, userId, OrderTypeEnum.Reverse, "R-");
        //}

        //public async Task<long> CreateReverseQCOrderAsync(CreateForwardOrderRequest request, string userId)
        //{
        //    return await CreateOrderInternalAsync(request, userId, OrderTypeEnum.ReverseQC, "QC-");
        //}

        private async Task<long> CreateOrderInternalAsync(CreateOrderRequestBase request, string userId,
        OrderTypeEnum orderType,
        string orderRefPrefix)
        {
            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null)
                throw new InvalidOperationException(SystemMessage.SellerNotFound);


            var orderRef = string.IsNullOrEmpty(request.OrderRef)
                ? orderRefPrefix + OrderService.GenerateOrderRef()
                : request.OrderRef;

            var existingOrder = await _orderRepository.GetByOrderRefAsync(orderRef);
            if (existingOrder != null)
                throw new InvalidOperationException(String.Format(SystemMessage.IsOrderRefExist, orderRef));

            var volumetricWeight = (request.LengthCm * request.BreadthCm * request.HeightCm) / 5000 * 1000;
            var totalAmount = request.Items.Sum(i => i.Qty * i.Price);
            var finalPayableAmount = totalAmount - request.Discount + request.ShippingCharges + request.CodCharges + request.TaxAmount;

            if (request.IsCollectableAmountDifferent && request.CollectableAmount.HasValue)
                finalPayableAmount = request.CollectableAmount.Value;

            var pickupAddressId = request.PickupAddressId ?? seller.DefaultPickupAddressId;

            if (orderType == OrderTypeEnum.Forward)
            {
                var isValidPickup = await _addressRepository.IsPickupAddressValidAsync(pickupAddressId.Value, seller.UserId);

                if (!isValidPickup)
                {
                    throw new Exception(SystemMessage.IsValidAddress);
                }
            }
            var order = new TryNextPost.Domain.Entities.Order
            {
                SellerId = seller.SellerId,
                OrderRef = orderRef,
                OrderDate = DateTime.UtcNow,
                Channel = "Manual",
                TotalAmount = totalAmount,
                FinalPayableAmount = finalPayableAmount,

                OrderCategory = OrderCategoryEnum.B2C,
                PaymentMode = (PaymentMode)request.PaymentMode,
                OrderType = orderType,                         
                Status = OrderStatus.Pending,

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
                PickupAddressId = pickupAddressId,

                // Billing — agar "Same as Shipping" hai to Shipping se copy karo
                IsBillingSameAsShipping = request.IsBillingSameAsShipping,
                BillingFirstName = request.IsBillingSameAsShipping ? request.CustomerName : request.BillingFirstName,
                BillingLastName = request.IsBillingSameAsShipping ? null : request.BillingLastName,
                BillingCompanyName = request.IsBillingSameAsShipping ? request.CustomerCompanyName : request.BillingCompanyName,
                BillingAddressLine1 = request.IsBillingSameAsShipping ? request.ShippingAddressLine1 : request.BillingAddressLine1,
                BillingAddressLine2 = request.IsBillingSameAsShipping ? request.ShippingAddressLine2 : request.BillingAddressLine2,
                BillingCity = request.IsBillingSameAsShipping ? request.ShippingCity : request.BillingCity,
                BillingState = request.IsBillingSameAsShipping ? request.ShippingState : request.BillingState,
                BillingPincode = request.IsBillingSameAsShipping ? request.ShippingPincode : request.BillingPincode,
                BillingCountry = request.IsBillingSameAsShipping ? request.ShippingCountry : request.BillingCountry,

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

            if (orderType == OrderTypeEnum.ReverseQC)
            {
                var qcRequest = (CreateReverseQcOrderRequest)request;
                order.ReverseQcDetail = new ReverseQcDetail
                {
                    ProductCategory = qcRequest.ProductCategory.Trim(),
                    IsUsedProduct = qcRequest.IsUsedProduct,
                    IsDamagedProduct = qcRequest.IsDamagedProduct,
                    IsBrandMatched = qcRequest.IsBrandMatched,
                    IsSizeMatched = qcRequest.IsSizeMatched,
                    IsColorMatched = qcRequest.IsColorMatched,
                    Images = qcRequest.ReferenceImageUrls
                        .Select((url, index) => new ReverseQcImage
                        {
                            ImageUrl = url.Trim(),
                            DisplayOrder = index + 1
                        })
                        .ToList()
                };
            }

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order.OrderId;
        }

        private static string GenerateOrderRef()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        }

        public async Task<OrderDetailResponse> GetOrderByIdAsync(long OrderId, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(OrderId);
            if (order == null)
                throw new InvalidOperationException(string.Format(SystemMessage.OrderNotFound));

            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null || order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.Unauthorized));

            var activeShipments = await _shipmentRepository.GetActiveShipmentsByOrderIdsAsync(
                new[] { order.OrderId });
            activeShipments.TryGetValue(order.OrderId, out var activeShipment);

            return MapToResponse(order, activeShipment);
        }

        private OrderDetailResponse MapToResponse(
            TryNextPost.Domain.Entities.Order order,
            Domain.Entities.Shipment? activeShipment)
        {
            var hasShipment = activeShipment != null;
            var canShip = order.Status == OrderStatus.Pending && !hasShipment;

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
                StatusName = order.Status.ToString(),
                CanShip = canShip,
                HasShipment = hasShipment,
                ShipmentId = activeShipment?.ShipmentId,
                AwbNumber = activeShipment?.AwbNumber,
                ShipmentStatus = activeShipment?.Status.ToString(),
                CourierName = activeShipment?.Courier?.CourierName,
           
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

                PickupAddressId =order.PickupAddressId,
                IsBillingSameAsShipping = order.IsBillingSameAsShipping,
                BillingFirstName = order.BillingFirstName,
                BillingLastName = order.BillingLastName,
                BillingCompanyName = order.BillingCompanyName,
                BillingAddressLine1 = order.BillingAddressLine1,
                BillingAddressLine2 = order.BillingAddressLine2,
                BillingCity = order.BillingCity,
                BillingState = order.BillingState,
                BillingPincode = order.BillingPincode,
                BillingCountry = order.BillingCountry,

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
                }).ToList(),
                ReverseQcDetail = order.ReverseQcDetail == null
                ? null
                 : new ReverseQcDetailResponse
        {
        ProductCategory = order.ReverseQcDetail.ProductCategory,
        IsUsedProduct = order.ReverseQcDetail.IsUsedProduct,
        IsDamagedProduct = order.ReverseQcDetail.IsDamagedProduct,
        IsBrandMatched = order.ReverseQcDetail.IsBrandMatched,
        IsSizeMatched = order.ReverseQcDetail.IsSizeMatched,
        IsColorMatched = order.ReverseQcDetail.IsColorMatched,
        ReferenceImageUrls = order.ReverseQcDetail.Images
            .OrderBy(image => image.DisplayOrder)
            .Select(image => image.ImageUrl)
            .ToList()
    }
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
            order.PickupAddressId = request.PickupAddressId;
            order.IsBillingSameAsShipping = request.IsBillingSameAsShipping;
            order.BillingFirstName = request.BillingFirstName;
            order.BillingLastName = request.BillingLastName;
            order.BillingCompanyName = request.BillingCompanyName;
            order.BillingAddressLine1 = request.BillingAddressLine1;
            order.BillingAddressLine2 = request.BillingAddressLine2;
            order.BillingCity = request.BillingCity;
            order.BillingState = request.BillingState;
            order.BillingPincode = request.BillingPincode;
            order.BillingCountry = request.BillingCountry;
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

        public async Task<OrderListResponse> GetAllOrdersAsync(string userId, OrderFilterRequest request)
        {
            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null)
                throw new InvalidOperationException(SystemMessage.SellerNotFound);

            OrderStatus? statusFilter = request.Tab?.ToLower() switch
            {
                "not-shipped" => OrderStatus.Pending,
                "booked" => OrderStatus.Confirmed,
                "cancelled" => OrderStatus.Cancelled,
                "fulfilled" => OrderStatus.Delivered,
                _ => null
            };

            var criteria = new OrderFilterCriteria
            {
                Page = request.Page,
                PageSize = request.PageSize,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                OrderIds = request.OrderIds,
                SearchQuery = request.SearchQuery,
                ProductName = request.ProductName,
                Channel = request.Channel,
                Type = request.Type,
                IvrStatus = request.IvrStatus,
                WhatsAppStatus = request.WhatsAppStatus,
                Tags = request.Tags
            };

            var orders = await _orderRepository.GetOrdersFilteredAsync(seller.SellerId, criteria, statusFilter);
            var totalCount = await _orderRepository.GetOrdersFilteredCountAsync(seller.SellerId, criteria, statusFilter);

            var activeShipments = await _shipmentRepository.GetActiveShipmentsByOrderIdsAsync(
                orders.Select(o => o.OrderId));

            var tabCounts = new OrderTabCounts
            {
                AllOrders = await _orderRepository.GetOrdersCountAsync(seller.SellerId, null),
                NotShipped = await _orderRepository.GetOrdersCountAsync(seller.SellerId, OrderStatus.Pending),
                Booked = await _orderRepository.GetOrdersCountAsync(seller.SellerId, OrderStatus.Confirmed),
                Cancelled = await _orderRepository.GetOrdersCountAsync(seller.SellerId, OrderStatus.Cancelled),
                FulfilledOrders = await _orderRepository.GetOrdersCountAsync(seller.SellerId, OrderStatus.Delivered)
            };

            return new OrderListResponse
            {
                Orders = orders
                    .Select(o =>
                    {
                        activeShipments.TryGetValue(o.OrderId, out var shipment);
                        return MapToListItem(o, shipment);
                    })
                    .ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TabCounts = tabCounts
            };
        }

        private static OrderListItemResponse MapToListItem(
            TryNextPost.Domain.Entities.Order order,
            Domain.Entities.Shipment? activeShipment)
        {
            var productSummary = order.OrderItems != null && order.OrderItems.Any()
                ? order.OrderItems.Count == 1
                    ? order.OrderItems.First().ProductName
                    : $"{order.OrderItems.First().ProductName} +{order.OrderItems.Count - 1} more"
                : "N/A";

            var hasShipment = activeShipment != null;
            var canShip = order.Status == OrderStatus.Pending && !hasShipment;

            return new OrderListItemResponse
            {
                OrderId = order.OrderId,
                Channel = order.Channel ?? "Manual",
                OrderRef = order.OrderRef,
                OrderDate = order.OrderDate,
                ProductSummary = productSummary,
                PaymentMode = order.PaymentMode.ToString(),
                CustomerName = order.CustomerName,
                CustomerMobile = order.CustomerMobile,
                WeightGrams = order.WeightGrams,
                IvrStatus = order.IvrStatus,
                WhatsAppStatus = order.WhatsAppStatus,
                ShopifyTags = order.ShopifyTags,
                Tags = order.Tags,
                Status = order.Status.ToString(),
                StatusCode = (int)order.Status,
                CanShip = canShip,
                HasShipment = hasShipment,
                ShipmentId = activeShipment?.ShipmentId,
                AwbNumber = activeShipment?.AwbNumber,
                ShipmentStatus = activeShipment?.Status.ToString(),
                CourierName = activeShipment?.Courier?.CourierName
            };
        }

        public Task<long> CreateReverseOrderAsync(CreateReverseOrderRequest request, string userId)
        {
            return CreateOrderInternalAsync(request, userId, OrderTypeEnum.Reverse, "R-");

        }

        public Task<long> CreateReverseQCOrderAsync(CreateReverseQcOrderRequest request, string userId)
        {
            return CreateOrderInternalAsync(request, userId, OrderTypeEnum.ReverseQC, "QC-");
        }
    }
}

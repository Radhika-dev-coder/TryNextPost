using System.ComponentModel.DataAnnotations;

namespace TryNextPost.Application.DTO.Order
{
    public class CreateB2BOrderRequest : CreateOrderRequestBase
    {
        [Required]
        public long BillingAddressId { get; set; }
    }
}

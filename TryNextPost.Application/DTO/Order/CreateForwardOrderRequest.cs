using System.ComponentModel.DataAnnotations;

namespace TryNextPost.Application.DTO.Order
{
    public class CreateForwardOrderRequest : CreateOrderRequestBase
    {
        [Required]
        public long BillingAddressId { get; set; }
    }
}

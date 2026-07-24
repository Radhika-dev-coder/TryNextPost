using System.Text.Json.Serialization;

namespace TryNextPost.Application.DTO.Weight
{
    public class WeightDiscrepancyListItemResponse
    {
        public long WeightDiscrepancyId { get; set; }

        /// <summary>Owning seller — required for SuperAdmin multi-seller table.</summary>
        public long SellerId { get; set; }

        /// <summary>Company / business name when available; empty for seller-scoped views is fine.</summary>
        public string SellerName { get; set; } = string.Empty;

        public DateTime WeightAppliedDate { get; set; }
        public string? AwbNumber { get; set; }
        public long? OrderId { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public decimal EnteredWeightGrams { get; set; }
        public decimal AppliedWeightGrams { get; set; }
        public decimal WeightCharges { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? CourierName { get; set; }
        public long? CourierId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;

        /// <summary>Seller remarks when they raised the dispute.</summary>
        [JsonPropertyName("disputeRemarks")]
        public string? DisputeRemarks { get; set; }

        /// <summary>SuperAdmin remarks when the dispute was closed (DB column ClosedRemarks).</summary>
        [JsonPropertyName("closedRemarks")]
        public string? ClosedRemarks { get; set; }
    }
}

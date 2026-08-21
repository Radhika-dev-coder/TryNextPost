using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public class DtdcBookingData
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("reference_number")] public string? ReferenceNumber { get; set; }
        [JsonPropertyName("courier_partner")] public string? CourierPartner { get; set; }
        [JsonPropertyName("courier_account")] public string? CourierAccount { get; set; }
        [JsonPropertyName("courier_partner_reference_number")] public string? CourierPartnerReferenceNumber { get; set; }
        [JsonPropertyName("customer_reference_number")] public string? CustomerReferenceNumber { get; set; }

        // FIXED: Allows .NET to safely read "0.50" string as a valid decimal number
        [JsonPropertyName("chargeable_weight")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal? ChargeableWeight { get; set; }

        [JsonPropertyName("self_pickup_enabled")] public bool? SelfPickupEnabled { get; set; }
        [JsonPropertyName("pieces")] public List<DtdcBookingPiece>? Pieces { get; set; }
        [JsonPropertyName("barCodeData")] public string? BarCodeData { get; set; }
        [JsonPropertyName("error_message")] public string? ErrorMessage { get; set; }
        [JsonPropertyName("error_desc")] public string? ErrorDesc { get; set; }
    }
}

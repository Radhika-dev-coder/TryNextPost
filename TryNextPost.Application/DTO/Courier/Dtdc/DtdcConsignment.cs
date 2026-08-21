using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcConsignment
    {
        [JsonPropertyName("customer_code")] public string? CustomerCode { get; set; }
        [JsonPropertyName("service_type_id")] public string? ServiceTypeId { get; set; }
        [JsonPropertyName("load_type")] public string? LoadType { get; set; }
        [JsonPropertyName("consignment_type")] public string? ConsignmentType { get; set; }
        [JsonPropertyName("dimension_unit")] public string? DimensionUnit { get; set; }
        [JsonPropertyName("length")] public string? Length { get; set; }
        [JsonPropertyName("width")] public string? Width { get; set; }
        [JsonPropertyName("height")] public string? Height { get; set; }
        [JsonPropertyName("weight_unit")] public string? WeightUnit { get; set; }
        [JsonPropertyName("weight")] public string? Weight { get; set; }
        [JsonPropertyName("num_pieces")] public string? NumPieces { get; set; }
        [JsonPropertyName("customer_reference_number")] public string? CustomerReferenceNumber { get; set; }
        [JsonPropertyName("declared_value")] public string? DeclaredValue { get; set; }
        [JsonPropertyName("is_risk_surcharge_applicable")] public string? IsRiskSurchargeApplicable { get; set; }
        [JsonPropertyName("commodity_id")] public string? CommodityId { get; set; }
        [JsonPropertyName("cod_collection_mode")] public string? CodCollectionMode { get; set; }
        [JsonPropertyName("cod_amount")] public string? CodAmount { get; set; }

        [JsonPropertyName("origin_details")] public DtdcOriginDetails OriginDetails { get; set; } = new();
        [JsonPropertyName("destination_details")] public DtdcDestinationDetails DestinationDetails { get; set; } = new();

        // Added multi piece optional block support as per Page 8 spec sheets
        [JsonPropertyName("pieces_detail")] public List<DtdcPiecesDetail>? PiecesDetail { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    /// <summary>
    /// Root response returned by Amazon Shipping Purchase Shipment API.
    /// </summary>
    public class AmazonPurchaseShipmentApiResponse
    {
        [JsonPropertyName("payload")]
        public AmazonPurchaseShipmentResponse? Payload { get; set; }
    }

    /// <summary>
    /// Amazon purchased shipment details.
    /// </summary>
    public class AmazonPurchaseShipmentResponse
    {
        /// <summary>
        /// Amazon's unique shipment identifier.
        /// </summary>
        [JsonPropertyName("shipmentId")]
        public string? ShipmentId { get; set; }

        /// <summary>
        /// Package-level documents and tracking information.
        /// </summary>
        [JsonPropertyName("packageDocumentDetails")]
        public List<AmazonPackageDocumentDetail>? PackageDocumentDetails { get; set; }

        /// <summary>
        /// Pickup and delivery promise windows.
        /// </summary>
        [JsonPropertyName("promise")]
        public AmazonPromise? Promise { get; set; }

        /// <summary>
        /// Base shipment charge.
        /// </summary>
        [JsonPropertyName("totalCharge")]
        public AmazonCharge? TotalCharge { get; set; }

        /// <summary>
        /// Shipment charge after adjustments.
        /// </summary>
        [JsonPropertyName("totalChargeWithAdjustments")]
        public AmazonCharge? TotalChargeWithAdjustments { get; set; }

        /// <summary>
        /// Payment type used for the shipment.
        /// </summary>
        [JsonPropertyName("paymentType")]
        public string? PaymentType { get; set; }

        /// <summary>
        /// Benefits associated with the shipment.
        /// </summary>
        [JsonPropertyName("benefits")]
        public List<AmazonBenefit>? Benefits { get; set; }
    }

    /// <summary>
    /// Package-level tracking and shipping documents.
    /// </summary>
    public class AmazonPackageDocumentDetail
    {
        /// <summary>
        /// Client reference ID supplied while creating the shipment.
        /// </summary>
        [JsonPropertyName("packageClientReferenceId")]
        public string? PackageClientReferenceId { get; set; }

        /// <summary>
        /// Shipping documents such as labels.
        /// </summary>
        [JsonPropertyName("packageDocuments")]
        public List<AmazonDocument>? PackageDocuments { get; set; }

        /// <summary>
        /// Carrier tracking number.
        /// </summary>
        [JsonPropertyName("trackingId")]
        public string? TrackingId { get; set; }

        /// <summary>
        /// Amazon/carrier identifier, when provided.
        /// </summary>
        [JsonPropertyName("carrierId")]
        public string? CarrierId { get; set; }

        /// <summary>
        /// Additional package label attributes.
        /// </summary>
        [JsonPropertyName("packageLabelAttributes")]
        public AmazonPackageLabelAttributes? PackageLabelAttributes { get; set; }
    }

    /// <summary>
    /// Shipping document returned by Amazon.
    /// </summary>
    public class AmazonDocument
    {
        /// <summary>
        /// Document type, for example LABEL.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Document format, for example PNG or PDF.
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        /// <summary>
        /// Base64 encoded document contents.
        /// </summary>
        [JsonPropertyName("contents")]
        public string? Contents { get; set; }

        /// <summary>
        /// Amazon document identifier, when provided.
        /// </summary>
        [JsonPropertyName("documentId")]
        public string? DocumentId { get; set; }

        /// <summary>
        /// Document size, when provided by Amazon.
        /// </summary>
        [JsonPropertyName("size")]
        public long? Size { get; set; }
        [JsonIgnore]
        public byte[]? ContentBytes { get; set; }
    }

    /// <summary>
    /// Pickup and delivery windows promised by Amazon.
    /// </summary>
    public class AmazonPromise
    {
        [JsonPropertyName("pickupWindow")]
        public AmazonTimeWindow? PickupWindow { get; set; }

        [JsonPropertyName("deliveryWindow")]
        public AmazonTimeWindow? DeliveryWindow { get; set; }
    }

    /// <summary>
    /// Represents a start and end time window.
    /// </summary>
    public class AmazonTimeWindow
    {
        [JsonPropertyName("start")]
        public DateTimeOffset? Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset? End { get; set; }
    }

    /// <summary>
    /// Shipment charge information.
    /// </summary>
    public class AmazonCharge
    {
        /// <summary>
        /// Currency unit, for example GBP or INR.
        /// </summary>
        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        /// <summary>
        /// Charge amount.
        /// </summary>
        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    /// <summary>
    /// Amazon shipment benefit information.
    /// </summary>
    public class AmazonBenefit
    {
        [JsonPropertyName("benefitType")]
        public string? BenefitType { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Additional package label attributes.
    /// Kept extensible because Amazon may populate additional attributes.
    /// </summary>
    public class AmazonPackageLabelAttributes
    {
        // Add Amazon-specific fields here when they are returned
        // by the API and required by the application.
    }
}

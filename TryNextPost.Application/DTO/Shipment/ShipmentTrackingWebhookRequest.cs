namespace TryNextPost.Application.DTO.Shipment
{
    /// <summary>
    /// Courier / aggregator tracking webhook payload (skeleton — extend per courier contract).
    /// </summary>
    public class ShipmentTrackingWebhookRequest
    {
        public string AwbNumber { get; set; } = string.Empty;
        public string? Status { get; set; }
        public int? StatusCode { get; set; }
        public string? CourierStatusCode { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime? EventTime { get; set; }
        public string? CourierCode { get; set; }
    }
}

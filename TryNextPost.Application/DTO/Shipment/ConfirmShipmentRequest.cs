namespace TryNextPost.Application.DTO.Shipment
{
    public class ConfirmShipmentRequest
    {
        public long OrderId { get; set; }

        /// <summary>Preferred: resolve courier from DB by id.</summary>
        public long? CourierId { get; set; }

        /// <summary>Alternative: resolve by code (e.g. DELHIVERY).</summary>
        public string? CourierCode { get; set; }

        /// <summary>Service code from the selected rate option.</summary>
        public string? ServiceCode { get; set; }

        /// <summary>Freight charge from the selected rate (wallet debit amount).</summary>
        public decimal ChargeAmount { get; set; }
    }
}

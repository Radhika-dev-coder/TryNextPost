using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonGetRatesResponse
    {
        public AmazonGetRatesPayload Payload { get; set; } = new();
    }
    public class AmazonGetRatesPayload
    {
        public string RequestToken { get; set; } = string.Empty;
        public List<AmazonRate> Rates { get; set; } = [];
        public List<object> IneligibleRates { get; set; } = [];
    }

    public class AmazonRate
    {
        public string RateId { get; set; } = string.Empty;
        public string CarrierId { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;

        public AmazonMoney TotalCharge { get; set; } = new();

        public AmazonBilledWeight BilledWeight { get; set; } = new();

        public bool RequiresAdditionalInputs { get; set; }
    }

    public class AmazonBilledWeight
    {
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonGetRatesRequest
    {
        public AmazonAddress ShipTo { get; set; } = new();
        public AmazonAddress ShipFrom { get; set; } = new();
        public List<AmazonPackage> Packages { get; set; } = [];
        public AmazonServiceSelection? ServiceSelection { get; set; }
        public AmazonChannelDetails ChannelDetails { get; set; } = new();
    }
    public class AmazonServiceSelection
    {
        public List<string> ServiceId { get; set; } = [];
    }
    public class AmazonAddress
    {
        public string Name { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string StateOrRegion { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string CountryCode { get; set; } = "IN";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class AmazonPackage
    {
        public AmazonDimensions Dimensions { get; set; } = new();
        public AmazonWeight Weight { get; set; } = new();
        public List<AmazonItem> Items { get; set; } = [];
        public AmazonMoney? InsuredValue { get; set; }
        public string PackageClientReferenceId { get; set; } = string.Empty;
    }

    public class AmazonDimensions
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string Unit { get; set; } = "CENTIMETER";
    }

    public class AmazonWeight
    {
        public decimal Value { get; set; }
        public string Unit { get; set; } = "GRAM";
    }

    public class AmazonItem
    {
        public int Quantity { get; set; }
        public string ItemIdentifier { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsHazmat { get; set; }
        public AmazonWeight Weight { get; set; } = new();
    }

    public class AmazonMoney
    {
        public string Unit { get; set; } = "INR";
        public decimal Value { get; set; }
    }

    public class AmazonChannelDetails
    {
        public string ChannelType { get; set; } = "EXTERNAL";
    }
}

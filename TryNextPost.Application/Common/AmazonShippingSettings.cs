using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.Common
{
    public class AmazonShippingSettings
    {
        public string LwaBaseUrl { get; set; } = "https://api.amazon.com";

        public string ShippingBaseUrl { get; set; } =  "https://sandbox.sellingpartnerapi-eu.amazon.com";

        public string ShippingBusinessId { get; set; } =  "AmazonShipping_IN";

        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}

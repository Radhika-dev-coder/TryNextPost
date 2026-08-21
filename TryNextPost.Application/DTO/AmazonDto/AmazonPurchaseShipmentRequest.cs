using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonPurchaseShipmentRequest
    {
        public string RequestToken { get; set; } = default!;

        public string RateId { get; set; } = default!;

        public AmazonRequestedDocumentSpecification  RequestedDocumentSpecification  { get; set; } = default!;
    }
}

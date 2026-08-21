using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonGetLabelResponse
    {
        public AmazonGetLabelPayload Payload { get; set; } = new();
    }

    public class AmazonGetLabelPayload
    {
        public string? LabelUrl { get; set; }

        public string? LabelFormat { get; set; }

        public string? LabelContent { get; set; }
    }
}

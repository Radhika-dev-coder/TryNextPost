using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonRequestedDocumentSpecification
    {
        public string Format { get; set; } = "PNG";

        public AmazonDocumentSize Size { get; set; } = new();

        public int Dpi { get; set; } = 300;

        public string PageLayout { get; set; } = "DEFAULT";

        public bool NeedFileJoining { get; set; } = false;

        public List<string> RequestedDocumentTypes { get; set; } = new();
    }

    public class AmazonDocumentSize
    {
        public decimal Length { get; set; } = 6;

        public string Unit { get; set; } = "INCH";

        public decimal Width { get; set; } = 4;
    }
}

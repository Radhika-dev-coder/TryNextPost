using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class SellerDocument : BaseDbModel
    {

        [Key]
        public int Id { get; set; }


        public string SellerId { get; set; }
        public string? DocumentType { get; set; } = string.Empty;
        public string? FilePath { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? UploadedBy { get; set; } = string.Empty;

        public DateTime? UploadedOn { get; set; }

        public string? VerifiedBy { get; set; }

        public DateTime? VerifiedOn { get; set; }


        public string? Remarks { get; set; }


    }
}

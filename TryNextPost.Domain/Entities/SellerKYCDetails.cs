using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class SellerKYCDetails : BaseDbModel
    {
        [Key]
        public int Id { get; set; }

        public string SellerId { get; set; }
        public string? AadharKYCStatus { get; set; }
        public string? PanKYCStatus { get; set; }    
        public string? BankKYCStatus { get; set; }    
    }
}

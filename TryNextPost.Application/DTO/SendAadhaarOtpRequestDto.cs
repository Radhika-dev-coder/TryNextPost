using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO
{
    public class SendAadhaarOtpRequestDto
    {
        [Required]
        [RegularExpression(@"^[2-9][0-9]{11}$",
        ErrorMessage = "Invalid Aadhaar Number")]
        public string AadhaarNumber { get; set; } = string.Empty;
    }
}

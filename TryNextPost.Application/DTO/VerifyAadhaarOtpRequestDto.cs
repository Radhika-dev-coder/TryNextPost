using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO
{
    public class VerifyAadhaarOtpRequestDto
    {
        [Required]
        [RegularExpression(@"^[2-9][0-9]{11}$")]
        public string AadhaarNumber { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$")]
        public string Otp { get; set; } = string.Empty;
    }
}

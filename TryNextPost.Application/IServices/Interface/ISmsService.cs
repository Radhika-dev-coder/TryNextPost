using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.IServices.Interface
{
    public interface ISmsService
    {
        Task SendOtpSms(String mobile, string otp);
    }
}

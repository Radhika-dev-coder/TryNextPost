using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.IServices.Interface;

namespace TryNextPost.Infrastructure.Service
{
    public class SmsService : ISmsService
    {
        public async Task SendOtpSms(string PhoneNumber, string Otp)
        {
            Console.WriteLine($"[DUMMY SMS] OTP for {PhoneNumber} : {Otp}");
            await Task.CompletedTask;
        }
    }
}

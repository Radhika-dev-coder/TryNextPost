using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.IServices.Interface
{
    public interface IEmailService
    {
        Task SendOtpEmail(string email, string otp);
        Task SendWelcomeEmail(string email, string fullName);
        Task SendEmployeeInviteEmail(string email, string fullName, string temporaryPassword);
    }
}

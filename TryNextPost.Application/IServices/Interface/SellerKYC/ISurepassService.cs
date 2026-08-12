using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.SellerKYC;

namespace TryNextPost.Application.IServices.Interface.SellerKYC
{
    public interface ISurepassService
    {
        Task<PanComprehensiveResponse?> VerifyPanAsync(string panNumber, string userId, CancellationToken cancellationToken = default);
        Task<BankVerificationResponse?> VerifyBankAccountAsync(BankAccountVerificationRequest request, string userId);
    }
}

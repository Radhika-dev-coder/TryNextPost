using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ISellerKycRepository
    {
        Task<SellerKYCDetails?> GetBySellerIdAsync(string sellerId);
        Task<PANKYC?> GetByPanSellerKYCAsync(string sellerId);
        Task<BankKYC?> GetByBankSellerKYCAsync(string sellerId);

        Task AddAsync(SellerKYCDetails sellerKyc);

        Task UpdateAsync(SellerKYCDetails sellerKyc);
        Task UpdateBankKycAsync(BankKYC kyc);

        Task<bool> SaveChangesAsync();
        Task AddPanKycAsync(PANKYC pan);
        Task AddBankKycAsync(BankKYC kyc);
        Task UpdatePanKycAsync(PANKYC pan);

    }
}

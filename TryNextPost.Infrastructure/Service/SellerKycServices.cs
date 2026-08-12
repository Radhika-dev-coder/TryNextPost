using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TryNextPost.Application.DTO;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.Identity;

namespace TryNextPost.Infrastructure.Service
{
    public class SellerKycServices : ISellerKycServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISellerKycRepository _sellerKycRep;
        public SellerKycServices(UserManager<ApplicationUser> userManager, ISellerKycRepository sellerKycRep)
        {
            _userManager = userManager;
            _sellerKycRep = sellerKycRep;
        }

        public async Task<BaseResponse<object>> AddSellerKycAsync(VerifyAadhaarOtpRequestDto dto, string sellerId)
        {
            var response = new BaseResponse<object>();
            try
            {
                if (string.IsNullOrWhiteSpace(dto.AadhaarNumber) || !Regex.IsMatch(dto.AadhaarNumber, @"^\d{12}$"))
                {
                    response.StatusCode = (int)ApiStatusCode.BadRequest;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.AadharInvalid;
                    return response;
                }
                if (string.IsNullOrWhiteSpace(dto.Otp) || !Regex.IsMatch(dto.Otp, @"^\d{6}$"))
                {
                    response.StatusCode = (int)ApiStatusCode.BadRequest;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.InvalidOtp;
                    return response;
                }
                var data = await _userManager.FindByIdAsync(sellerId);

                var otpEntity = await _otpRepository.GetLatestActiveByMobileAsync(data.PhoneNumber);

                if (otpEntity == null || otpEntity.ExpiryTime < DateTime.UtcNow)
                    throw new UnauthorizedAccessException(SystemMessage.InvalidOtp);

                if (otpEntity.ExpiryTime < DateTime.UtcNow)
                    throw new UnauthorizedAccessException(SystemMessage.OtpExpired);

                if (otpEntity.FailedAttempts >= 5)
                    throw new InvalidOperationException(SystemMessage.RequestNewOtp);
                var incomingHash = HashOtp(dto.Otp, data.PhoneNumber);

                if (!CryptographicOperations.FixedTimeEquals(
                        Convert.FromHexString(otpEntity.CodeHash),
                        Convert.FromHexString(incomingHash)))
                {
                    otpEntity.FailedAttempts++;
                    await _otpRepository.SaveChangesAsync();
                    throw new InvalidOperationException(SystemMessage.InvalidOtp);
                }

                otpEntity.IsUsed = true;
                await _otpRepository.SaveChangesAsync();

                
                var data1 = new SellerKYCDetails
                {
                    SellerId = sellerId,
                    //AadharLast4Digit = dto.AadhaarNumber.Substring(dto.AadhaarNumber.Length - 4),
                    //AadharVerified = KycStatus.Pending.ToString(),
                    //KYCStatus = KycStatus.Pending.ToString(),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = sellerId
                };
                await _sellerKycRep.AddAsync(data);
                var isSaved = await _sellerKycRep.SaveChangesAsync();
                if (!isSaved)
                {
                    response.StatusCode = (int)ApiStatusCode.BadRequest;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.SomethingWentWrong;
                    return response;
                }
                response.StatusCode = (int)ApiStatusCode.Success;
                response.Success = true;
                response.Data = null;
                response.Message = SystemMessage.KycVerified;
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)ApiStatusCode.BadRequest;
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<BaseResponse<object>> SendOtpAadharKyc(SendAadhaarOtpRequestDto dto, string sellerId)
        {
            var response = new BaseResponse<object>();
            try
            {
                if (string.IsNullOrWhiteSpace(dto.AadhaarNumber) || !Regex.IsMatch(dto.AadhaarNumber, @"^\d{12}$"))
                {
                    response.StatusCode = (int)ApiStatusCode.BadRequest;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.AadharInvalid;
                    return response;
                }
                var data = await _userManager.FindByIdAsync(sellerId);
                if (data == null)
                {
                    response.StatusCode = (int)ApiStatusCode.NotFound;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.Unauthorized;
                    return response;
                }
                var existing = await _sellerKycRep.GetBySellerIdAsync(sellerId);
                //if (existing != null)
                //{
                //  /  switch (existing.KYCStatus)
                //    {
                //        case nameof(KycStatus.Verified):
                //            response.StatusCode = (int)ApiStatusCode.Conflict;
                //            response.Success = false;
                //            response.Data = null;
                //            response.Message = SystemMessage.AlreadyKycUpdated;
                //            return response;

                //        case nameof(KycStatus.Pending):
                //            response.StatusCode = (int)ApiStatusCode.Conflict;
                //            response.Success = false;
                //            response.Data = null;
                //            response.Message = SystemMessage.KycPending;
                //            return response;

                //        case nameof(KycStatus.Reject):
                //            response.StatusCode = (int)ApiStatusCode.Conflict;
                //            response.Success = false;
                //            response.Data = null;
                //            response.Message = SystemMessage.RejectKyc;
                //            return response;
                //    }
                //}
                var mobileNo = data.PhoneNumber;
                var cacheKey = $"phone_otp_{mobileNo}";
                if (_cache.TryGetValue(cacheKey, out _))
                {
                    response.StatusCode = (int)ApiStatusCode.BadRequest;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.AlreadyOTPSend;
                    return response;
                }


                await _otpRepository.InvalidateActiveOtpsAsync(mobileNo);

                var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                var entity = new Otp
                {
                    MobileNumber = mobileNo,
                    CodeHash = HashOtp(otp, mobileNo),
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false,
                    FailedAttempts = 0
                };

                await _msService.SendOtpSms(mobileNo, otp);
                await _otpRepository.AddAsync(entity);
                await _otpRepository.SaveChangesAsync();
                _cache.Set(cacheKey, true, TimeSpan.FromSeconds(60));
                response.StatusCode = (int)ApiStatusCode.Success;
                response.Success = true;
                response.Data = null;
                response.Message = SystemMessage.AadharOtpSend;
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)ApiStatusCode.BadRequest;
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}

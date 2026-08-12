using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.SellerKYC;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.SellerKYC;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.SellerKYC
{
    public class SurepassService : ISurepassService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ISellerKycRepository _sellerKycRepository;


        public SurepassService(HttpClient httpClient, IConfiguration configuration, ISellerKycRepository sellerKycRepository, IUnitOfWork unitofWork)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _sellerKycRepository = sellerKycRepository;
        }

        public async Task<BankVerificationResponse?> VerifyBankAccountAsync(BankAccountVerificationRequest request, string userId)
        {
            var token = _configuration["Surepass:Token"];
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bank-verification/");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(httpRequest);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Surepass Bank API failed: {responseBody}");
            }
            var res = JsonSerializer.Deserialize<BankVerificationResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (res == null || res.Data == null)
            {
                throw new Exception("Invalid response received from Surepass.");
            }



            var sellerKYC = new SellerKYCDetails
            {
                SellerId = userId,
                BankKYCStatus = KycStatus.Pending.ToString(),
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
            };
            if (res.StatusCode == (int)ApiStatusCode.Success)
            {
                sellerKYC.BankKYCStatus = KycStatus.Verified.ToString();

            }
            var existingData = await _sellerKycRepository.GetBySellerIdAsync(userId);
            if (existingData == null)
            {
                await _sellerKycRepository.AddAsync(sellerKYC);
                var bankKyc = new BankKYC
                {
                    SellerKycId = sellerKYC.Id,
                    AccountHolderName = res.Data.FullName,
                    AccountNumberMasked = res.Data.AccountNumber,
                    IFSC = res.Data.IfscDetails.Ifsc,
                    BankName = res.Data.IfscDetails.BankName,
                    BranchName = res.Data.IfscDetails.Branch,
                    AccountStatus = res.Data.Status,
                    ResponseCode = res.StatusCode.ToString(),
                    Status = res.MessageCode,
                    ProviderName = "SurePass",
                    ClientId = res.Data.ClientId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };
                await _sellerKycRepository.AddBankKycAsync(bankKyc);
            }
            else
            {
                existingData.UpdatedAt = DateTime.Now;
                existingData.UpdatedBy = userId;
                await _sellerKycRepository.UpdateAsync(existingData);
                var data = await _sellerKycRepository.GetByBankSellerKYCAsync(userId);
                if (data == null)
                {
                    var bankKyc = new BankKYC
                    {
                        SellerKycId = existingData.Id,
                        AccountHolderName = res.Data.FullName,
                        AccountNumberMasked = res.Data.AccountNumber,
                        IFSC = res.Data.IfscDetails.Ifsc,
                        BankName = res.Data.IfscDetails.BankName,
                        BranchName = res.Data.IfscDetails.Branch,
                        AccountStatus = res.Data.Status,
                        ResponseCode = res.StatusCode.ToString(),
                        Status = res.MessageCode,
                        ProviderName = "SurePass",
                        ClientId = res.Data.ClientId,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };
                    await _sellerKycRepository.AddBankKycAsync(bankKyc);
                }
                else
                {
                    data.AccountHolderName = res.Data.FullName;
                    data.AccountNumberMasked = res.Data.AccountNumber;
                    data.IFSC = res.Data.IfscDetails.Ifsc;
                    data.BankName = res.Data.IfscDetails.BankName;
                    data.BranchName = res.Data.IfscDetails.Branch;
                    data.AccountStatus = res.Data.Status;
                    data.ResponseCode = res.StatusCode.ToString();
                    data.Status = res.MessageCode;
                    data.ProviderName = "SurePass";
                    data.ClientId = res.Data.ClientId;
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedBy = userId;
                    await _sellerKycRepository.UpdateBankKycAsync(data);
                }
            }

            return new BankVerificationResponse
            {
                Data = res.Data,
                Message = SystemMessage.RecordSave,
                StatusCode = (int)ApiStatusCode.Success,
                Success = true,
                MessageCode = res.MessageCode
            };
        }

        public async Task<PanComprehensiveResponse?> VerifyPanAsync(string panNumber, string userId, CancellationToken cancellationToken = default)
        {

            var token = _configuration["Surepass:Token"];

            var request = new PanComprehensiveRequest
            {
                id_number = panNumber,
                masked_aadhaar_variant = "v1, v2, empty"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pan/pan-comprehensive");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Surepass PAN API failed: {responseBody}");
            }
            var res = JsonSerializer.Deserialize<PanComprehensiveResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (res == null || res.Data == null)
            {
                throw new Exception("Invalid response received from Surepass.");
            }



            var sellerKYC = new SellerKYCDetails
            {
                SellerId = userId,
                PanKYCStatus = KycStatus.Pending.ToString(),
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
            };
            if (res.StatusCode == (int)ApiStatusCode.Success)
            {
                sellerKYC.PanKYCStatus = KycStatus.Verified.ToString();

            }
            var existingData = await _sellerKycRepository.GetBySellerIdAsync(userId);
            if (existingData == null)
            {
                await _sellerKycRepository.AddAsync(sellerKYC);
                var panKyc = new PANKYC
                {
                    SellerKycId = sellerKYC.Id,
                    MaskedAadhar = res.Data.MaskedAadhaar,
                    PanNumber = res.Data.PanNumber,
                    Name = res.Data.FullName,
                    DOB = Convert.ToDateTime(res.Data.Dob),
                    Status = res.Data.MessageCode,
                    ProviderName = "SurePass",
                    ClientId = res.Data.ClientId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };
                await _sellerKycRepository.AddPanKycAsync(panKyc);
            }
            else
            {

                existingData.UpdatedAt = DateTime.Now;
                existingData.UpdatedBy = userId;
                await _sellerKycRepository.UpdateAsync(existingData);
                var data = await _sellerKycRepository.GetByPanSellerKYCAsync(userId);
                if (data == null)
                {
                    var panKyc = new PANKYC
                    {
                        SellerKycId = existingData.Id,
                        MaskedAadhar = res.Data.MaskedAadhaar,
                        PanNumber = res.Data.PanNumber,
                        Name = res.Data.FullName,
                        DOB = Convert.ToDateTime(res.Data.Dob),
                        Status = res.Data.MessageCode,
                        ProviderName = "SurePass",
                        ClientId = res.Data.ClientId,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };
                    await _sellerKycRepository.AddPanKycAsync(panKyc);
                }
                else
                {
                    data.MaskedAadhar = res.Data.MaskedAadhaar;
                    data.PanNumber = res.Data.PanNumber;
                    data.Name = res.Data.FullName;
                    data.DOB = Convert.ToDateTime(res.Data.Dob);
                    data.Status = res.Data.MessageCode;
                    data.ClientId = res.Data.ClientId;
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedBy = userId;
                    await _sellerKycRepository.UpdatePanKycAsync(data);
                }
                // Existing entity ko update karo
                //existingData.PanNumber = entity.PanNumber;
                //existingData.PanHolderName = entity.PanHolderName;
                //existingData.PanVerfied = entity.PanVerfied;
                //existingData.PanVerfiedOn = entity.PanVerfiedOn;
                //existingData.VerificationProvider =
                //    entity.VerificationProvider;
                //existingData.VerificationReferenceId =
                //    entity.VerificationReferenceId;
                //existingData.KYCStatus = entity.KYCStatus;
                //existingData.FailureReason = entity.FailureReason;
            }

            return new PanComprehensiveResponse
            {
                Data = res.Data,
                Message = SystemMessage.RecordSave,
                StatusCode = (int)ApiStatusCode.Success,
                Success = true,
                MessageCode = res.MessageCode
            };


        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Common
{
    public static class SystemMessage
    {
<<<<<<< Updated upstream
        // ───── Auth Module ─────
        public const string RegisterSuccess = "Registration successful.";
        public const string RegisterFailed = "Registration failed.";
        public const string InvalidCredentials = "Invalid email or password.";
        public const string EmailNotFound = "Email not found.";
        public const string PasswordMismatch = "Password and Confirm Password do not match.";
        public const string LoginSuccess = "Login successful.";
        public const string OtpSentEmail = "OTP sent to your email.";
        public const string OtpResentEmail = "OTP resent to your email.";
        public const string OtpSentPhone = "OTP sent to your mobile number.";
        public const string InvalidOtp = "Invalid or expired OTP.";
        public const string InvalidOtpFormat = "Invalid OTP format. Enter a 6-digit number.";
        public const string PasswordResetSuccess = "Password reset successful. Please login with your new password.";
        public const string EmailVerifiedRegistrationRequired = "Email verified. Please complete your registration.";
        public const string PhoneVerifiedRegistrationRequired = "Phone verified. Please complete your registration.";
        public const string UserNotFound = "User not found.";
        public const string OtpWaitMessage = "Please wait {0} seconds before requesting another OTP.";
        public const string VerifiedOtp = "OTP verified successfully. You can now reset your password.";

        // ───── Seller Module ─────
        public const string SellerNotFound = "Seller profile not found. Please complete KYC first.";
        public const string SellerProfileIncomplete = "Please complete your seller profile.";

        // ───── Address Module ─────
        public const string AddressNotFound = "Address not found.";
        public const string AddressAddedSuccess = "Pickup address added successfully.";
        public const string AddressUpdatedSuccess = "Address updated successfully.";
        public const string AddressDeletedSuccess = "Address deleted successfully.";

        // ───── Order Module ─────
        public const string OrderNotFound = "Order not found.";
        public const string OrderCreatedSuccess = "Order created successfully.";
        public const string OrderUpdatedSuccess = "Order updated successfully.";
        public const string OrderFetchedSuccess = "Orders fetched successfully";
        public const string OrderCancelledSuccess = "Order cancelled successfully.";
        public const string OrderCannotBeEdited = "Cannot edit order once it has been shipped or processed.";
        public const string OrderCannotBeCancelled = "Cannot cancel order once it has been shipped or processed.";
        public const string AtLeastOneItemRequired = "At least one item is required.";

        // ───── Common/Generic ─────
        public const string SomethingWentWrong = "Something went wrong. Please try again later.";
        public const string NotFound = "Data not found.";
        public const string RequestBodyNull = "Request body cannot be null.";
        public const string RequiredId = "Id is required.";
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string InvalidToken = "Invalid or missing authentication token.";
        public const string ValidationFailed = "Validation failed. Please check your input.";
=======
        public static string RegisterSuccess = "Registration Successfull...";
        public static string RegisterFaield = "Registration Faield...";
        public static string SomethingWentWrong = "Something went Wrong...";
        public static string NotFound = "Data Not Found...";
        public static string RequestBodyNull = "Request Body Null...";
        public static string RequiredId = "Id is Required...";
        public static string AadharInvalid = "Aadhar number is Invalid..";
        public static string DataFound = "Data Found Succesfully..";
        public static string UnAuthorized = "You Are UnAuthorized..";
        public static string AlreadyKycUpdated = "KYC Already Verifeid..";
        public static string RejectKyc = "You Kyc is Rejected. Please Contact Adminstration..";
        public static string KycNotFound = "Seller KYC record not found..";
        public static string KycVerified = "Seller KYC has been verified successfully..";
        public static string KycPending = "Seller KYC verification is pending..";
        public static string AadharOtpSend = "Aadhaar KYC OTP sent successfully..";
        public static string AadharkycSubmitted = "Successfully Kyc Submitted..";
        public static string InvalidOtp = "Please Enter Valid OTP..";

>>>>>>> Stashed changes
    }
}

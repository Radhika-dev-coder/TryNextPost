using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Common
{
    public static class SystemMessage
    {

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
        public static string OtpExpired = "Your OTP has expired. Please request a new one.";
        public static string OtpAlreadyUsed = "This OTP has already been used.";
        public const string InvalidOtp = "Invalid or expired OTP.";
        public const string InvalidOtpFormat = "Invalid OTP format. Enter a 6-digit number.";
        public const string PasswordResetSuccess = "Password reset successful. Please login with your new password.";
        public const string EmailVerifiedRegistrationRequired = "Email verified. Please complete your registration.";
        public const string PhoneVerifiedRegistrationRequired = "Phone verified. Please complete your registration.";
        public const string UserNotFound = "User not found.";
        public const string OtpWaitMessage = "Please wait {0} seconds before requesting another OTP.";
        public const string VerifiedOtp = "OTP verified successfully. You can now reset your password.";
        public static string InvalidMobile = "Invalid mobile number";
        public static string RequestNewOtp = "Too many invalid attempts. Request a new OTP.";
        public const string LogoutSuccess = "Logged out successfully.";
        public const string InvalidRefreshToken = "Invalid or expired refresh token.";
        public const string SessionRevoked = "Session has been revoked. Please login again.";

        // ───── Seller Module ─────
        public const string SellerNotFound = "Seller profile not found. Please complete KYC first.";
        public const string SellerProfileIncomplete = "Please complete your seller profile.";

        // ───── KYC Module (Tumhara code) ─────
        public const string AlreadyKycUpdated = "KYC already verified.";
        public const string RejectKyc = "Your KYC is rejected. Please contact administration.";
        public const string KycNotFound = "Seller KYC record not found.";
        public const string KycVerified = "Seller KYC has been verified successfully.";
        public const string KycPending = "Seller KYC verification is pending.";
        public const string AadharInvalid = "Aadhar number is invalid.";
        public const string AadharOtpSend = "Aadhaar KYC OTP sent successfully.";
        public const string AadharKycSubmitted = "KYC submitted successfully.";

        // ───── Address Module ─────
        public const string AddressNotFound = "Address not found.";
        public const string AddressAddedSuccess = "Pickup address added successfully.";
        public const string AddressUpdatedSuccess = "Address updated successfully.";
        public const string AddressDeletedSuccess = "Address deleted successfully.";
        public const string IsValidAddress = "Invalid Pickup Address";

        // ───── Order Module ─────
        public const string OrderNotFound = "Order not found.";
        public const string OrderCreatedSuccess = "Order created successfully.";
        public const string OrderUpdatedSuccess = "Order updated successfully.";
        public const string OrderFetchedSuccess = "Orders fetched successfully";
        public const string OrderCancelledSuccess = "Order cancelled successfully.";
        public const string OrderCannotBeEdited = "Cannot edit order once it has been shipped or processed.";
        public const string OrderCannotBeCancelled = "Cannot cancel order once it has been shipped or processed.";
        public const string AtLeastOneItemRequired = "At least one item is required.";
        public const string IsOrderRefExist = "Order reference already exists. Please use a different reference.";

        // ───── Shipment Module ─────
        public const string ShipmentNotFound = "Shipment not found.";
        public const string ShipmentRatesFetchedSuccess = "Shipment rates fetched successfully.";
        public const string ShipmentsFetchedSuccess = "Shipments fetched successfully.";
        public const string ShipmentBookedSuccess = "Shipment booked successfully.";
        public const string ShipmentBookingFailed = "Shipment booking failed.";
        public const string ShipmentAlreadyExists = "An active shipment already exists for this order.";
        public const string OrderNotShippable = "Order is not in a shippable status. Only pending orders can be booked.";
        public const string CourierNotFound = "Courier not found or inactive.";
        public const string CourierNotSupported = "No adapter is registered for the selected courier.";
        public const string PickupAddressRequired = "Order must have a valid pickup address before booking.";
        public const string ReturnWarehouseRequired = "Reverse orders need a seller return warehouse (pickup address or default warehouse).";
        public const string ChargeAmountInvalid = "Charge amount must be greater than zero.";
        public const string ChargeAmountMismatch = "Charge amount does not match the selected courier rate. Please refresh rates and try again.";
        public const string CourierRequired = "CourierId or CourierCode is required.";
        public const string InvalidShipmentStatusTab = "Invalid StatusTab. Use all, Booked, PendingPickup, PickedUp, InTransit, OutForDelivery, Delivered, RTO, Exception, or Cancelled.";
        public const string ShipmentLabelFetchedSuccess = "Shipment label fetched successfully.";
        public const string ShipmentCancelledSuccess = "Shipment cancelled successfully.";
        public const string ShipmentTrackedSuccess = "Shipment tracking fetched successfully.";
        public const string DataFound = "Data fetched successfully.";
        public const string ShipmentCancelFailed = "Shipment cancellation failed.";
        public const string ShipmentLabelFailed = "Failed to fetch shipment label.";
        public const string ShipmentTrackFailed = "Failed to fetch shipment tracking.";
        public const string ShipmentNotCancellable = "Shipment cannot be cancelled in its current status.";
        public const string InvalidShipmentStatusTransition = "Invalid shipment status transition.";
        public const string TrackingWebhookAccepted = "Tracking webhook processed successfully.";
        public const string TrackingWebhookInvalid = "Invalid tracking webhook payload.";
        public const string AwbRequired = "AWB number is required.";

        // ───── NDR Module ─────
        public const string NdrFetchedSuccess = "NDRs fetched successfully.";
        public const string NdrNotFound = "NDR not found.";
        public const string NdrActionSuccess = "NDR action applied successfully.";
        public const string NdrActionNotAllowed = "Action is only allowed on open NDRs (Action Required / Action Requested).";
        public const string NdrActionInvalid = "Invalid NDR action. Supported actions: Reattempt, Rto / MarkRto.";
        public const string InvalidNdrStatusTab = "Invalid StatusTab. Use all, action-required, action-requested, delivered, pending, or rto.";


        // ───── Wallet Module ─────
        public const string WalletNotFound = "Wallet not found.";
        public const string WalletInsufficientBalance = "Insufficient wallet balance to book this shipment.";
        public const string WalletDebitSuccess = "Wallet debited successfully.";
        public const string WalletCreditSuccess = "Wallet credited successfully.";
        public const string WalletFetchedSuccess = "Wallet balance fetched successfully.";
        public const string WalletAmountInvalid = "Amount must be greater than zero.";
        public const string WalletRechargeCreated = "Wallet recharge order created successfully.";
        public const string WalletRechargeNotFound = "Wallet recharge order not found.";
        public const string WalletPaymentVerified = "Payment verified and wallet credited successfully.";
        public const string WalletPaymentAlreadyProcessed = "Payment already processed.";
        public const string WalletWebhookAccepted = "Payment webhook processed successfully.";
        public const string WalletWebhookInvalidSignature = "Invalid Razorpay webhook signature.";
        public const string WalletWebhookIgnored = "Webhook event ignored.";
        public const string RazorpayCredentialsMissing =
            "Razorpay credentials are not configured. Set Razorpay:KeyId and Razorpay:KeySecret (User Secrets or environment).";

        // ───── Rate Card / Settlement Module ─────
        public const string CourierSettlementFetchedSuccess = "Courier settlement data fetched successfully.";
        public const string CourierSettlementCreatedSuccess = "Courier settlement batch created successfully.";
        public const string CourierSettlementPaidSuccess = "Courier settlement marked as paid.";
        public const string CourierSettlementNotFound = "Courier settlement not found.";
        public const string CourierSettlementNoShipments = "No unsettled shipments found for the selected courier and period.";
        public const string CourierSettlementInvalidPeriod = "Invalid settlement period. End date must be on or after start date.";
        public const string CourierSettlementPaymentRefRequired = "Payment reference is required to mark settlement as paid.";
        public const string CourierSettlementAlreadyPaid = "This settlement has already been marked as paid.";

        // ───── Weight Management Module ─────
        public const string WeightDiscrepancyFetchedSuccess = "Weight discrepancies fetched successfully.";
        public const string WeightDiscrepancyNotFound = "Weight discrepancy not found.";
        public const string WeightDiscrepancyAccepted = "Weight discrepancy accepted successfully.";
        public const string WeightDiscrepancyDisputed = "Weight discrepancy dispute opened successfully.";
        public const string WeightDiscrepancyClosed = "Weight discrepancy dispute closed successfully.";
        public const string WeightDiscrepancyActionNotAllowed = "Action is only allowed on Action Required discrepancies.";
        public const string WeightDiscrepancyCloseNotAllowed = "Only open disputes can be closed.";
        public const string InvalidWeightDiscrepancyStatusTab = "Invalid StatusTab. Use all, action-required, accepted, open-disputes, or closed-disputes.";
        public const string WeightFreezeFetchedSuccess = "Product weight freeze records fetched successfully.";
        public const string WeightFreezeNotFound = "Product weight freeze record not found.";
        public const string WeightFreezeCreatedSuccess = "Weight freeze request submitted successfully.";
        public const string WeightFreezeActionSuccess = "Weight freeze action applied successfully.";
        public const string WeightFreezeActionNotAllowed = "Action is only allowed on Requested weight freeze records.";
        public const string WeightFreezeActionInvalid = "Invalid action. Supported actions: Accept, Reject.";
        public const string WeightFreezeImportSuccess = "Weight freeze CSV imported successfully.";
        public const string WeightFreezeImportEmpty = "CSV file is empty or missing required columns.";
        public const string InvalidWeightFreezeStatusTab = "Invalid StatusTab. Use all, requested, accepted, or rejected.";
        public const string WeightExportSuccess = "Export generated successfully.";

        // ───── Common/Generic ─────
        public const string SomethingWentWrong = "Something went wrong. Please try again later.";
        public const string NotFound = "Data not found.";
        public const string RequestBodyNull = "Request body cannot be null.";
        public const string RequiredId = "Id is required.";
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string InvalidToken = "Invalid or missing authentication token.";
        public const string ValidationFailed = "Validation failed. Please check your input.";
    }
}

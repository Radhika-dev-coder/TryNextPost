using Microsoft.Extensions.Logging;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.IServices.Interface.Courier;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    /// <summary>
    /// Shared stub behavior until real courier HTTP APIs are wired.
    /// When credentials (BaseUrl + ApiKey) are present, <see cref="EnsureApiReady"/> throws NotImplementedException
    /// so callers know config is ready but integration code is still pending.
    /// </summary>
    public abstract class CourierAdapterBase : ICourierAdapter
    {
        private readonly ILogger _logger;

        protected CourierAdapterBase(ILogger logger)
        {
            _logger = logger;
        }

        public abstract string CourierCode { get; }

        protected abstract CourierProviderSettings Settings { get; }

        /// <summary>
        /// True when appsettings has BaseUrl and ApiKey for this courier.
        /// </summary>
        protected bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Settings.BaseUrl)
            && !string.IsNullOrWhiteSpace(Settings.ApiKey);

        public virtual Task<CourierRateResponse> GetRatesAsync(
            CourierRateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (IsConfigured)
                return GetRatesInternalAsync(request, cancellationToken);

            return Task.FromResult(CreateStubRates(request));
        }

        public virtual Task<CourierBookShipmentResponse> BookShipmentAsync(
            CourierBookShipmentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (IsConfigured)
                return BookShipmentInternalAsync(request, cancellationToken);

            return Task.FromResult(CreateStubBooking(request));
        }

        public virtual Task<CourierLabelResponse> GetLabelAsync(
            CourierLabelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (IsConfigured)
                return GetLabelInternalAsync(request, cancellationToken);

            return Task.FromResult(CreateStubLabel(request));
        }

        public virtual Task<CourierCancelResponse> CancelAsync(
            CourierCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (IsConfigured)
                return CancelInternalAsync(request, cancellationToken);

            return Task.FromResult(CreateStubCancel(request));
        }

        public virtual Task<CourierTrackResponse> TrackAsync(
            CourierTrackRequest request,
            CancellationToken cancellationToken = default)
        {
            if (IsConfigured)
                return TrackInternalAsync(request, cancellationToken);

            return Task.FromResult(CreateStubTrack(request));
        }

        /// <summary>Override when wiring the real GetRates API.</summary>
        protected virtual Task<CourierRateResponse> GetRatesInternalAsync(
            CourierRateRequest request,
            CancellationToken cancellationToken)
        {
            EnsureApiReady(nameof(GetRatesAsync));
            throw new NotImplementedException(); // unreachable — Keep compiler happy for overrides
        }

        /// <summary>Override when wiring the real BookShipment API.</summary>
        protected virtual Task<CourierBookShipmentResponse> BookShipmentInternalAsync(
            CourierBookShipmentRequest request,
            CancellationToken cancellationToken)
        {
            EnsureApiReady(nameof(BookShipmentAsync));
            throw new NotImplementedException();
        }

        /// <summary>Override when wiring the real GetLabel API.</summary>
        protected virtual Task<CourierLabelResponse> GetLabelInternalAsync(
            CourierLabelRequest request,
            CancellationToken cancellationToken)
        {
            EnsureApiReady(nameof(GetLabelAsync));
            throw new NotImplementedException();
        }

        /// <summary>Override when wiring the real Cancel API.</summary>
        protected virtual Task<CourierCancelResponse> CancelInternalAsync(
            CourierCancelRequest request,
            CancellationToken cancellationToken)
        {
            EnsureApiReady(nameof(CancelAsync));
            throw new NotImplementedException();
        }

        /// <summary>Override when wiring the real Track API.</summary>
        protected virtual Task<CourierTrackResponse> TrackInternalAsync(
            CourierTrackRequest request,
            CancellationToken cancellationToken)
        {
            EnsureApiReady(nameof(TrackAsync));
            throw new NotImplementedException();
        }

        protected void EnsureApiReady(string operation)
        {
            throw new NotImplementedException(
                $"[STUB] {CourierCode} credentials are configured (BaseUrl present) but {operation} HTTP integration is not implemented yet. " +
                $"Replace {GetType().Name}.*InternalAsync with the real API client.");
        }

        protected CourierRateResponse CreateStubRates(CourierRateRequest request)
        {
            _logger.LogWarning(
                "[STUB] {CourierCode} GetRates — returning fake rates. Origin={Origin} Dest={Dest}",
                CourierCode,
                request.OriginPincode,
                request.DestinationPincode);

            var weightFactor = Math.Max(request.WeightKg, 0.5m);
            var baseCharge = 40m + (weightFactor * 25m);
            var codCharge = request.IsCod ? 30m : 0m;

            return new CourierRateResponse
            {
                Success = true,
                IsStub = true,
                CourierCode = CourierCode,
                Message = $"[STUB] Fake rates from {CourierCode}. Not a live courier quote.",
                Rates =
                [
                    new CourierRateOption
                    {
                        ServiceName = $"{CourierCode} Surface (STUB)",
                        ServiceCode = $"{CourierCode}_SURFACE_STUB",
                        TotalCharge = Math.Round(baseCharge + codCharge, 2),
                        CodCharge = codCharge,
                        EstimatedDays = 4,
                        IsStub = true
                    },
                    new CourierRateOption
                    {
                        ServiceName = $"{CourierCode} Express (STUB)",
                        ServiceCode = $"{CourierCode}_EXPRESS_STUB",
                        TotalCharge = Math.Round(baseCharge * 1.35m + codCharge, 2),
                        CodCharge = codCharge,
                        EstimatedDays = 2,
                        IsStub = true
                    }
                ]
            };
        }

        protected CourierBookShipmentResponse CreateStubBooking(CourierBookShipmentRequest request)
        {
            var stubAwb = $"STUB{CourierCode[..Math.Min(3, CourierCode.Length)]}{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";

            _logger.LogWarning(
                "[STUB] {CourierCode} BookShipment — fake AWB {Awb} for OrderRef={OrderRef}",
                CourierCode,
                stubAwb,
                request.OrderRef);

            return new CourierBookShipmentResponse
            {
                Success = true,
                IsStub = true,
                CourierCode = CourierCode,
                AwbNumber = stubAwb,
                CourierReference = $"STUB-REF-{request.OrderRef}",
                LabelUrl = null,
                Message = $"[STUB] Fake AWB from {CourierCode}. Do not use for real shipping."
            };
        }

        protected CourierLabelResponse CreateStubLabel(CourierLabelRequest request)
        {
            _logger.LogWarning("[STUB] {CourierCode} GetLabel for AWB {Awb}", CourierCode, request.AwbNumber);

            var stubText = $"[STUB LABEL] {CourierCode} AWB:{request.AwbNumber}";
            return new CourierLabelResponse
            {
                Success = true,
                IsStub = true,
                CourierCode = CourierCode,
                LabelUrl = null,
                LabelContent = System.Text.Encoding.UTF8.GetBytes(stubText),
                ContentType = "text/plain",
                Message = $"[STUB] Fake label content from {CourierCode}."
            };
        }

        protected CourierCancelResponse CreateStubCancel(CourierCancelRequest request)
        {
            _logger.LogWarning("[STUB] {CourierCode} Cancel AWB {Awb}", CourierCode, request.AwbNumber);

            return new CourierCancelResponse
            {
                Success = true,
                IsStub = true,
                CourierCode = CourierCode,
                Message = $"[STUB] Fake cancel acknowledged by {CourierCode} for AWB {request.AwbNumber}."
            };
        }

        protected CourierTrackResponse CreateStubTrack(CourierTrackRequest request)
        {
            _logger.LogWarning("[STUB] {CourierCode} Track AWB {Awb}", CourierCode, request.AwbNumber);

            var now = DateTime.UtcNow;
            return new CourierTrackResponse
            {
                Success = true,
                IsStub = true,
                CourierCode = CourierCode,
                AwbNumber = request.AwbNumber,
                CurrentStatus = "InTransit",
                Message = $"[STUB] Fake tracking from {CourierCode}.",
                Events =
                [
                    new CourierTrackEvent
                    {
                        EventTime = now.AddHours(-6),
                        Status = "PickedUp",
                        StatusCode = "STUB_PU",
                        Location = "Origin Hub (STUB)",
                        Description = $"[STUB] Shipment picked up by {CourierCode}."
                    },
                    new CourierTrackEvent
                    {
                        EventTime = now.AddHours(-2),
                        Status = "InTransit",
                        StatusCode = "STUB_IT",
                        Location = "Transit Hub (STUB)",
                        Description = "[STUB] In transit to destination."
                    }
                ]
            };
        }
    }
}

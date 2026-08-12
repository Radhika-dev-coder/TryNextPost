using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Pincode;
using TryNextPost.Application.IServices.Interface;


namespace TryNextPost.Application.IServices.Class
{
    public class PincodeService : IPincodeService
    {
        private readonly HttpClient _httpClient;

        public PincodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LocationResponseDto> GetAddressFromCoordinates(LocationRequestDto request)
        {
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={request.Latitude}&lon={request.Longitude}";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TryNextPostApp");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch location");

            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var address = root.GetProperty("address");

            var city = address.TryGetProperty("city", out var c) ? c.GetString()
                     : address.TryGetProperty("town", out var t) ? t.GetString()
                     : address.TryGetProperty("village", out var v) ? v.GetString()
                     : "";

            var state = address.GetProperty("state").GetString();
            var country = address.GetProperty("country").GetString();
            var pincode = address.TryGetProperty("postcode", out var p) ? p.GetString() : "";

            var road = address.TryGetProperty("road", out var r) ? r.GetString() : "";
            var area = address.TryGetProperty("suburb", out var a) ? a.GetString() : "";

            var fullAddress = string.Join(", ", new[]
                                      {
                              road,
                              area,
                              city,
                              state,
                              pincode
                          }.Where(x => !string.IsNullOrEmpty(x)));
             
                         return new LocationResponseDto
                         {
                             FullAddress = fullAddress,
                             State = state,
                             City = city,
                             Area = area,
                             Road = road,
                             Pincode = pincode,
                             Country = country
                         };
        }

        public async Task<PincodeResponseDto> GetAddressFromPincode(string pincode)
        {
            var response = await _httpClient.GetAsync($"https://api.postalpincode.in/pincode/{pincode}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch pincode data");

            var json = await response.Content.ReadAsStringAsync();

            var data = JsonDocument.Parse(json);

            var root = data.RootElement[0];

            if (root.GetProperty("Status").GetString() != "Success")
                throw new Exception("Invalid pincode");

            var postOffice = root.GetProperty("PostOffice")[0];

            return new PincodeResponseDto
            {
                Pincode = pincode,
                State = postOffice.GetProperty("State").GetString(),
                City = postOffice.GetProperty("District").GetString(),
                Area = postOffice.GetProperty("Name").GetString()
            };
        }
    }
}

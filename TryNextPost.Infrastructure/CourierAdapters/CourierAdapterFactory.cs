using TryNextPost.Application.IServices.Interface.Courier;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class CourierAdapterFactory : ICourierAdapterFactory
    {
        private readonly IReadOnlyDictionary<string, ICourierAdapter> _adapters;

        public CourierAdapterFactory(IEnumerable<ICourierAdapter> adapters)
        {
            _adapters = adapters
                .GroupBy(a => a.CourierCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }

        public ICourierAdapter Resolve(string courierCode)
        {
            if (TryResolve(courierCode, out var adapter) && adapter is not null)
                return adapter;

            throw new InvalidOperationException(
                $"No courier adapter registered for code '{courierCode}'. " +
                $"Known codes: {string.Join(", ", _adapters.Keys)}.");
        }

        public bool TryResolve(string courierCode, out ICourierAdapter? adapter)
        {
            if (string.IsNullOrWhiteSpace(courierCode))
            {
                adapter = null;
                return false;
            }

            return _adapters.TryGetValue(courierCode.Trim(), out adapter);
        }

        public IReadOnlyCollection<ICourierAdapter> GetAll() => _adapters.Values.ToList();
    }
}

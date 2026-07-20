namespace TryNextPost.Application.IServices.Interface.Courier
{
    /// <summary>
    /// Resolves the correct <see cref="ICourierAdapter"/> by CourierCode (e.g. DELHIVERY).
    /// </summary>
    public interface ICourierAdapterFactory
    {
        ICourierAdapter Resolve(string courierCode);

        bool TryResolve(string courierCode, out ICourierAdapter? adapter);

        IReadOnlyCollection<ICourierAdapter> GetAll();
    }
}

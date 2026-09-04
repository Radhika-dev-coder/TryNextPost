namespace TryNextPost.Application.IServices.Interface.Courier
{
    public interface ICourierAdapterFactory
    {
        ICourierAdapter Resolve(string courierCode);

        bool TryResolve(string courierCode, out ICourierAdapter? adapter);

        IReadOnlyCollection<ICourierAdapter> GetAll();
    }
}

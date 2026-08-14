using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.IServices.Interface.ICourier
{
    public interface ICourierTestService
    {
        Task<string> GenerateXpressBeesTokenAsync();
    }
}

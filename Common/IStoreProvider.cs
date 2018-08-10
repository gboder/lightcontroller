using CameraCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IStoreProvider<T, K> : IInitializable
        where T : IEntity<K>
        where K : struct
    {
        Task Save(T capture);
        Task<IReadOnlyList<T>> List();
        Task<T> Get(K id);
        Task Delete(T capture);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoSharp
{
    public interface IRecvChannel<T>
    {
        T Recv();

        Task<T> RecvAsync();

        IEnumerable<T> Range { get; }

        void Close();
    }
}

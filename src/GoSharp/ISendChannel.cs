using System.Threading.Tasks;

namespace GoSharp
{
    public interface ISendChannel<T>
    {
        bool Send(T msg);

        Task<bool> SendAsync(T msg);

        void Close();
    }
}

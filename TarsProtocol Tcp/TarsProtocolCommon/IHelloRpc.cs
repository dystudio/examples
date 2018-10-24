using System.Threading.Tasks;
using Tars.Net.Attributes;

namespace TarsProtocolCommon
{
    [Rpc]
    public interface IHelloRpc
    {
        string Hello(int no, string name);

        void HelloHolder(int no, out string name);

        Task<string> HelloTask(int no, string name);

        ValueTask<string> HelloValueTask(int no, string name);

        [Oneway]
        Task HelloOneway(int no, string name);
    }
}
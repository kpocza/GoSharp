using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoSharp.Test
{
    [TestClass]
    public class AsyncTest
    {
        [TestMethod]
        public async Task ChannelSendRecvTest()
        {
            var channel = Channel<int>.CreateNonBuffered();
            var done = Channel<bool>.CreateNonBuffered();

            int i = 2;
            Go.Run(async () =>
            {
                i = await channel.RecvAsync();
                await done.SendAsync(true);
            });
            await channel.SendAsync(1);
            await done.RecvAsync();

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public async Task SelectAsync()
        {
            var channel = Channel<int>.CreateBuffered(1);
            var done = Channel<bool>.CreateNonBuffered();

            int i = 2;
            Go.Run(async () =>
            {
                await Select.CaseRecv(channel, a => i = a).GoAsync();
                await Select.CaseSend(done, true).GoAsync();
            });
            await Select.CaseSend(channel, 1).DefaultAsync(() => i = 3);
            await Select.CaseRecv(done, _ => {  }).GoAsync();

            Assert.AreEqual(1, i);
        }
    }
}

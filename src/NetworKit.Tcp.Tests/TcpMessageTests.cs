using NetworKit.Tcp.Utils;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetworKit.Tcp.Tests
{
    public class TcpMessageTests
    {
        [Theory]
        [InlineData("0|Message", TcpNetworkCommand.None, "Message")]
        [InlineData("1|Message", TcpNetworkCommand.ConnectionRequest, "Message")]
        [InlineData("1|", TcpNetworkCommand.ConnectionRequest, "")]
        internal void ParseMessageValidTests(string messageStr, TcpNetworkCommand cmd, string innerMessage)
        {
            var message = TcpMessage.Parse(messageStr);

            Assert.True(message.IsValid);
            Assert.Equal(cmd, message.Command);
            Assert.Equal(innerMessage, message.InnerMessage);
        }

        [Theory]
        [InlineData("12|Message")]
        [InlineData("12|")]
        public void ParseMessageOutOfEnumTests(string messageStr)
        {
            var message = TcpMessage.Parse(messageStr);

            Assert.False(message.IsValid);
            Assert.Equal(TcpNetworkCommand.None, message.Command);
            Assert.Equal(messageStr, message.InnerMessage);
        }

        [Theory]
        [InlineData("Message")]
        [InlineData("")]
        [InlineData(null)]
        public void ParseMessageMisformattedTests(string messageStr)
        {
            var message = TcpMessage.Parse(messageStr);

            Assert.False(message.IsValid);
            Assert.Equal(TcpNetworkCommand.None, message.Command);
            Assert.Equal(messageStr, message.InnerMessage);
        }

        [Theory]
        [InlineData("toto|Message")]
        [InlineData("1.1|Message")]
        [InlineData("1,1|Message")]
        [InlineData("|Message")]
        public void ParseMessageWrongCommandTests(string messageStr)
        {
            var message = TcpMessage.Parse(messageStr);

            Assert.False(message.IsValid);
            Assert.Equal(TcpNetworkCommand.None, message.Command);
            Assert.Equal(messageStr, message.InnerMessage);
        }

        [Theory]
        [InlineData(TcpNetworkCommand.Message, "Message")]
        [InlineData(TcpNetworkCommand.ConnectionRequest, "")]
        [InlineData(TcpNetworkCommand.None, null)]
        internal void ToStringTests(TcpNetworkCommand cmd, string messageStr)
        {
            var message = new TcpMessage(cmd, messageStr);

            Assert.Equal($"{(int)cmd}|{messageStr}", message.ToString());
        }
    }
}

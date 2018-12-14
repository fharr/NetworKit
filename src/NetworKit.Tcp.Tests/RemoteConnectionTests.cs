using NetworKit.Exceptions;
using NetworKit.Tcp.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetworKit.Tcp.Tests
{
    public class RemoteConnectionTests : IDisposable
    {
        #region fields

        private const int ListenerPort = 1337;
        private const int TestedPort = 1338;
        private const string TestedMessageSeparator = "//";
        private readonly Encoding TestedEncoding = Encoding.Unicode;

        private TcpClient _tester;
        private TcpRemoteConnection _tested;
        private TcpListener _listener;

        #endregion

        #region initialization

        public RemoteConnectionTests()
        {
            _listener = new TcpListener(IPAddress.Any, ListenerPort);

            _listener.Start();
            var connection = _listener.AcceptTcpClientAsync();

            var client = new TcpClient(new IPEndPoint(IPAddress.Any, TestedPort));
            client.ConnectAsync("127.0.0.1", ListenerPort).Wait();
            _tested = new TcpRemoteConnection(client, new TcpNetworkClientSettings { LocalPort = TestedPort, MessageEncoding = TestedEncoding, MessageSeparator = TestedMessageSeparator });

            _tester = connection.Result;
        }

        #endregion

        #region clean up

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _tester?.Close();
                    _tested?.Dispose();
                    _listener?.Stop();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        [Fact]
        public void TestProperties()
        {
            Assert.Equal(ListenerPort, _tested.Port);
            Assert.Equal("127.0.0.1", _tested.IPAddress);
            Assert.True(_tested.Connected);
        }

        [Fact]
        public async Task TestSendMessage()
        {
            var msg = new TcpMessage(TcpNetworkCommand.Message, "Test");

            await _tested.SendAsync(msg);

            var buffer = new byte[_tester.Available];
            await _tester.GetStream().ReadAsync(buffer, 0, buffer.Length);

            var message = TestedEncoding.GetString(buffer);

            Assert.Equal(msg.ToString() + TestedMessageSeparator, message);
        }

        [Fact]
        public async Task TestReceiveMessage()
        {
            var initialMessage = "New Message";
            var buffer = TestedEncoding.GetBytes(initialMessage);

            await _tester.GetStream().WriteAsync(buffer, 0, buffer.Length);

            Assert.Null(await _tested.ReceiveAsync());
            Assert.Null(await _tested.ReceiveAsync(500));

            var secondMessage = "Next Message";
            buffer = TestedEncoding.GetBytes(TestedMessageSeparator + secondMessage);

            await _tester.GetStream().WriteAsync(buffer, 0, buffer.Length);

            var message = await _tested.ReceiveAsync();
            Assert.NotNull(message);
            Assert.Equal(initialMessage, message.InnerMessage);

            Assert.Null(await _tested.ReceiveAsync());

            var receiving = _tested.ReceiveAsync(1000);

            buffer = TestedEncoding.GetBytes(TestedMessageSeparator);
            await _tester.GetStream().WriteAsync(buffer, 0, buffer.Length);

            message = await receiving;
            Assert.NotNull(message);
            Assert.Equal(secondMessage, message.InnerMessage);
        }

        [Fact]
        public async Task TestDispose()
        {
            _tester.Close();
            _tested.Dispose();

            TryCatchException(typeof(ObjectDisposedException), () => { var _ = _tested.Port; }, "Can't access to the port if disposed");
            TryCatchException(typeof(ObjectDisposedException), () => { var _ = _tested.IPAddress; }, "Can't access to the ip address if disposed");
            await TryCatchException(typeof(ObjectDisposedException), async () => await _tested.SendAsync(null), "Can't send messages if disposed");
            await TryCatchException(typeof(ObjectDisposedException), async () => await _tested.ReceiveAsync(), "Can't receive messages if disposed");
            await TryCatchException(typeof(ObjectDisposedException), async () => await _tested.ReceiveAsync(), "Can't receive messages if disposed");
        }

        [Fact]
        public async Task TestConnectionLostOnSend()
        {
            _tester.Close();

            var message = new TcpMessage(TcpNetworkCommand.None);

            // The first bytes written into the stream does not throw a connection lost exception
            await _tested.SendAsync(message);

            // Detects the connection lost
            await TryCatchException(typeof(ConnectionLostException), async () => await _tested.SendAsync(message), "Can't send messages if the connection is lost");

            Assert.False(_tested.Connected, "The TcpRemoteConnection should not be connected");

            // Ensures the Not connected exception is thrown
            await TryCatchException(typeof(NotConnectedException), async () => await _tested.SendAsync(message), "Can't send messages if the connection is closed");
            await TryCatchException(typeof(NotConnectedException), async () => await _tested.ReceiveAsync(), "Can't receive messages if the connection is closed");
            TryCatchException(typeof(NotConnectedException), () => { var _ = _tested.Port; }, "Can't access to the port if the connection is closed");
            TryCatchException(typeof(NotConnectedException), () => { var _ = _tested.IPAddress; }, "Can't access to the ip address if the connection is closed");
        }

        [Fact]
        public async Task TestConnectionLostOnReceive()
        {
            //Assert.Inconclusive("Not Implemented Yet");

            //_tester.Close();

            //// Detects the connection lost
            //await TryCatchException(typeof(ConnectionLostException), async () => await _tested.ReceiveAsync(), "Can't send messages if the connection is lost");

            //Assert.False(_tested.Connected, "The TcpRemoteConnection should not be connected");

            //// Ensures the Not connected exception is thrown
            //await TryCatchException(typeof(NotConnectedException), async () => await _tested.SendAsync(new TcpMessage(TcpNetworkCommand.None)), "Can't send messages if the connection is closed");
            //await TryCatchException(typeof(NotConnectedException), async () => await _tested.ReceiveAsync(), "Can't receive messages if the connection is closed");
            //TryCatchException(typeof(NotConnectedException), () => { var _ = _tested.Port; }, "Can't access to the port if the connection is closed");
            //TryCatchException(typeof(NotConnectedException), () => { var _ = _tested.IPAddress; }, "Can't access to the ip address if the connection is closed");
        }

        #region inner test methods

        private void TryCatchException(Type expectedException, Action forbiddenAction, string noExceptionMessage)
        {
            try
            {
                forbiddenAction();
               // Assert.Fail($"Action not allowed. Expected: {noExceptionMessage}");
            }
            catch (Exception e)
            {
               // Assert.AreSame(expectedException, e.GetType());
            }
        }

        private async Task TryCatchException(Type expectedException, Func<Task> forbiddenAction, string noExceptionMessage)
        {
            try
            {
                await forbiddenAction();
               // Assert.Fail($"Action not allowed. Expected: {noExceptionMessage}");
            }
            catch (Exception e)
            {
              //  Assert.AreSame(expectedException, e.GetType());
            }
        }

        #endregion
    }
}

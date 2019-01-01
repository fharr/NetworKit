using NetworKit.Exceptions;
using NetworKit.MessageHandler.Queue;
using NetworKit.Tcp.Utils;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetworKit.Tcp.Tests.Client
{
    public class ConnectionTests : IDisposable
    {
        #region fields

        private const int ListenerPort = 1339;
        private const int TestedPort = 1340;

        private INetworkClientMessageHandler _handler;

        private TcpListener _listener;
        private TcpNetworkClient _tested;

        #endregion

        #region initialization

        public ConnectionTests()
        {
            _listener = new TcpListener(IPAddress.Any, ListenerPort);
            _listener.Start();

            _handler = new NetworkClientQueueMessageHandler();

            _tested = new TcpNetworkClient(_handler);
            _tested.Settings.LocalPort = TestedPort;
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

        [Theory]
        [InlineData("Hello world", "Hello test")]
        [InlineData("Hello world", null)]
        [InlineData("Hello world", "")]
        [InlineData(null, "Hello test")]
        [InlineData("", "Hello test")]
        public async Task TestSuccessfullConnection(string clientRequest, string serverResponse)
        {
            var serverConnection = _listener.AcceptTcpClientAsync();

            await Task.Delay(30);

            var clientConnection = _tested.ConnectAsync("127.0.0.1", ListenerPort, clientRequest);

            using (var remote = new TcpRemoteConnection(await serverConnection, _tested.TcpSettings))
            {
                var request = await remote.ReceiveAsync(200);

                Assert.Equal(TcpNetworkCommand.ConnectionRequest, request.Command);
                Assert.Equal(clientRequest ?? "", request.InnerMessage);

                await remote.SendAsync(new TcpMessage(TcpNetworkCommand.ConnectionGranted, serverResponse));

                var response = await clientConnection;

                Assert.Equal(serverResponse ?? "", response);
            }
        }

        [Fact]
        public async Task TestRemoteConnectionUnreachableConnection()
        {
            _tested.TcpSettings.ConnectionTimeout = 1000;

            var chrono = Stopwatch.StartNew();

            var e = await Assert.ThrowsAsync<ConnectionFailedException>(async () => await _tested.ConnectAsync("128.0.0.1", ListenerPort));

            chrono.Stop();

            Assert.Equal(ConnectionFailedType.RemoteConnectionUnreachable, e.TypeErreur);
            Assert.True(chrono.ElapsedMilliseconds > _tested.TcpSettings.ConnectionTimeout, $"The connection request timeout too quickly (Expected: { _tested.TcpSettings.ConnectionTimeout}, Actual: {chrono.ElapsedMilliseconds})");
            Assert.True(chrono.ElapsedMilliseconds < _tested.TcpSettings.ConnectionTimeout + 500, $"The connection request timeout too slowly (Expected: { _tested.TcpSettings.ConnectionTimeout}, Actual: {chrono.ElapsedMilliseconds})");
        }

        [Fact]
        public async Task TestRemoteConnectionImpossible()
        {
            _listener.Stop();

            var e = await Assert.ThrowsAsync<ConnectionFailedException>(async () => await _tested.ConnectAsync("127.0.0.1", ListenerPort));

            Assert.Equal(ConnectionFailedType.ConnectionRequestFailed, e.TypeErreur);
        }

        [Fact]
        public async Task TestConnectionTimeout()
        {
            _tested.TcpSettings.ConnectionTimeout = 2000;

            var clientRequest = "Hello world";

            var serverConnection = _listener.AcceptTcpClientAsync();

            await Task.Delay(30);

            var chrono = Stopwatch.StartNew();

            var e = await Assert.ThrowsAsync<ConnectionFailedException>(async () => await _tested.ConnectAsync("127.0.0.1", ListenerPort, clientRequest));

            chrono.Stop();

            Assert.Equal(ConnectionFailedType.ConnectionTimeout, e.TypeErreur);
            Assert.True(chrono.ElapsedMilliseconds > _tested.TcpSettings.ConnectionTimeout, $"The connection request timeout too quickly (Expected: { _tested.TcpSettings.ConnectionTimeout}, Actual: {chrono.ElapsedMilliseconds})");
            Assert.True(chrono.ElapsedMilliseconds < _tested.TcpSettings.ConnectionTimeout + 100, $"The connection request timeout too slowly (Expected: { _tested.TcpSettings.ConnectionTimeout}, Actual: {chrono.ElapsedMilliseconds})");

            (await serverConnection).Close();
        }

        [Fact]
        public async Task TestConnectionUnexpectedResponse()
        {
            var clientRequest = "Hello world";

            var serverConnection = _listener.AcceptTcpClientAsync();

            await Task.Delay(30);

            var clientConnection = _tested.ConnectAsync("127.0.0.1", ListenerPort, clientRequest);

            using (var remote = new TcpRemoteConnection(await serverConnection, _tested.TcpSettings))
            {
                var serverResponse = "Hello test";

                await remote.SendAsync(new TcpMessage(TcpNetworkCommand.Message, serverResponse));

                var e = await Assert.ThrowsAsync<ConnectionFailedException>(async () => await clientConnection);

                Assert.Equal(ConnectionFailedType.UnexpectedResponse, e.TypeErreur);
            }
        }

        [Fact]
        public async Task TestConnectionInvalidResponse()
        {
            var clientRequest = "Hello world";

            var serverConnection = _listener.AcceptTcpClientAsync();

            await Task.Delay(30);

            var clientConnection = _tested.ConnectAsync("127.0.0.1", ListenerPort, clientRequest);

            using (var remote = new TcpRemoteConnection(await serverConnection, new TcpNetworkServerSettings()))
            {
                var serverResponse = "Hello test";

                await remote.SendAsync(new TcpMessage((TcpNetworkCommand)42, serverResponse));

                var e = await Assert.ThrowsAsync<ConnectionFailedException>(async () => await clientConnection);

                Assert.Equal(ConnectionFailedType.InvalidResponse, e.TypeErreur);
            }
        }
    }
}

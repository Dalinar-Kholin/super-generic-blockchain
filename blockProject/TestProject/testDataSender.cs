using System.Net;
using System.Net.Sockets;
using System.Text;
using blockProject.blockchain;
using blockProject.httpServer;
using blockProject.nodeCommunicatio;
using blockProject.nodeCommunicatio.communicationFrames;
using blockProject.randomSrc;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace testProject.testCommunication;

public class testDataSender
{
	private readonly List<Keys> _keys;

	public testDataSender()
	{
		_keys = Keys.LoadFromFile("testCommunication/.KeyFile");
	}

	[Fact]
	public async Task SendData_ShouldReturnNull_WhenSendingToMockServer()
	{
		var sender = new DataSender();

		var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		var ip = (IPEndPoint)listener.LocalEndpoint;
		sender.AddIP(ip);

		var message = new messageRecord(
		  Convert.ToBase64String(_keys[1].PublicKey),
		  Encoding.UTF8.GetBytes("test message"),
		  _keys[0]
		);

		var serverTask = Task.Run(async () =>
		{
			using var client = await listener.AcceptTcpClientAsync();
			using var stream = client.GetStream();

			var buffer = new byte[4096];
			var read = await stream.ReadAsync(buffer);
			var received = Encoding.UTF8.GetString(buffer, 0, read);

			var frame = JsonConvert.DeserializeObject<Frame>(received);
			frame.Should().NotBeNull();
			frame!.Request.Should().Be(Requests.ADD_RECORD);

			var response = JsonConvert.SerializeObject(new Frame(Requests.ADD_RECORD, frame.data));
			var responseBytes = Encoding.UTF8.GetBytes(response);
			await stream.WriteAsync(responseBytes);
		});


		var error = await sender.SendData(message);


		error.Should().BeNull();

		listener.Stop();
	}

	[Fact]
	public async Task SendData_ShouldReturnError_WhenServerNotAvailable()
	{
		// Arrange
		var sender = new DataSender();
		var fakeIp = new IPEndPoint(IPAddress.Loopback, 9999);
		sender.AddIP(fakeIp);

		var message = new messageRecord(
		  Convert.ToBase64String(_keys[1].PublicKey),
		  Encoding.UTF8.GetBytes("test message"),
		  _keys[0]
		);


		var error = await sender.SendData(message);

		error.Should().NotBeNull();
		error!.Message.Should().Contain("Błąd podczas wysyłania rekordu");
	}
}
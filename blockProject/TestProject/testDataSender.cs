using System.Net;
using System.Net.Sockets;
using System.Text;
using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using blockProject.randomSrc;
//using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace TestProject;

public class DataSenderTests
{
	[Fact]
	public void AddIP_ShouldAddNewIPEndPoint()
	{
		var sender = new DataSender();
		var ip = new IPEndPoint(IPAddress.Loopback, 12345);

		sender.getIpMaster().AddIP(ip);

		Assert.Contains(ip.ToString(), sender.getIpMaster().GetIps());
	}

	[Fact]
	public void AddIP_ShouldNotAddDuplicateIPEndPoint()
	{
		var sender = new DataSender();
		var ip = new IPEndPoint(IPAddress.Loopback, 12345);

		sender.getIpMaster().AddIP(ip);
		sender.getIpMaster().AddIP(ip);

		Assert.Single(sender.getIpMaster().GetIps());
	}

	[Fact]
	public async Task SendData_ShouldReturnError_WhenDataIsNull()
	{
		var sender = new DataSender();

		var result = await sender.SendData<object>(null!);

		Assert.NotNull(result);
		Assert.Equal("empty data", result!.Message);
	}


	//[Fact]
	//public async Task PingNode_ShouldReturnError_WhenNodeIsUnreachable()
	//{
	//	var sender = new DataSender();
	//	sender.AddIP(new IPEndPoint(IPAddress.Loopback, 65000)); // Port zamknięty

	//	var result = await sender.pingNode();

	//	Assert.NotNull(result);
	//	Assert.Contains("nie udało się wysłać danych", result!.Message);
	//}

	[Fact]
	public void GetIps_ShouldReturnFormattedIPs()
	{
		var sender = new DataSender();
		var ip1 = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 1234);
		var ip2 = new IPEndPoint(IPAddress.Parse("10.0.0.2"), 5678);

		sender.getIpMaster().AddIP(ip1);
		sender.getIpMaster().AddIP(ip2);

		var ips = sender.getIpMaster().GetIps();

		Assert.Contains("192.168.0.1:1234", ips);
		Assert.Contains("10.0.0.2:5678", ips);
	}
}

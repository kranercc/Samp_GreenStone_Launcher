using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Samp_GreenStone_Launcher
{
	// Token: 0x02000002 RID: 2
	internal class RCONQuery
	{
		// Token: 0x06000004 RID: 4 RVA: 0x0000251C File Offset: 0x0000091C
		public RCONQuery(string IP, int port, string password)
		{
			this.qSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this.qSocket.SendTimeout = 5000;
			this.qSocket.ReceiveTimeout = 5000;
			this.address = Dns.GetHostAddresses(IP)[0];
			this._port = port;
			this._password = password;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000025BC File Offset: 0x000009BC
		public bool Send(string command)
		{
			IPEndPoint remoteEP = new IPEndPoint(this.address, this._port);
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write("SAMP".ToCharArray());
			string[] array = this.address.ToString().Split(new char[]
			{
				'.'
			});
			binaryWriter.Write(Convert.ToByte(Convert.ToInt32(array[0])));
			binaryWriter.Write(Convert.ToByte(Convert.ToInt32(array[1])));
			binaryWriter.Write(Convert.ToByte(Convert.ToInt32(array[2])));
			binaryWriter.Write(Convert.ToByte(Convert.ToInt32(array[3])));
			binaryWriter.Write((ushort)this._port);
			binaryWriter.Write('x');
			binaryWriter.Write((ushort)this._password.Length);
			binaryWriter.Write(this._password.ToCharArray());
			binaryWriter.Write((ushort)command.Length);
			binaryWriter.Write(command.ToCharArray());
			bool flag = this.qSocket.SendTo(memoryStream.ToArray(), remoteEP) > 0;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000275C File Offset: 0x00000B5C
		public int Recieve()
		{
			for (int i = 0; i < this.results.GetLength(0); i++)
			{
				this.results.SetValue(null, i);
			}
			this._count = 0;
			EndPoint endPoint = new IPEndPoint(this.address, this._port);
			byte[] buffer = new byte[500];
			int num = this.qSocket.ReceiveFrom(buffer, ref endPoint);
			MemoryStream memoryStream = new MemoryStream(buffer);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			bool flag = memoryStream.Length <= 11L;
			int count;
			if (flag)
			{
				count = this._count;
			}
			else
			{
				binaryReader.ReadBytes(11);
				short value;
				while ((value = binaryReader.ReadInt16()) != 0)
				{
					string[] array = this.results;
					int count2 = this._count;
					this._count = count2 + 1;
					array[count2] = new string(binaryReader.ReadChars(Convert.ToInt32(value)));
				}
				count = this._count;
			}
			return count;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000028FC File Offset: 0x00000CFC
		public string[] Store(int count)
		{
			string[] array = new string[count];
			int num = 0;
			while (num < count && num < this._count)
			{
				array[num] = this.results[num];
				num++;
			}
			this._count = 0;
			return array;
		}

		// Token: 0x04000001 RID: 1
		private Socket qSocket;

		// Token: 0x04000002 RID: 2
		private IPAddress address;

		// Token: 0x04000003 RID: 3
		private int _port = 0;

		// Token: 0x04000004 RID: 4
		private string _password = null;

		// Token: 0x04000005 RID: 5
		private string[] results = new string[50];

		// Token: 0x04000006 RID: 6
		private int _count = 0;
	}
}

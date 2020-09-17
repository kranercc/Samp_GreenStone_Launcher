using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Samp_GreenStone_Launcher
{
	// Token: 0x02000003 RID: 3
	internal class Query
	{
		// Token: 0x06000008 RID: 8 RVA: 0x00002944 File Offset: 0x00000D44
		public Query(string IP, int port)
		{
			this.qSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this.qSocket.SendTimeout = 5000;
			this.qSocket.ReceiveTimeout = 5000;
			this.address = Dns.GetHostAddresses(IP)[0];
			this._port = port;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000029D4 File Offset: 0x00000DD4
		public bool Send(char opcode)
		{
			EndPoint remoteEP = new IPEndPoint(this.address, this._port);
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
			binaryWriter.Write(opcode);
			bool flag = opcode == 'p';
			if (flag)
			{
				binaryWriter.Write("8493".ToCharArray());
			}
			this.timestamp[0] = DateTime.Now;
			bool flag2 = this.qSocket.SendTo(memoryStream.ToArray(), remoteEP) > 0;
			bool result;
			if (flag2)
			{
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002B60 File Offset: 0x00000F60
		public int Recieve()
		{
			this._count = 0;
			EndPoint endPoint = new IPEndPoint(this.address, this._port);
			byte[] buffer = new byte[500];
			this.qSocket.ReceiveFrom(buffer, ref endPoint);
			this.timestamp[1] = DateTime.Now;
			MemoryStream memoryStream = new MemoryStream(buffer);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			bool flag = memoryStream.Length <= 10L;
			int count;
			if (flag)
			{
				count = this._count;
			}
			else
			{
				binaryReader.ReadBytes(10);
				char c = binaryReader.ReadChar();
				char c2 = c;
				if (c2 <= 'd')
				{
					if (c2 == 'c')
					{
						int num = (int)binaryReader.ReadInt16();
						this.results = new string[num * 2];
						for (int i = 0; i < num; i++)
						{
							int count2 = (int)binaryReader.ReadByte();
							string[] array = this.results;
							int count3 = this._count;
							this._count = count3 + 1;
							array[count3] = new string(binaryReader.ReadChars(count2));
							string[] array2 = this.results;
							count3 = this._count;
							this._count = count3 + 1;
							array2[count3] = Convert.ToString(binaryReader.ReadInt32());
						}
						return this._count;
					}
					if (c2 == 'd')
					{
						int num2 = (int)binaryReader.ReadInt16();
						this.results = new string[num2 * 4];
						for (int j = 0; j < num2; j++)
						{
							string[] array3 = this.results;
							int count3 = this._count;
							this._count = count3 + 1;
							array3[count3] = Convert.ToString(binaryReader.ReadByte());
							int count4 = (int)binaryReader.ReadByte();
							string[] array4 = this.results;
							count3 = this._count;
							this._count = count3 + 1;
							array4[count3] = new string(binaryReader.ReadChars(count4));
							string[] array5 = this.results;
							count3 = this._count;
							this._count = count3 + 1;
							array5[count3] = Convert.ToString(binaryReader.ReadInt32());
							string[] array6 = this.results;
							count3 = this._count;
							this._count = count3 + 1;
							array6[count3] = Convert.ToString(binaryReader.ReadInt32());
						}
						return this._count;
					}
				}
				else
				{
					if (c2 == 'i')
					{
						this.results = new string[6];
						string[] array7 = this.results;
						int count3 = this._count;
						this._count = count3 + 1;
						array7[count3] = Convert.ToString(binaryReader.ReadByte());
						string[] array8 = this.results;
						count3 = this._count;
						this._count = count3 + 1;
						array8[count3] = Convert.ToString(binaryReader.ReadInt16());
						string[] array9 = this.results;
						count3 = this._count;
						this._count = count3 + 1;
						array9[count3] = Convert.ToString(binaryReader.ReadInt16());
						int count5 = binaryReader.ReadInt32();
						string[] array10 = this.results;
						count3 = this._count;
						this._count = count3 + 1;
						array10[count3] = new string(binaryReader.ReadChars(count5));
						int count6 = binaryReader.ReadInt32();
						string[] array11 = this.results;
						count3 = this._count;
						this._count = count3 + 1;
						array11[count3] = new string(binaryReader.ReadChars(count6));
						int count7 = binaryReader.ReadInt32();
						string[] array12 = this.results;
						count3 = this._count;
						this._count = count3 + 1;
						array12[count3] = new string(binaryReader.ReadChars(count7));
						return this._count;
					}
					if (c2 == 'p')
					{
						this.results = new string[1];
						string[] array13 = this.results;
						int count3 = this._count;
						this._count = count3 + 1;
						array13[count3] = this.timestamp[1].Subtract(this.timestamp[0]).Milliseconds.ToString();
						return this._count;
					}
					if (c2 == 'r')
					{
						int num3 = (int)binaryReader.ReadInt16();
						this.results = new string[num3 * 2];
						for (int k = 0; k < num3; k++)
						{
							int count8 = (int)binaryReader.ReadByte();
							string[] array14 = this.results;
							int count3 = this._count;
							this._count = count3 + 1;
							array14[count3] = new string(binaryReader.ReadChars(count8));
							int count9 = (int)binaryReader.ReadByte();
							string[] array15 = this.results;
							count3 = this._count;
							this._count = count3 + 1;
							array15[count3] = new string(binaryReader.ReadChars(count9));
						}
						return this._count;
					}
				}
				count = this._count;
			}
			return count;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00003048 File Offset: 0x00001448
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

		// Token: 0x04000007 RID: 7
		private Socket qSocket;

		// Token: 0x04000008 RID: 8
		private IPAddress address;

		// Token: 0x04000009 RID: 9
		private int _port = 0;

		// Token: 0x0400000A RID: 10
		private string[] results;

		// Token: 0x0400000B RID: 11
		private int _count = 0;

		// Token: 0x0400000C RID: 12
		private DateTime[] timestamp = new DateTime[2];
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;


namespace Samp_GreenStone_Launcher
{
	// Token: 0x02000005 RID: 5
	public class frmMain : Form
	{
		// Token: 0x06000013 RID: 19
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		// Token: 0x06000014 RID: 20
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00003090 File Offset: 0x00001490
		public Version LocalVersion
		{
			get
			{
				return new Version(Program.VersionText);
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000016 RID: 22 RVA: 0x0000203D File Offset: 0x0000043D
		// (set) Token: 0x06000017 RID: 23 RVA: 0x00002045 File Offset: 0x00000445
		public Version OnlineVersion { get; private set; }

		// Token: 0x06000018 RID: 24
		[DllImport("Gdi32.dll")]
		private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

		// Token: 0x06000019 RID: 25 RVA: 0x000030AC File Offset: 0x000014AC
		public frmMain(string[] args)
		{
			this.InitializeComponent();
			base.FormBorderStyle = FormBorderStyle.None;
			base.Region = Region.FromHrgn(frmMain.CreateRoundRectRgn(0, 0, base.Width, base.Height, 20, 20));
			this.minimizePictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
			this.IsOnline();
			this.FetchPatchNotes();
			this.InitializeVersionControl();
			this.CreateUrlSchemeRegistry();
			bool flag = args.Length >= 1;
			if (flag)
			{
				this.username = args[0].Replace("larp:", string.Empty).Replace("/", string.Empty);
			}
			else
			{
				bool testMode = Program.TestMode;
				if (testMode)
				{
					this.username = "Larp_Tester";
				}
				else
				{
					this.username = "NULL";
				}
			}
			bool flag2 = this.username.Length > 0;
			if (flag2)
			{
				this.PercentageLabel.Text = "Bine ai venit! Se incarca ...";
			}
			this.currentVersionLabel.Visible = Program.SHOW_VERSION_TEXT;
			this.GameStart.Start();
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000323C File Offset: 0x0000163C
		public string GetUsername()
		{
			return this.username;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00003254 File Offset: 0x00001654
		private void GameStart_Tick(object sender, EventArgs e)
		{
			this.GameStart.Stop();
			this.KillGameProcess();
			Debug.Print("InitLevel - " + this.InitLevel + " - ");
			switch (this.InitLevel)
			{
				case 0:
					{
						HttpStatusCode[] array = new HttpStatusCode[2];
						HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(Program.InfowebURL));
						HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
						array[0] = httpWebResponse.StatusCode;
						httpWebResponse.Close();
						httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(Program.LauncherURL));
						httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
						array[1] = httpWebResponse.StatusCode;
						httpWebResponse.Close();
						bool flag = array[0] != HttpStatusCode.OK && array[1] != HttpStatusCode.OK;
						if (flag)
						{
							this.PercentageLabel.Text = "Could not connect to server";
							this.forum.Text = "forum";
							this.SetBottomLeftLabelFunction(Program.InfowebURL);
							this.ShowBottomLabels(2);
							base.TopMost = true;
							base.TopMost = false;
						}
						else
						{
							this.GameStart.Start();
						}
						break;
					}
				case 1:
					this.StartUpdate();
					break;
				case 2:
					{
						this.GetAuthorizedFilesFromServer(Program.CheckURL + "/getfilelist.php?type=AllowedFiles");
						bool flag2 = this.BlockUnauthorizedPrograms(false);
						if (flag2)
						{
							return;
						}
						bool flag3 = !this.CompareMD5OfGameFile(Program.LauncherURL + "/getfilelist.php?type=Patch");
						if (flag3)
						{
							this.StartPatch(Program.LauncherURL + "/Patch", this.GetDissimilarFiles(), this.GetNumberOfDissimilarFiles() - 1);
						}
						else
						{
							this.GameStart.Start();
						}
						break;
					}
				case 3:
					{
						bool flag4 = this.BlockUnauthorizedPrograms(false);
						if (flag4)
						{
							return;
						}
						RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\SAMP", RegistryKeyPermissionCheck.ReadWriteSubTree);
						registryKey.SetValue("PlayerName", this.GetUsername());
						bool flag5 = this.GetNewLauncherPath() != string.Empty;
						if (flag5)
						{
							this.CreateUrlSchemeRegistry();
							Process.Start(this.GetNewLauncherPath(), this.GetUsername());
							Application.Exit();
						}
						else
						{
							bool flag6 = string.Compare(this.GetUsername(), "NULL") == 0;
							if (flag6)
							{
								this.OnlineVersion = this.GetOnlineVersion();
								bool flag7 = this.OnlineVersion == this.LocalVersion;
								if (flag7)
								{
									base.FormBorderStyle = FormBorderStyle.None;
									base.Region = Region.FromHrgn(frmMain.CreateRoundRectRgn(0, 0, base.Width, base.Height, 20, 20));
									this.label291.Visible = true;
									this.label292.Visible = false;
								}
								else
								{
									this.label291.Visible = false;
									this.label292.Visible = true;
								}
								this.tbNick.Visible = true;
								this.label3.Visible = true;
								this.label5.Visible = true;
								string path = Path.Combine(this.GetGamePath(), "nick.txt");
								bool flag8 = File.Exists(path);
								if (flag8)
								{
									string text = File.ReadAllText(path);
									Console.WriteLine(text);
									this.tbNick.Text = text;
									this.label5.Text = text + ".";
								}
								else
								{
									this.tbNick.Text = "Enter Nickname";
								}
								this.discord.Visible = true;
								this.panel.Visible = true;
								this.forum.Visible = true;
								this.PercentageLabel.Text = "Ready!";
								this.SetBottomLeftLabelFunction(Program.InfowebURL);
								this.ShowBottomLabels(2);
								base.TopMost = true;
								base.TopMost = false;
							}
						}
						break;
					}
			}
			this.InitLevel++;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003670 File Offset: 0x00001A70
		private void GameExit_Tick(object sender, EventArgs e)
		{
			bool flag = this.IsProcessRunning("gta_sa");
			if (flag)
			{
				bool flag2 = this.ExitLevel == 0;
				if (flag2)
				{
					base.Hide();
				}
				this.BlockUnauthorizedPrograms(true);
				this.ExitLevel = 1;
			}
			else
			{
				bool flag3 = this.ExitLevel > 0;
				if (flag3)
				{
					this.ExitLevel = 0;
					string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "GTA San Andreas User Files", "SAMP");
					string destFileName = Path.Combine(Program.Path_ChatLog, "ChatLog" + DateTime.Now.ToString("-yyMMdd-HHmmss") + ".txt");
					bool flag4 = !Directory.Exists(Program.Path_ChatLog);
					if (flag4)
					{
						Directory.CreateDirectory(Program.Path_ChatLog);
					}
					File.Copy(Path.Combine(path, "chatlog.txt"), destFileName, true);
					this.PercentageLabel.Text = "The chatlog was automatically backed up.";
					this.SetBottomLeftLabelFunction(Program.Path_ChatLog);
					this.ShowBottomLabels(2);
					base.Show();
					base.TopMost = true;
					base.TopMost = false;
				}
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x0000204E File Offset: 0x0000044E
		private void InitializeVersionControl()
		{
			this.currentVersionLabel.Text = Program.VersionText;
			this.OnlineVersion = this.GetOnlineVersion();
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000037AC File Offset: 0x00001BAC
		private void IsOnline()
		{
			Query query = new Query("54.38.22.23", 7777);
			query.Send('i');
			int count = query.Recieve();
			string[] array = query.Store(count);
			bool flag = array.Length == 0;
			if (flag)
			{
				this.players.Text = "";
				this.staus.Text = "OFFLINE";
				this.staus.ForeColor = Color.Red;
			}
			else
			{
				this.players.Text = array[1] + " / " + array[2];
				this.staus.Text = "ONLINE";
				this.staus.ForeColor = Color.Lime;
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00003864 File Offset: 0x00001C64
		private Version GetOnlineVersion()
		{
			string text = new WebClient().DownloadString(Program.VERSION_URL);
			Console.WriteLine(this.LocalVersion >= new Version(text));
			Version result;
			Version.TryParse(text, out result);
			return result;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000206F File Offset: 0x0000046F
		private void HeadCaption_MouseDown(object sender, MouseEventArgs e)
		{
			this.MouseLocation = e.Location;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000038D0 File Offset: 0x00001CD0
		private void HeadCaption_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = e.Button == MouseButtons.Left;
			if (flag)
			{
				int x = base.Location.X + e.Location.X - this.MouseLocation.X;
				int y = base.Location.Y + e.Location.Y - this.MouseLocation.Y;
				base.Location = new Point(x, y);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000207E File Offset: 0x0000047E
		private void SetBottomLeftLabelFunction(string function)
		{
			this.BottomLeftLabelFunction = function;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003954 File Offset: 0x00001D54
		private void ShowBottomLabels(int count)
		{
			switch (count)
			{
				case 0:
					this.forum.Visible = false;
					this.BottomRightLabel.Visible = false;
					break;
				case 1:
					this.forum.Visible = false;
					this.BottomRightLabel.Visible = false;
					break;
				case 2:
					this.forum.Visible = true;
					this.BottomRightLabel.Visible = true;
					break;
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000039D0 File Offset: 0x00001DD0
		private void FetchPatchNotes()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Program.PATCH_NOTES_URL);
			foreach (object obj in xmlDocument.DocumentElement)
			{
				XmlNode xmlNode = (XmlNode)obj;
				PatchNoteBlock patchNoteBlock = new PatchNoteBlock();
				for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
				{
					switch (i)
					{
						case 0:
							patchNoteBlock.Title = xmlNode.ChildNodes[i].InnerText;
							break;
						case 1:
							patchNoteBlock.Text = xmlNode.ChildNodes[i].InnerText;
							break;
						case 2:
							patchNoteBlock.Link = xmlNode.ChildNodes[i].InnerText;
							break;
					}
				}
				this.patchNoteBlocks.Add(patchNoteBlock);
			}
			Label[] array = new Label[]
			{
				this.patchTitle1,
				this.patchTitle2,
				this.patchTitle3
			};
			Label[] array2 = new Label[]
			{
				this.patchText1,
				this.patchText2,
				this.patchText3
			};
			for (int j = 0; j < this.patchNoteBlocks.Count; j++)
			{
				array[j].Text = this.patchNoteBlocks[j].Title;
				array2[j].Text = this.patchNoteBlocks[j].Text;
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000037AC File Offset: 0x00001BAC
		private void button7_Click(object sender, EventArgs e)
		{
			Query query = new Query("54.38.22.23", 7777);
			query.Send('i');
			int count = query.Recieve();
			string[] array = query.Store(count);
			bool flag = array.Length == 0;
			if (flag)
			{
				this.players.Text = "";
				this.staus.Text = "OFFLINE";
				this.staus.ForeColor = Color.Red;
			}
			else
			{
				this.players.Text = array[1] + " / " + array[2];
				this.staus.Text = "ONLINE";
				this.staus.ForeColor = Color.Lime;
			}
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002088 File Offset: 0x00000488
		private void discord_Click(object sender, EventArgs e)
		{
			Process.Start("https://discord.gg/RpD7QJQ");
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002096 File Offset: 0x00000496
		private void forum_Click(object sender, EventArgs e)
		{
			Process.Start("https://forum.g-stone.ro");
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000020A4 File Offset: 0x000004A4
		private void panel_Click(object sender, EventArgs e)
		{
			Process.Start("https://panel.g-stone.ro");
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000020B2 File Offset: 0x000004B2
		private void tbNick_Text(object sender, EventArgs e)
		{
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003BD4 File Offset: 0x00001FD4
		private void tbNick_Changed(object sender, EventArgs e)
		{
			string path = Path.Combine(this.GetGamePath(), "nick.txt");
			bool flag = !File.Exists(path);
			if (flag)
			{
				FileStream fileStream = File.Create(path);
				fileStream.Close();
				StreamWriter streamWriter = File.CreateText(path);
				streamWriter.Write(this.tbNick.Text);
			}
			else
			{
				StreamWriter streamWriter2 = File.CreateText(path);
				streamWriter2.Write(this.tbNick.Text);
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003C80 File Offset: 0x00002080
		private void tbNick_Enter(object sender, EventArgs e)
		{
			bool flag = this.tbNick.Text == "Enter Nickname";
			if (flag)
			{
				this.tbNick.Text = "";
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003CBC File Offset: 0x000020BC
		private void tbNick_Leave(object sender, EventArgs e)
		{
			bool flag = this.tbNick.Text == "";
			if (flag)
			{
				this.tbNick.Text = "Enter Nickname";
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003CF8 File Offset: 0x000020F8
		private void label291_Click(object sender, EventArgs e)
		{
			this.OnlineVersion = this.GetOnlineVersion();
			bool flag = this.OnlineVersion == this.LocalVersion;
			if (flag)
			{
				this.label291.Visible = true;
				this.label292.Visible = false;
				base.FormBorderStyle = FormBorderStyle.None;
				base.Region = Region.FromHrgn(frmMain.CreateRoundRectRgn(0, 0, base.Width, base.Height, 20, 20));
				Registry.CurrentUser.OpenSubKey("Software\\SAMP", true).SetValue("PlayerName", this.tbNick.Text);
				Button value = new Button();
				base.Controls.Add(value);
				string path = this.GetGamePath() + "\\SPC_PA\\v2.0.5.txt";
				this.PercentageLabel.Text = "You are currently online!";
				bool flag2 = !File.Exists(path);
				if (flag2)
				{
					MessageBox.Show("New Update!\n\nFisierele jocului au fost actualizate, acesta versiune de GTA nu mai este disponibila!\n Noul setup este disponibil pe https://download.g-stone.ro, mai multe detalii pe discord.");
					this.PercentageLabel.Text = "Update Disponibil";
				}
				else
				{
					this.PercentageLabel.Text = "You are currently online!";
					Process.Start(new ProcessStartInfo
					{
						FileName = "samp.exe",
						WorkingDirectory = this.GetGamePath(),
						Arguments = string.Format("{0} {1}", "54.38.22.23", "@P@R@laServerSampLauncher$@")
					});
					this.GameExit.Start();
				}
			}
			else
			{
				this.PercentageLabel.Text = "Maintenance Mode!";
				this.label291.Visible = false;
				this.label292.Visible = true;
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003E90 File Offset: 0x00002290
		private void label292_Click(object sender, EventArgs e)
		{
			bool flag = this.OnlineVersion != this.LocalVersion;
			if (flag)
			{
				Button value = new Button();
				base.Controls.Add(value);
			}
			else
			{
				this.label292.Visible = false;
				this.label291.Visible = true;
				this.PercentageLabel.Text = "Ready!";
				Registry.CurrentUser.OpenSubKey("Software\\SAMP", true).SetValue("PlayerName", this.tbNick.Text);
				this.PercentageLabel.Text = "You are currently online!";
				Process.Start(Path.Combine(this.GetGamePath(), "samp.exe"), "54.38.22.23");
				this.GameExit.Start();
			}
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003F58 File Offset: 0x00002358
		private void label291_MouseEnter(object sender, EventArgs e)
		{
			this.focus[0] = true;
			this.label291.BackColor = Color.Lime;
			this.label291.ForeColor = Color.Green;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Region = Region.FromHrgn(frmMain.CreateRoundRectRgn(0, 0, base.Width, base.Height, 20, 20));
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003FBC File Offset: 0x000023BC
		private void label291_MouseLeave(object sender, EventArgs e)
		{
			this.focus[0] = false;
			this.label291.BackColor = Color.Green;
			this.label291.ForeColor = Color.White;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Region = Region.FromHrgn(frmMain.CreateRoundRectRgn(0, 0, base.Width, base.Height, 20, 20));
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00004020 File Offset: 0x00002420
		private void label291_MouseDown(object sender, MouseEventArgs e)
		{
			this.label291.BackColor = Color.Lime;
			this.label291.ForeColor = Color.Green;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Region = Region.FromHrgn(frmMain.CreateRoundRectRgn(0, 0, base.Width, base.Height, 20, 20));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x0000407C File Offset: 0x0000247C
		private void label291_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[0];
			if (flag)
			{
				this.label291.BackColor = Color.Lime;
				this.label291.ForeColor = Color.Green;
				base.FormBorderStyle = FormBorderStyle.None;
				base.Region = Region.FromHrgn(frmMain.CreateRoundRectRgn(0, 0, base.Width, base.Height, 20, 20));
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000020B5 File Offset: 0x000004B5
		private void label292_MouseEnter(object sender, EventArgs e)
		{
			this.focus[0] = true;
			this.label292.BackColor = Color.Brown;
			this.label292.ForeColor = Color.Red;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x000020E3 File Offset: 0x000004E3
		private void label292_MouseLeave(object sender, EventArgs e)
		{
			this.focus[0] = false;
			this.label292.BackColor = Color.Red;
			this.label292.ForeColor = Color.White;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002111 File Offset: 0x00000511
		private void label292_MouseDown(object sender, MouseEventArgs e)
		{
			this.label292.BackColor = Color.Brown;
			this.label292.ForeColor = Color.Red;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x000040E8 File Offset: 0x000024E8
		private void label292_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[0];
			if (flag)
			{
				this.label292.BackColor = Color.Brown;
				this.label292.ForeColor = Color.Red;
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002136 File Offset: 0x00000536
		private void panel_MouseEnter(object sender, EventArgs e)
		{
			this.focus[0] = true;
			this.panel.ForeColor = Color.SteelBlue;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002153 File Offset: 0x00000553
		private void panel_MouseLeave(object sender, EventArgs e)
		{
			this.focus[0] = false;
			this.panel.ForeColor = Color.White;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002170 File Offset: 0x00000570
		private void panel_MouseDown(object sender, MouseEventArgs e)
		{
			this.panel.ForeColor = Color.White;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00004128 File Offset: 0x00002528
		private void panel_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[0];
			if (flag)
			{
				this.panel.ForeColor = Color.White;
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00002184 File Offset: 0x00000584
		private void forum_MouseEnter(object sender, EventArgs e)
		{
			this.focus[0] = true;
			this.forum.ForeColor = Color.SteelBlue;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000021A1 File Offset: 0x000005A1
		private void forum_MouseLeave(object sender, EventArgs e)
		{
			this.focus[0] = false;
			this.forum.ForeColor = Color.White;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000021BE File Offset: 0x000005BE
		private void forum_MouseDown(object sender, MouseEventArgs e)
		{
			this.forum.ForeColor = Color.White;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00004158 File Offset: 0x00002558
		private void forum_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[0];
			if (flag)
			{
				this.forum.ForeColor = Color.White;
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000021D2 File Offset: 0x000005D2
		private void label6_MouseEnter(object sender, EventArgs e)
		{
			this.focus[0] = true;
			this.label6.BackColor = Color.Red;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000021EF File Offset: 0x000005EF
		private void label6_MouseLeave(object sender, EventArgs e)
		{
			this.focus[0] = false;
			this.label6.BackColor = Color.Transparent;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x0000220C File Offset: 0x0000060C
		private void label6_MouseDown(object sender, MouseEventArgs e)
		{
			this.label6.BackColor = Color.Transparent;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004188 File Offset: 0x00002588
		private void label6_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[0];
			if (flag)
			{
				this.label6.BackColor = Color.Transparent;
			}
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002220 File Offset: 0x00000620
		private void label7_MouseEnter(object sender, EventArgs e)
		{
			this.focus[0] = true;
			this.label7.BackColor = Color.Blue;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000223D File Offset: 0x0000063D
		private void label7_MouseLeave(object sender, EventArgs e)
		{
			this.focus[0] = false;
			this.label7.BackColor = Color.Transparent;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x0000225A File Offset: 0x0000065A
		private void label7_MouseDown(object sender, MouseEventArgs e)
		{
			this.label7.BackColor = Color.Transparent;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000041B8 File Offset: 0x000025B8
		private void label7_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[0];
			if (flag)
			{
				this.label7.BackColor = Color.Transparent;
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x0000226E File Offset: 0x0000066E
		private void minimizePictureBox_MouseEnter(object sender, EventArgs e)
		{
			this.focus[2] = true;
			this.minimizePictureBox.BackColor = Color.SkyBlue;
			this.minimizePictureBox.ForeColor = Color.White;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x0000229C File Offset: 0x0000069C
		private void minimizePictureBox_MouseLeave(object sender, EventArgs e)
		{
			this.focus[2] = false;
			this.minimizePictureBox.BackColor = Color.Transparent;
			this.minimizePictureBox.ForeColor = Color.Transparent;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000022CA File Offset: 0x000006CA
		private void minimizePictureBox_MouseDown(object sender, MouseEventArgs e)
		{
			this.minimizePictureBox.BackColor = Color.LightBlue;
			this.minimizePictureBox.ForeColor = Color.White;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000041E8 File Offset: 0x000025E8
		private void minimizePictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[2];
			if (flag)
			{
				this.minimizePictureBox.BackColor = Color.SkyBlue;
				this.minimizePictureBox.ForeColor = Color.White;
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000022EF File Offset: 0x000006EF
		private void discord_MouseEnter(object sender, EventArgs e)
		{
			this.focus[0] = true;
			this.discord.ForeColor = Color.SteelBlue;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000230C File Offset: 0x0000070C
		private void discord_MouseLeave(object sender, EventArgs e)
		{
			this.focus[0] = false;
			this.discord.ForeColor = Color.White;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002329 File Offset: 0x00000729
		private void discord_MouseDown(object sender, MouseEventArgs e)
		{
			this.discord.ForeColor = Color.SteelBlue;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004228 File Offset: 0x00002628
		private void discord_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[0];
			if (flag)
			{
				this.discord.ForeColor = Color.SteelBlue;
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x0000233D File Offset: 0x0000073D
		private void ButtomLeftLabel_Click(object sender, EventArgs e)
		{
			Process.Start(this.BottomLeftLabelFunction);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x0000234C File Offset: 0x0000074C
		private void BottomLabel_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00002355 File Offset: 0x00000755
		private void BottomLeftLabel_MouseEnter(object sender, EventArgs e)
		{
			this.focus[1] = true;
			this.forum.BackColor = Color.SkyBlue;
			this.forum.ForeColor = Color.White;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002383 File Offset: 0x00000783
		private void BottomLeftLabel_MouseLeave(object sender, EventArgs e)
		{
			this.focus[1] = false;
			this.forum.BackColor = Color.AliceBlue;
			this.forum.ForeColor = Color.SteelBlue;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000023B1 File Offset: 0x000007B1
		private void BottomLeftLabel_MouseDown(object sender, MouseEventArgs e)
		{
			this.forum.BackColor = Color.LightBlue;
			this.forum.ForeColor = Color.White;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004258 File Offset: 0x00002658
		private void BottomLeftLabel_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[1];
			if (flag)
			{
				this.forum.BackColor = Color.SkyBlue;
				this.forum.ForeColor = Color.White;
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x000023D6 File Offset: 0x000007D6
		private void BottomRightLabel_MouseEnter(object sender, EventArgs e)
		{
			this.focus[2] = true;
			this.BottomRightLabel.BackColor = Color.SkyBlue;
			this.BottomRightLabel.ForeColor = Color.White;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00002404 File Offset: 0x00000804
		private void BottomRightLabel_MouseLeave(object sender, EventArgs e)
		{
			this.focus[2] = false;
			this.BottomRightLabel.BackColor = Color.Transparent;
			this.BottomRightLabel.ForeColor = Color.Transparent;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00002432 File Offset: 0x00000832
		private void BottomRightLabel_MouseDown(object sender, MouseEventArgs e)
		{
			this.BottomRightLabel.BackColor = Color.LightBlue;
			this.BottomRightLabel.ForeColor = Color.White;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00004298 File Offset: 0x00002698
		private void BottomRightLabel_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.focus[2];
			if (flag)
			{
				this.BottomRightLabel.BackColor = Color.SkyBlue;
				this.BottomRightLabel.ForeColor = Color.White;
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000042D8 File Offset: 0x000026D8
		private void StartPatch(string host, string[] filelist, int finalindex = -1)
		{
			bool flag = this.BlockUnauthorizedPrograms(false);
			if (!flag)
			{
				this.SetProgressBar(0, 0);
				this.SetProgressBar(1, 0);
				this.sHost = host;
				this.sFileList = filelist;
				this.sCurrentFileIndex = 0;
				bool flag2 = finalindex == -1;
				if (flag2)
				{
					for (int i = 0; i < filelist.Length; i++)
					{
						bool flag3 = filelist[i].Length > 0;
						if (flag3)
						{
							finalindex++;
						}
					}
				}
				this.sFinalIndex = finalindex;
				Debug.Print("finalindex = " + finalindex.ToString());
				this.sUrlToReadFileFrom = this.sHost + "/" + this.sFileList[this.sCurrentFileIndex].Replace('\\', '/');
				this.sFilePathToWriteFileTo = Path.Combine(this.GetGamePath() + "/models/ ", this.sFileList[this.sCurrentFileIndex]);
				this.PercentageLabel.Text = "Downloading: " + this.sFileList[this.sCurrentFileIndex];
				this.sMode = "Patch";
				this.DownloadWorker.RunWorkerAsync();
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000043FC File Offset: 0x000027FC
		private void StartUpdate()
		{
			Uri requestUri = new Uri(Program.LauncherURL + "/getfilehash.php?name=" + Program.LauncherFileName);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			HttpStatusCode statusCode = httpWebResponse.StatusCode;
			string text = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd().ToUpper();
			httpWebResponse.Close();
			bool flag = statusCode != HttpStatusCode.OK || text == string.Empty;
			if (flag)
			{
				this.PercentageLabel.Text = "Please download the latest version of the launcher.";
				this.forum.Text = "forum";
				this.SetBottomLeftLabelFunction(Program.InfowebURL);
				this.ShowBottomLabels(2);
				base.TopMost = true;
				base.TopMost = false;
			}
			else
			{
				string strB = this.GetMD5OfFile(Application.ExecutablePath).ToUpper();
				int num = string.Compare(Application.ExecutablePath, Path.Combine(this.GetGamePath(), Program.LauncherFileName), true);
				bool flag2 = string.Compare(text, strB, true) == 0 && num == 0;
				if (flag2)
				{
					this.GameStart.Start();
				}
				else
				{
					this.SetProgressBar(0, 0);
					this.SetProgressBar(1, 50);
					bool flag3 = !Directory.Exists(Program.Path_Setup);
					if (flag3)
					{
						Directory.CreateDirectory(Program.Path_Setup);
					}
					this.sUrlToReadFileFrom = Program.LauncherURL + "/" + Program.LauncherFileName;
					this.sFilePathToWriteFileTo = Path.Combine(this.GetGamePath(), (num == 0) ? Program.UpdaterFileName : Program.LauncherFileName);
					this.NewLauncherPath = this.sFilePathToWriteFileTo;
					bool flag4 = File.Exists(this.sFilePathToWriteFileTo);
					if (flag4)
					{
						File.Delete(this.sFilePathToWriteFileTo);
					}
					this.PercentageLabel.Text = "Se instaleaza ultimele actualizari..";
					this.sMode = "Update";
					this.DownloadWorker.RunWorkerAsync();
				}
			}
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000463C File Offset: 0x00002A3C
		private string GetNewLauncherPath()
		{
			return this.NewLauncherPath;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00004654 File Offset: 0x00002A54
		private bool SetProgressBar(int type, int percentage)
		{
			Panel panel;
			Panel panel2;
			if (type != 0)
			{
				if (type != 1)
				{
					return false;
				}
				panel = this.TotalProgressBar;
				panel2 = this.TotalProgressBar_Background;
			}
			else
			{
				panel = this.UnitProgressBar;
				panel2 = this.UnitProgressBar_Background;
			}
			panel.Width = (panel2.Width - 2) / 100 * percentage;
			panel.Left = panel2.Width / 2 - panel.Width / 2;
			return true;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000046CC File Offset: 0x00002ACC
		private void DownloadWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			bool flag = string.Compare(this.sMode, "Patch") == 0;
			if (flag)
			{
				Debug.Print("Patching:[" + this.sCurrentFileIndex.ToString() + "] " + this.sUrlToReadFileFrom);
			}
			Uri requestUri = new Uri(this.sUrlToReadFileFrom);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			long contentLength = httpWebResponse.ContentLength;
			httpWebResponse.Close();
			long num = 0L;
			WebClient webClient = new WebClient();
			Stream stream = webClient.OpenRead(new Uri(this.sUrlToReadFileFrom));
			Stream stream2 = new FileStream(this.sFilePathToWriteFileTo, FileMode.Create, FileAccess.Write, FileShare.None);
			byte[] array = new byte[contentLength];
			int num2;
			while ((num2 = stream.Read(array, 0, array.Length)) > 0)
			{
				stream2.Write(array, 0, num2);
				num += (long)num2;
				double num3 = (double)num;
				double num4 = (double)array.Length;
				double num5 = num3 / num4;
				int percentProgress = (int)(num5 * 100.0);
				this.DownloadWorker.ReportProgress(percentProgress);
			}
			stream2.Close();
			stream.Close();
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00002457 File Offset: 0x00000857
		private void DownloadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.SetProgressBar(0, e.ProgressPercentage);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000048F8 File Offset: 0x00002CF8
		private void DownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			bool flag = string.Compare(this.sMode, "Patch") == 0;
			if (flag)
			{
				bool flag2 = !e.Cancelled && this.sCurrentFileIndex < this.sFinalIndex;
				if (flag2)
				{
					this.sCurrentFileIndex++;
					this.sUrlToReadFileFrom = this.sHost + "/" + this.sFileList[this.sCurrentFileIndex].Replace('\\', '/');
					this.sFilePathToWriteFileTo = Path.Combine(this.GetGamePath() + "/models/", this.sFileList[this.sCurrentFileIndex]);
					this.SetProgressBar(1, (this.sCurrentFileIndex - 1) * 100 / this.sFinalIndex);
					this.PercentageLabel.Text = "Downloading: " + this.sFileList[this.sCurrentFileIndex];
					this.DownloadWorker.RunWorkerAsync();
				}
				else
				{
					bool flag3 = this.sCurrentFileIndex >= this.sFinalIndex;
					if (flag3)
					{
						this.GameStart.Start();
					}
					else
					{
						this.PercentageLabel.Text = " The patch has been canceled";
						base.TopMost = true;
						base.TopMost = false;
					}
				}
			}
			else
			{
				bool flag4 = string.Compare(this.sMode, "Update") == 0;
				if (flag4)
				{
					bool cancelled = e.Cancelled;
					if (cancelled)
					{
						this.PercentageLabel.Text = " Update was canceled";
						base.TopMost = true;
						base.TopMost = false;
					}
					else
					{
						this.GameStart.Start();
					}
				}
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00004A90 File Offset: 0x00002E90
		private bool CompareMD5OfGameFile(string listurl)
		{
			Uri requestUri = new Uri(listurl);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			string[] array = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd().Split(new char[]
			{
				'|'
			});
			httpWebResponse.Close();
			for (int i = 0; i < this.DissimilarFiles.Length; i++)
			{
				this.DissimilarFiles[i] = string.Empty;
			}
			int num = 0;
			for (int j = 0; j < array.Length; j++)
			{
				string[] array2 = array[j].Split(new char[]
				{
					','
				});
				bool flag = this.GetMD5OfFile(Path.Combine(this.GetGamePath() + "/models/", array2[0])).ToUpper() != array2[1].ToUpper();
				if (flag)
				{
					Debug.Print("<DF> " + array2[0] + ": " + this.GetMD5OfFile(Path.Combine(this.GetGamePath() + "/models/", array2[0])).ToUpper());
					this.DissimilarFiles[num] = array2[0];
					num++;
				}
			}
			return num == 0;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00004C1C File Offset: 0x0000301C
		private string[] GetDissimilarFiles()
		{
			return this.DissimilarFiles;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004C34 File Offset: 0x00003034
		private int GetNumberOfDissimilarFiles()
		{
			int num = 0;
			for (int i = 0; i < this.DissimilarFiles.Length; i++)
			{
				bool flag = this.DissimilarFiles[i] != null && this.DissimilarFiles[i].Length > 0;
				if (flag)
				{
					num++;
				}
			}
			return num;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00004C88 File Offset: 0x00003088
		private bool BlockUnauthorizedPrograms(bool blockdissimilarfiles = false)
		{
			bool flag = false;
			bool flag2 = this.AntiAUF();
			if (flag2)
			{
				flag = true;
			}
			bool flag3 = this.GetNumberOfDissimilarFiles() > 0 && blockdissimilarfiles;
			if (flag3)
			{
				flag = true;
				this.alert("A aparut o eroare, te rog sa iesi si sa intri iara in aplicatie.", true);
				for (int i = 0; i < this.GetNumberOfDissimilarFiles(); i++)
				{
					Debug.Print(string.Concat(new object[]
					{
						"DissimilarFiles[",
						i,
						"]: ",
						this.GetDissimilarFiles()[i]
					}));
				}
			}
			bool flag4 = flag;
			bool result;
			if (flag4)
			{
				this.KillGameProcess();
				this.ShowBottomLabels(1);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00004D34 File Offset: 0x00003134
		private bool AntiAUF()
		{
			string gamePath = this.GetGamePath();
			string[] unauthorizedFiles = this.GetUnauthorizedFiles(gamePath);
			Debug.Print("AUF: " + unauthorizedFiles[0]);
			Debug.Print("DIR: " + unauthorizedFiles[1]);
			string[] array = unauthorizedFiles[0].Split(new char[]
			{
				'|'
			});
			string[] array2 = unauthorizedFiles[1].Split(new char[]
			{
				'|'
			});
			bool flag = unauthorizedFiles[0].Length > 0 && array.Length != 0;
			bool result;
			if (flag)
			{
				this.KillGameProcess();
				string text = "Fisierele au fost mutate:\n";
				string text2 = Path.Combine(Program.Path_UAF, DateTime.Now.ToString("yyMMdd HHmmss"));
				for (int i = 0; i < array.Length; i++)
				{
					string text3 = array[i].Split(new char[]
					{
						','
					})[0];
					string text4 = Path.Combine(array2[i], text3);
					string path = Regex.Replace(array2[i].Substring(gamePath.Length), "^\\\\", "");
					string text5 = Path.Combine(text2, path, text3);
					text = text + text4 + "\n";
					bool flag2 = !Directory.Exists(Path.GetDirectoryName(text5));
					if (flag2)
					{
						Directory.CreateDirectory(Path.GetDirectoryName(text5));
					}
					File.Move(text4, text5);
				}
				this.PercentageLabel.Text = "Au fost detectate fisiere neautorizate!";
				this.forum.Text = "Fisiere neautorizate!";
				this.SetBottomLeftLabelFunction(text2);
				this.ShowBottomLabels(2);
				base.TopMost = true;
				base.TopMost = false;
				MessageBox.Show(text, " Guide to Unauthorized File Movement", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00004F10 File Offset: 0x00003310
		private string GetAuthorizedFilesFromServer(string listurl)
		{
			string text = "";
			Uri requestUri = new Uri(listurl);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			text = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();
			bool flag = httpWebResponse.StatusCode != HttpStatusCode.OK;
			if (flag)
			{
				this.alert(" Failed to get ACL information.", false);
			}
			else
			{
				httpWebResponse.Close();
			}
			this.AuthorizedFiles = text;
			Debug.Print(" Allowed Cleo: " + text);
			return text;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00004FD0 File Offset: 0x000033D0
		private string GetAuthorizedFiles()
		{
			return this.AuthorizedFiles;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00004FE8 File Offset: 0x000033E8
		private string[] GetUnauthorizedFiles(string path)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string authorizedFiles = this.GetAuthorizedFiles();
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			foreach (DirectoryInfo directoryInfo2 in directoryInfo.GetDirectories())
			{
				string[] unauthorizedFiles = this.GetUnauthorizedFiles(directoryInfo2.FullName);
				bool flag = unauthorizedFiles[0].Length > 0;
				if (flag)
				{
					text = text + unauthorizedFiles[0] + "|";
				}
				bool flag2 = unauthorizedFiles[1].Length > 0;
				if (flag2)
				{
					text2 = text2 + unauthorizedFiles[1] + "|";
				}
			}
			foreach (FileInfo fileInfo in directoryInfo.GetFiles())
			{
				foreach (string text3 in this.AllowedExtension)
				{
					bool flag3 = fileInfo.Extension.ToUpper() == text3.ToUpper();
					if (flag3)
					{
						string text4 = this.GetMD5OfFile(fileInfo.FullName).ToUpper();
						bool flag4 = !authorizedFiles.ToUpper().Contains("," + text4);
						if (flag4)
						{
							text = string.Concat(new string[]
							{
								text,
								fileInfo.Name,
								",",
								text4,
								"|"
							});
							text2 = text2 + fileInfo.Directory + "|";
						}
					}
				}
			}
			bool flag5 = text.Length > 0;
			if (flag5)
			{
				text = text.Substring(0, text.Length - 1);
			}
			bool flag6 = text2.Length > 0;
			if (flag6)
			{
				text2 = text2.Substring(0, text2.Length - 1);
			}
			return new string[]
			{
				text,
				text2
			};
		}

		// Token: 0x06000068 RID: 104 RVA: 0x000051C4 File Offset: 0x000035C4
		private string GetGamePath()
		{
			string path = string.Empty;
			string text = string.Empty;
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\SAMP");
			path = registryKey.GetValue("gta_sa_exe").ToString();
			text = Path.GetDirectoryName(path);
			bool flag = !File.Exists(path);
			if (flag)
			{
				text = this.ShowGamePathDialog();
			}
			bool flag2 = text.Length == 0;
			if (flag2)
			{
				this.KillGameProcess();
				Application.Exit();
			}
			return text;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00005260 File Offset: 0x00003660
		private string ShowGamePathDialog()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Please specify the GTA San Andreas executable file.";
			openFileDialog.Filter = "GTA San Andreas (gta_sa.exe)|gta_sa.exe";
			bool flag = openFileDialog.ShowDialog() == DialogResult.Cancel;
			string result;
			if (flag)
			{
				this.KillGameProcess();
				Application.Exit();
				result = string.Empty;
			}
			else
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\SAMP", RegistryKeyPermissionCheck.ReadWriteSubTree);
				registryKey.SetValue("gta_sa_exe", openFileDialog.FileName);
				result = Regex.Split(openFileDialog.FileName, "gta_sa.exe")[0];
			}
			return result;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x000052E8 File Offset: 0x000036E8
		private string GetMD5OfFile(string filepath)
		{
			bool flag = File.Exists(filepath);
			string result;
			if (flag)
			{
				FileStream inputStream = File.OpenRead(filepath);
				MD5CryptoServiceProvider md5CryptoServiceProvider = new MD5CryptoServiceProvider();
				result = string.Join("", from i in md5CryptoServiceProvider.ComputeHash(inputStream).ToArray<byte>()
										 select i.ToString("X2"));
			}
			else
			{
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002468 File Offset: 0x00000868
		private void KillGameProcess()
		{
			this.KillProcessesByName("samp");
			this.KillProcessesByName("gta_sa");
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00005388 File Offset: 0x00003788
		private string GetCurrentProcessName()
		{
			IntPtr hWnd = IntPtr.Zero;
			uint processId = 0U;
			hWnd = frmMain.GetForegroundWindow();
			frmMain.GetWindowThreadProcessId(hWnd, out processId);
			return Process.GetProcessById((int)processId).ProcessName;
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000053C4 File Offset: 0x000037C4
		private bool IsProcessRunning(string pname)
		{
			return Process.GetProcessesByName(pname).Length != 0;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x000053EC File Offset: 0x000037EC
		private void KillProcessesByName(string pname)
		{
			foreach (Process process in Process.GetProcessesByName(pname))
			{
				process.Kill();
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0000541C File Offset: 0x0000381C
		private void alert(string message, bool showMessageBox)
		{
			this.KillGameProcess();
			if (showMessageBox)
			{
				MessageBox.Show(message, "notice", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				this.PercentageLabel.Text = message;
			}
			base.TopMost = true;
			base.TopMost = false;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00005468 File Offset: 0x00003868
		private void CreateUrlSchemeRegistry()
		{
			RegistryKey registryKey = Registry.ClassesRoot.CreateSubKey("LARP", RegistryKeyPermissionCheck.ReadWriteSubTree);
			registryKey.SetValue("", "GreenStone Launcher");
			registryKey.SetValue("Url Protocol", "");
			registryKey = registryKey.CreateSubKey("shell\\open\\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
			registryKey.SetValue("", "\"" + Path.Combine(this.GetGamePath(), Program.LauncherFileName) + "\" \"%1\"");
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000020B2 File Offset: 0x000004B2
		private void HeadLabel_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000054E4 File Offset: 0x000038E4
		private string GetEncryptKey()
		{
			return "LARP2009";
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00002483 File Offset: 0x00000883
		private void Label7_Click(object sender, EventArgs e)
		{
			Process.Start("https://www.facebook.com/gstonero");
		}

		// Token: 0x06000074 RID: 116 RVA: 0x000020B2 File Offset: 0x000004B2
		private void FrmMain_Load(object sender, EventArgs e)
		{
		}

		// Token: 0x06000075 RID: 117 RVA: 0x000020B2 File Offset: 0x000004B2
		private void Label5_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00002491 File Offset: 0x00000891
		private void Label6_Click(object sender, EventArgs e)
		{
			Process.Start("https://www.youtube.com/greenstoneromania");
		}

		// Token: 0x06000077 RID: 119 RVA: 0x000020B2 File Offset: 0x000004B2
		private void PatchPanel1_Paint(object sender, PaintEventArgs e)
		{
		}

		// Token: 0x06000078 RID: 120 RVA: 0x000020B2 File Offset: 0x000004B2
		private void PatchPanel3_Paint(object sender, PaintEventArgs e)
		{
		}

		// Token: 0x06000079 RID: 121 RVA: 0x000020B2 File Offset: 0x000004B2
		private void PatchPanel2_Paint(object sender, PaintEventArgs e)
		{
		}

		// Token: 0x0600007A RID: 122 RVA: 0x000020B2 File Offset: 0x000004B2
		private void Staus_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x0600007B RID: 123 RVA: 0x000020B2 File Offset: 0x000004B2
		private void Label4_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x0600007C RID: 124 RVA: 0x000020B2 File Offset: 0x000004B2
		private void LeftPanel_Paint(object sender, PaintEventArgs e)
		{
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000054FC File Offset: 0x000038FC
		private string AESEncrypt256(string InputText, string Key)
		{
			string s = "LARP" + InputText + "SEONGBUM";
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			byte[] bytes = Encoding.Unicode.GetBytes(s);
			byte[] bytes2 = Encoding.ASCII.GetBytes(Key.Length.ToString());
			PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Key, bytes2);
			ICryptoTransform transform = rijndaelManaged.CreateEncryptor(passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			byte[] inArray = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			return Convert.ToBase64String(inArray);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x0000249F File Offset: 0x0000089F
		private void minimizePictureBox_Click(object sender, EventArgs e)
		{
			base.WindowState = FormWindowState.Minimized;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000055B8 File Offset: 0x000039B8
		private string AESDecrypt256(string InputText, string Key)
		{
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			byte[] array = Convert.FromBase64String(InputText);
			byte[] bytes = Encoding.ASCII.GetBytes(Key.Length.ToString());
			PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Key, bytes);
			ICryptoTransform transform = rijndaelManaged.CreateDecryptor(passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
			MemoryStream memoryStream = new MemoryStream(array);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
			byte[] array2 = new byte[array.Length];
			int count = cryptoStream.Read(array2, 0, array2.Length);
			memoryStream.Close();
			cryptoStream.Close();
			string @string = Encoding.Unicode.GetString(array2, 0, count);
			return @string.Substring(4, @string.Length - 12);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00005678 File Offset: 0x00003A78
		protected override void Dispose(bool disposing)
		{
			bool flag = disposing && this.components != null;
			if (flag)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000056B0 File Offset: 0x00003AB0
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// frmMain
			// 
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Name = "frmMain";
			this.Load += new System.EventHandler(this.FrmMain_Load);
			this.ResumeLayout(false);

		}

		// Token: 0x04000010 RID: 16
		private List<PatchNoteBlock> patchNoteBlocks = new List<PatchNoteBlock>();

		// Token: 0x04000012 RID: 18
		private int InitLevel = 0;

		// Token: 0x04000013 RID: 19
		private int ExitLevel = 0;

		// Token: 0x04000014 RID: 20
		private string username;

		// Token: 0x04000015 RID: 21
		private Point MouseLocation;

		// Token: 0x04000016 RID: 22
		private bool[] focus = new bool[3];

		// Token: 0x04000017 RID: 23
		private string BottomLeftLabelFunction = string.Empty;

		// Token: 0x04000018 RID: 24
		private string sMode;

		// Token: 0x04000019 RID: 25
		private string[] sFileList;

		// Token: 0x0400001A RID: 26
		private string sHost;

		// Token: 0x0400001B RID: 27
		private string sUrlToReadFileFrom;

		// Token: 0x0400001C RID: 28
		private string sFilePathToWriteFileTo;

		// Token: 0x0400001D RID: 29
		private int sCurrentFileIndex;

		// Token: 0x0400001E RID: 30
		private int sFinalIndex;

		// Token: 0x0400001F RID: 31
		private string NewLauncherPath = string.Empty;

		// Token: 0x04000020 RID: 32
		private string[] DissimilarFiles = new string[200];

		// Token: 0x04000021 RID: 33
		private string[] AllowedExtension = new string[]
		{
			".cleo",
			".asi",
			".cs",
			".dll"
		};

		// Token: 0x04000022 RID: 34
		private string AuthorizedFiles = string.Empty;

		// Token: 0x04000023 RID: 35
		private IContainer components = null;

		// Token: 0x04000024 RID: 36
		private Panel UnitProgressBar_Background;

		// Token: 0x04000025 RID: 37
		private Panel UnitProgressBar;

		// Token: 0x04000026 RID: 38
		private Label PercentageLabel;

		// Token: 0x04000027 RID: 39
		private Panel TotalProgressBar_Background;

		// Token: 0x04000028 RID: 40
		private Panel TopPanel;

		// Token: 0x04000029 RID: 41
		private Panel LeftPanel;

		// Token: 0x0400002A RID: 42
		private Panel RightPanel;

		// Token: 0x0400002B RID: 43
		private Panel panel1;

		// Token: 0x0400002C RID: 44
		private Label HeadLabel;

		// Token: 0x0400002D RID: 45
		private Panel TotalProgressBar;

		// Token: 0x0400002E RID: 46
		private BackgroundWorker AntiCleoWorker;

		// Token: 0x0400002F RID: 47
		private BackgroundWorker DownloadWorker;

		// Token: 0x04000030 RID: 48
		private Timer GameStart;

		// Token: 0x04000031 RID: 49
		private Timer GameExit;

		// Token: 0x04000032 RID: 50
		private Label BottomRightLabel;

		// Token: 0x04000033 RID: 51
		private Label label291;

		// Token: 0x04000034 RID: 52
		private Label label292;

		// Token: 0x04000035 RID: 53
		private TextBox tbNick;

		// Token: 0x04000036 RID: 54
		private Label currentVersionLabel;

		// Token: 0x04000037 RID: 55
		private PictureBox minimizePictureBox;

		// Token: 0x04000038 RID: 56
		private Label discord;

		// Token: 0x04000039 RID: 57
		private Panel patchPanel1;

		// Token: 0x0400003A RID: 58
		private Label patchTitle1;

		// Token: 0x0400003B RID: 59
		private Label patchText1;

		// Token: 0x0400003C RID: 60
		private Panel patchPanel2;

		// Token: 0x0400003D RID: 61
		private Label patchText2;

		// Token: 0x0400003E RID: 62
		private Label patchTitle2;

		// Token: 0x0400003F RID: 63
		private Panel patchPanel3;

		// Token: 0x04000040 RID: 64
		private Label patchText3;

		// Token: 0x04000041 RID: 65
		private Label patchTitle3;

		// Token: 0x04000042 RID: 66
		private Label label40;

		// Token: 0x04000043 RID: 67
		private Label players;

		// Token: 0x04000044 RID: 68
		private Label staus;

		// Token: 0x04000045 RID: 69
		private Label label4;

		// Token: 0x04000046 RID: 70
		private Button button7;

		// Token: 0x04000047 RID: 71
		private Label forum;

		// Token: 0x04000048 RID: 72
		private Label panel;

		// Token: 0x04000049 RID: 73
		private Label label1;

		// Token: 0x0400004A RID: 74
		private Label label2;

		// Token: 0x0400004B RID: 75
		private Label label3;

		// Token: 0x0400004C RID: 76
		private Label label5;

		// Token: 0x0400004D RID: 77
		private Label label6;

		// Token: 0x0400004E RID: 78
		private Label label7;


	}
}

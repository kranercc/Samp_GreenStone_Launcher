using System;
using System.IO;
using System.Windows.Forms;

namespace Samp_GreenStone_Launcher
{
	// Token: 0x02000007 RID: 7
	internal static class Program
	{
		// Token: 0x06000085 RID: 133 RVA: 0x000024CD File Offset: 0x000008CD
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new frmMain(args));
		}

		// Token: 0x04000051 RID: 81
		public static bool TestMode = false;

		// Token: 0x04000052 RID: 82
		public static string InfowebURL = "https://g-stone.ro";

		// Token: 0x04000053 RID: 83
		public static string LauncherURL = "https://panel.g-stone.ro";

		// Token: 0x04000054 RID: 84
		public static string CheckURL = "https://panel.g-stone.ro";

		// Token: 0x04000055 RID: 85
		public static string LauncherFileName = "gstone.exe";

		// Token: 0x04000056 RID: 86
		public static string UpdaterFileName = "gstoneupdater.exe";

		// Token: 0x04000057 RID: 87
		public static readonly string PATCH_NOTES_URL = "https://panel.g-stone.ro/updates.xml";

		// Token: 0x04000058 RID: 88
		public static readonly string VERSION_URL = "https://panel.g-stone.ro/version.txt";

		// Token: 0x04000059 RID: 89
		public static string Path_LARP = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "gstone");

		// Token: 0x0400005A RID: 90
		public static string Path_ChatLog = Path.Combine(Program.Path_LARP, "Chatlog");

		// Token: 0x0400005B RID: 91
		public static string Path_UAF = Path.Combine(Program.Path_LARP, "Unauthorized file");

		// Token: 0x0400005C RID: 92
		public static string Path_Setup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "");

		// Token: 0x0400005D RID: 93
		public static bool SHOW_ERROR_BOX_IF_PATCH_NOTES_DOWNLOAD_FAILS = true;

		// Token: 0x0400005E RID: 94
		public static bool SHOW_VERSION_TEXT = true;

		// Token: 0x0400005F RID: 95
		public static readonly string VersionText = "2.1.5";
	}
}

using System;

namespace Samp_GreenStone_Launcher
{
	// Token: 0x02000004 RID: 4
	public class PatchNoteBlock
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002000 File Offset: 0x00000400
		// (set) Token: 0x0600000D RID: 13 RVA: 0x00002008 File Offset: 0x00000408
		public string Title { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000E RID: 14 RVA: 0x00002011 File Offset: 0x00000411
		// (set) Token: 0x0600000F RID: 15 RVA: 0x00002019 File Offset: 0x00000419
		public string Text { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002022 File Offset: 0x00000422
		// (set) Token: 0x06000011 RID: 17 RVA: 0x0000202A File Offset: 0x0000042A
		public string Link { get; set; }
	}
}

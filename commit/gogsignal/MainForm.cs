/*
 * Created by SharpDevelop.
 * User: manager
 * Date: 04.06.2013
 * Time: 12:40
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Globalization;
using System.Web;


namespace gogsignal
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		
		
		
		public static Icon GetIcon(string text)
		{
			//Create bitmap, kind of canvas
			Bitmap bitmap = new Bitmap(32, 32);

			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));

			//  Icon icon = new Icon(@"tf2_2.ico");
			Icon icon = (Icon)resources.GetObject("notifyIcon1.Icon");
			System.Drawing.Font drawFont = new System.Drawing.Font("Tahoma", 16, FontStyle.Bold);
			
			
			System.Drawing.SolidBrush drawBrush;

			if(text != "24")
			{
				drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
			}
			else
			{
				drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow);
				
			}
			
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);

			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
			graphics.DrawIcon(icon, 0, 0);
			graphics.DrawString(text, drawFont, drawBrush, 1, 2);

			//To Save icon to disk
			//bitmap.Save("icon.ico", System.Drawing.Imaging.ImageFormat.Icon);

			Icon createdIcon = Icon.FromHandle(bitmap.GetHicon());

			drawFont.Dispose();
			drawBrush.Dispose();
			graphics.Dispose();
			bitmap.Dispose();

			return createdIcon;
		}
		
		
		
		public  Icon[] theIcons;
		
		
		void generateIcons()
		{
			theIcons = new Icon[60];
			
			theIcons[0] = GetIcon("");
			
			for(int i=1;i<60;i++)
			{
				theIcons[i] = GetIcon(""+i);
			}
			
		}
		
		string mapstring="";
		
		bool ServerWasEmpty = true;
		bool wasMap=false;
		List<string> playerNames;
		private  void updateIcon()
		{

			try
			{
				WebClient client = new WebClient();
				byte[] arr = client.DownloadData("http://thegrumpyoldgits.hlstatsx.com/status/");
				//	byte[] arr = client.DownloadData("http://eclub.gameme.com/status");

				string tempstr="";
				
				int playerNum=0;
				
				playerNames = new List<string>();

				for(int i=0;i<arr.Length;i++)
				{
					tempstr+=Convert.ToChar(arr[i]);
					if(Convert.ToChar(arr[i])=='\n')
					{
						if(tempstr.Contains("srv_empty") || tempstr.Contains("srv_nempty") || tempstr.Contains("srv_full"))
						{
							string[] substrs = tempstr.Split('>');

							string numstring =substrs[3];

							string[] numsubstrs = numstring.Split('/');

							string thenumber = numsubstrs[0];

							/*if(thenumber != "0")
							{
								notifyIcon1.Icon = GetIcon(thenumber);
							}
							else
							{
								notifyIcon1.Icon = GetIcon("24");
							}*/
							
							playerNum = Convert.ToInt32(thenumber);
							
							notifyIcon1.Icon = theIcons[playerNum];

						}
						if(tempstr.Contains("Map"))
						{
							wasMap=true;
						}
						else
						{
							if(wasMap==true)
							{
								wasMap=false;
								
								string[] substrs = tempstr.Split('>');

								mapstring =substrs[2];
								
								mapstring = mapstring.Substring(0, mapstring.LastIndexOf("</"));
								
								labelMap.Text = "Current Map: "+mapstring;
								
								System.Diagnostics.Debug.WriteLine(mapstring);
							}
						}
						
						
						if(tempstr.Contains("/playerinfo/"))
						{
							
							string[] substrs = tempstr.Split('>');

							string playerstring =substrs[6];
							
							try
							{
								playerstring = playerstring.Substring(0, playerstring.LastIndexOf("</"));
								
								playerstring = HttpUtility.HtmlDecode(playerstring);
								
								//System.Diagnostics.Debug.WriteLine(playerstring);
								playerNames.Add(playerstring);
								
								
								
								
							}
							catch{}
							
						}
						tempstr="";
					}
				}
				
				if(ServerWasEmpty==true && playerNum>0)
				{
					notifyIcon1.ShowBalloonTip(1000, "The server is no longer empty!", playerNames[0]+" has joined the server!", ToolTipIcon.Info);
					
				}
				
				if(playerNum==0)
				{
					ServerWasEmpty=true;
					notifyIcon1.Text="The server is empty.";
				}
				else
				{
					ServerWasEmpty=false;
					string ttText = "Map: "+mapstring+"\n\n";
					
					foreach(string str in playerNames)
					{
						ttText+=str+"\n";
					}
					
					if(ttText.Length>64)ttText=ttText.Substring(0,60)+"...";
					
					notifyIcon1.Text=ttText;
				}
				
				
				string[] row = { "","","" };
				int pi =0;
				listView1.Items.Clear();
				ListViewItem listViewItem;
				
				for(int i=0;i<playerNames.Count;i++)
				{
					
					
					row[pi]=playerNames[i];
					pi++;
					
					if(pi==3)
					{
						listViewItem = new ListViewItem(row);
						listView1.Items.Add(listViewItem);
						row[0]="";
						row[1]="";
						row[2]="";
						pi=0;
						
						System.Diagnostics.Debug.WriteLine("Adding a new row");
					}
				}

				listViewItem = new ListViewItem(row);
				listView1.Items.Add(listViewItem);



			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
		}
		
		
		public static bool showTips=true;
		
		public MainForm()
		{
			generateIcons();
			InitializeComponent();
			updateIcon();
		}
		
		void NotifyIcon1MouseClick(object sender, MouseEventArgs e)
		{
			
			this.Show();
			this.BringToFront();//MainForm.BringToFront();
			this.Activate();
			this.WindowState = FormWindowState.Normal;
			
			int screenHeight = Screen.GetWorkingArea(Cursor.Position).Height;
			int screenWidth = Screen.GetWorkingArea(Cursor.Position).Width;
			
			int validHeight = screenHeight - this.Height;
			int validWidth  = screenWidth - this.Width;
			
			int newX = Cursor.Position.X;
			int newY = Cursor.Position.Y;
			
			System.Diagnostics.Debug.WriteLine(newX+" "+newY);
			
			if(newX>validWidth)newX=validWidth;
			if(newY>validHeight)newY=validHeight;
			
			//	if(newX<1)newX=1;
			//	if(newY<1)newY=1;
			
			
			
			
			this.SetDesktopLocation(newX, newY);
			
			
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			this.Hide();
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			this.Close();
		}
		
		void Timer1Tick(object sender, EventArgs e)
		{
			updateIcon();
		}
		

		
		void MainFormShown(object sender, EventArgs e)
		{
			this.Hide();
		}
	}
}

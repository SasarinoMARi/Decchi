using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ParsingModule;

namespace Decchi
{

	public class ClientSelector
	{
		private ClientSelectWindow window;

		public ClientSelector( List<SongInfo> songs )
		{
			window = new ClientSelectWindow( songs );
			window.FormClosed += Window_FormClosed;
		}

		public void ShowDialog( Action<SongInfo> callback )
		{
			window.AddButtonClickListner( callback );
			window.ShowDialog( );
		}

		private void Window_FormClosed( object sender, FormClosedEventArgs e )
		{
			window.Dispose( );
		}

		private partial class ClientSelectWindow : Form
		{
			#region designer

			/// <summary>
			/// Required designer variable.
			/// </summary>
			private System.ComponentModel.IContainer components = null;

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
			protected override void Dispose( bool disposing )
			{
				if ( disposing && ( components != null ) )
				{
					components.Dispose( );
				}
				base.Dispose( disposing );
			}

			#region Windows Form Designer generated code

			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			private void InitializeComponent( )
			{
				this.listview_Clients = new System.Windows.Forms.ListView( );
				this.btn_Summit = new System.Windows.Forms.Button( );
				this.SuspendLayout( );
				// 
				// listview_Clients
				// 
				this.listview_Clients.Anchor = ( ( System.Windows.Forms.AnchorStyles ) ( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
				| System.Windows.Forms.AnchorStyles.Left )
				| System.Windows.Forms.AnchorStyles.Right ) ) );
				this.listview_Clients.Location = new System.Drawing.Point( 12, 12 );
				this.listview_Clients.Name = "listview_Clients";
				this.listview_Clients.Size = new System.Drawing.Size( 360, 276 );
				this.listview_Clients.TabIndex = 0;
				this.listview_Clients.UseCompatibleStateImageBehavior = false;
				this.listview_Clients.MultiSelect = false;
				this.listview_Clients.FullRowSelect = true;
				this.listview_Clients.GridLines = true;
				this.listview_Clients.View = View.List;
				// 
				// btn_Summit
				// 
				this.btn_Summit.Anchor = ( ( System.Windows.Forms.AnchorStyles ) ( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
				| System.Windows.Forms.AnchorStyles.Right ) ) );
				this.btn_Summit.Font = new System.Drawing.Font( "Malgun Gothic", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte ) ( 129 ) ) );
				this.btn_Summit.Location = new System.Drawing.Point( 12, 294 );
				this.btn_Summit.Name = "btn_Summit";
				this.btn_Summit.Size = new System.Drawing.Size( 360, 56 );
				this.btn_Summit.TabIndex = 1;
				this.btn_Summit.Text = "뎃찌!";
				this.btn_Summit.UseVisualStyleBackColor = true;
				this.btn_Summit.Click += Btn_Summit_Click;
				// 
				// ClientSelect
				// 
				this.AutoScaleDimensions = new System.Drawing.SizeF( 7F, 12F );
				this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				this.ClientSize = new System.Drawing.Size( 384, 362 );
				this.Controls.Add( this.btn_Summit );
				this.Controls.Add( this.listview_Clients );
				this.Name = "ClientSelect";
				this.Text = "ClientSelect";
				this.ResumeLayout( false );

			}

			#endregion

			private System.Windows.Forms.ListView listview_Clients;
			private System.Windows.Forms.Button btn_Summit;

			#endregion

			private Dictionary<ListViewItem, SongInfo> items = new Dictionary<ListViewItem, SongInfo>();
			private Action<SongInfo> callback;

			public ClientSelectWindow( List<SongInfo> songs )
			{
				InitializeComponent( );

				foreach ( var song in songs )
				{
					if ( song.Loaded == false ) continue;
					var item = new ListViewItem(song.ToString());
					listview_Clients.Items.Add( item );
					items.Add( item, song );
				}
			}

			public void AddButtonClickListner( Action<SongInfo> callback )
			{
				this.callback = callback;
			}

			private void Btn_Summit_Click( object sender, EventArgs e )
			{
				if(this.callback != null && listview_Clients.SelectedItems.Count == 1)
				{
					callback( items[listview_Clients.SelectedItems[0]] );
				}
				this.Close( );
			}
		}
	}

}

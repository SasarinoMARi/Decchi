namespace Decchi
{
	partial class Main
	{
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
			this.picbox_profileImage = new RoundedPicturebox();
			this.label_UserName = new System.Windows.Forms.Label();
			this.label_ScreenName = new System.Windows.Forms.Label();
			this.btn_post = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.picbox_profileImage)).BeginInit();
			this.SuspendLayout();
			// 
			// picbox_profileImage
			// 
			this.picbox_profileImage.BackColor = System.Drawing.Color.Transparent;
			this.picbox_profileImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.picbox_profileImage.Location = new System.Drawing.Point(113, 55);
			this.picbox_profileImage.Name = "picbox_profileImage";
			this.picbox_profileImage.Size = new System.Drawing.Size(200, 200);
			this.picbox_profileImage.TabIndex = 0;
			this.picbox_profileImage.TabStop = false;
			// 
			// label_UserName
			// 
			this.label_UserName.AutoSize = true;
			this.label_UserName.Font = new System.Drawing.Font("Malgun Gothic", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label_UserName.Location = new System.Drawing.Point(132, 282);
			this.label_UserName.Name = "label_UserName";
			this.label_UserName.Size = new System.Drawing.Size(181, 45);
			this.label_UserName.TabIndex = 1;
			this.label_UserName.Text = "유 저 네 임";
			// 
			// label_ScreenName
			// 
			this.label_ScreenName.AutoSize = true;
			this.label_ScreenName.Font = new System.Drawing.Font("Malgun Gothic", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label_ScreenName.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.label_ScreenName.Location = new System.Drawing.Point(156, 327);
			this.label_ScreenName.Name = "label_ScreenName";
			this.label_ScreenName.Size = new System.Drawing.Size(118, 30);
			this.label_ScreenName.TabIndex = 2;
			this.label_ScreenName.Text = "스크린네임";
			// 
			// btn_post
			// 
			this.btn_post.Font = new System.Drawing.Font("Malgun Gothic", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.btn_post.Location = new System.Drawing.Point(113, 447);
			this.btn_post.Name = "btn_post";
			this.btn_post.Size = new System.Drawing.Size(250, 60);
			this.btn_post.TabIndex = 3;
			this.btn_post.Text = "작 성";
			this.btn_post.UseVisualStyleBackColor = true;
			this.btn_post.Click += new System.EventHandler(this.btn_post_Click);
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(434, 611);
			this.Controls.Add(this.btn_post);
			this.Controls.Add(this.label_ScreenName);
			this.Controls.Add(this.label_UserName);
			this.Controls.Add(this.picbox_profileImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Main";
			this.Text = "뎃찌NP";
			this.Resize += new System.EventHandler(this.Main_Resize);
			((System.ComponentModel.ISupportInitialize)(this.picbox_profileImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

        private RoundedPicturebox picbox_profileImage;
        private System.Windows.Forms.Label label_UserName;
        private System.Windows.Forms.Label label_ScreenName;
		private System.Windows.Forms.Button btn_post;
	}
}


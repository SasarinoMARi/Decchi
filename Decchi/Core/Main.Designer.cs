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
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
			this.label_UserName = new System.Windows.Forms.Label();
			this.label_ScreenName = new System.Windows.Forms.Label();
			this.btn_post = new System.Windows.Forms.Button();
			this.picbox_profileImage = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.picbox_profileImage)).BeginInit();
			this.SuspendLayout();
			// 
			// label_UserName
			// 
			this.label_UserName.Font = new System.Drawing.Font("맑은 고딕", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label_UserName.Location = new System.Drawing.Point(132, 278);
			this.label_UserName.Name = "label_UserName";
			this.label_UserName.Size = new System.Drawing.Size(207, 49);
			this.label_UserName.TabIndex = 1;
			this.label_UserName.Text = "Loading...";
			this.label_UserName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_ScreenName
			// 
			this.label_ScreenName.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label_ScreenName.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.label_ScreenName.Location = new System.Drawing.Point(156, 327);
			this.label_ScreenName.Name = "label_ScreenName";
			this.label_ScreenName.Size = new System.Drawing.Size(173, 30);
			this.label_ScreenName.TabIndex = 2;
			this.label_ScreenName.Text = "";
			this.label_ScreenName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btn_post
			// 
			this.btn_post.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.btn_post.Location = new System.Drawing.Point(113, 447);
			this.btn_post.Name = "btn_post";
			this.btn_post.Size = new System.Drawing.Size(250, 60);
			this.btn_post.TabIndex = 3;
			this.btn_post.Text = "작 성";
			this.btn_post.UseVisualStyleBackColor = true;
			this.btn_post.Click += new System.EventHandler(this.btn_post_Click);
			// 
			// picbox_profileImage
			// 
			this.picbox_profileImage.BackColor = System.Drawing.Color.Transparent;
			this.picbox_profileImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.picbox_profileImage.Location = new System.Drawing.Point(113, 55);
			this.picbox_profileImage.Name = "picbox_profileImage";
			this.picbox_profileImage.Size = new System.Drawing.Size(200, 200);
			this.picbox_profileImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picbox_profileImage.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.Red;
			this.label1.Location = new System.Drawing.Point(48, 527);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(227, 12);
			this.label1.TabIndex = 4;
			this.label1.Text = "Ctrl + Q 누르면 어디서나 즉시 트윗 가능!";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(179, 566);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(133, 12);
			this.label2.TabIndex = 5;
			this.label2.Text = ">> 디자이너 구해요 <<";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(321, 590);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(101, 12);
			this.label3.TabIndex = 6;
			this.label3.Text = "@Sasarino MARi";
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(434, 611);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btn_post);
			this.Controls.Add(this.label_ScreenName);
			this.Controls.Add(this.label_UserName);
			this.Controls.Add(this.picbox_profileImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "뎃찌NP";
			this.Load += new System.EventHandler(this.Main_Load);
			this.Resize += new System.EventHandler(this.Main_Resize);
			((System.ComponentModel.ISupportInitialize)(this.picbox_profileImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox picbox_profileImage;
		private System.Windows.Forms.Label label_UserName;
		private System.Windows.Forms.Label label_ScreenName;
		private System.Windows.Forms.Button btn_post;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
	}
}


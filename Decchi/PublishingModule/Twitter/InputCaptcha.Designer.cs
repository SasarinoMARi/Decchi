namespace PublishingModule.Twitter
{
	partial class InputCaptcha
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputCaptcha));
			this.textBox_captcha = new System.Windows.Forms.TextBox();
			this.btn_summit = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBox_captcha
			// 
			this.textBox_captcha.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_captcha.Font = new System.Drawing.Font("Malgun Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.textBox_captcha.Location = new System.Drawing.Point(12, 12);
			this.textBox_captcha.Name = "textBox_captcha";
			this.textBox_captcha.Size = new System.Drawing.Size(125, 29);
			this.textBox_captcha.TabIndex = 0;
			this.textBox_captcha.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_captcha_KeyDown);
			this.textBox_captcha.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_captcha_KeyPress);
			// 
			// btn_summit
			// 
			this.btn_summit.Font = new System.Drawing.Font("Malgun Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.btn_summit.Location = new System.Drawing.Point(143, 12);
			this.btn_summit.Name = "btn_summit";
			this.btn_summit.Size = new System.Drawing.Size(79, 29);
			this.btn_summit.TabIndex = 1;
			this.btn_summit.Text = "확인";
			this.btn_summit.UseVisualStyleBackColor = true;
			this.btn_summit.Click += new System.EventHandler(this.btn_summit_Click);
			// 
			// InputCaptcha
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(234, 53);
			this.Controls.Add(this.btn_summit);
			this.Controls.Add(this.textBox_captcha);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "InputCaptcha";
			this.ShowIcon = false;
			this.Text = "인증";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox_captcha;
		private System.Windows.Forms.Button btn_summit;
	}
}
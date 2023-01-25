using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace LANC_v2
{
	public class LabelForm : Form
	{
		private IContainer components = null;

		private Button button1;

		private Button button2;

		private TextBox textBox1;

		private Label label1;

		public string gamertag;

		public bool changed = false;

		public LabelForm(string gamertag)
		{
			this.gamertag = gamertag;
			this.InitializeComponent();
			this.textBox1.Text = gamertag;
			this.textBox1.Focus();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			this.gamertag = this.textBox1.Text;
			this.changed = true;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			base.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			if ((!disposing ? false : this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(LabelForm));
			this.button1 = new Button();
			this.button2 = new Button();
			this.textBox1 = new TextBox();
			this.label1 = new Label();
			base.SuspendLayout();
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new Point(104, 81);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(87, 31);
			this.button1.TabIndex = 0;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new EventHandler(this.button1_Click);
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new Point(12, 82);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(86, 30);
			this.button2.TabIndex = 1;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new EventHandler(this.button2_Click);
			this.textBox1.Location = new Point(12, 40);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(169, 22);
			this.textBox1.TabIndex = 2;
			this.textBox1.KeyDown += new KeyEventHandler(this.textBox1_KeyDown);
			this.label1.AutoSize = true;
			this.label1.Location = new Point(16, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(47, 17);
			this.label1.TabIndex = 3;
			this.label1.Text = "Label:";
			base.AutoScaleDimensions = new SizeF(8f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(201, 122);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.textBox1);
			base.Controls.Add(this.button2);
			base.Controls.Add(this.button1);
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "GamertagForm";
			base.StartPosition = FormStartPosition.CenterParent;
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		protected override void OnActivated(EventArgs e)
		{
			this.textBox1.Focus();
			base.OnActivated(e);
		}

		private void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.button1.PerformClick();
			}
		}
	}
}
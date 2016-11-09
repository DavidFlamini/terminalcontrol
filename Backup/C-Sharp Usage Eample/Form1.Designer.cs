namespace C_Sharp_Usage_Eample
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.servertextBox1 = new System.Windows.Forms.TextBox();
            this.passtextBox2 = new System.Windows.Forms.TextBox();
            this.usertextBox3 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.terminalControl1 = new WalburySoftware.TerminalControl();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(143, 110);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // servertextBox1
            // 
            this.servertextBox1.Location = new System.Drawing.Point(12, 35);
            this.servertextBox1.Name = "servertextBox1";
            this.servertextBox1.Size = new System.Drawing.Size(100, 20);
            this.servertextBox1.TabIndex = 0;
            // 
            // passtextBox2
            // 
            this.passtextBox2.Location = new System.Drawing.Point(12, 113);
            this.passtextBox2.Name = "passtextBox2";
            this.passtextBox2.PasswordChar = '*';
            this.passtextBox2.Size = new System.Drawing.Size(100, 20);
            this.passtextBox2.TabIndex = 2;
            // 
            // usertextBox3
            // 
            this.usertextBox3.Location = new System.Drawing.Point(12, 74);
            this.usertextBox3.Name = "usertextBox3";
            this.usertextBox3.Size = new System.Drawing.Size(100, 20);
            this.usertextBox3.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "UserName";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "SSH Server";
            // 
            // terminalControl1
            // 
            this.terminalControl1.AuthType = Poderosa.ConnectionParam.AuthType.Password;
            
            this.terminalControl1.Host = "";
            this.terminalControl1.IdentifyFile = "";
            this.terminalControl1.Location = new System.Drawing.Point(12, 139);
            this.terminalControl1.Method = WalburySoftware.ConnectionMethod.Telnet;
            this.terminalControl1.Name = "terminalControl1";
            this.terminalControl1.Password = "";
            this.terminalControl1.Size = new System.Drawing.Size(622, 289);
            this.terminalControl1.TabIndex = 0;
            this.terminalControl1.Text = "terminalControl1";
            this.terminalControl1.UserName = "";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(262, 110);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "edit display";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 440);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.usertextBox3);
            this.Controls.Add(this.passtextBox2);
            this.Controls.Add(this.servertextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.terminalControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WalburySoftware.TerminalControl terminalControl1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox servertextBox1;
        private System.Windows.Forms.TextBox passtextBox2;
        private System.Windows.Forms.TextBox usertextBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
    }
}


/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: WarningWithDisableOption.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Poderosa;

namespace Poderosa.Forms
{
	/// <summary>
	/// WarningWithDisableOption �̊T�v�̐����ł��B
	/// </summary>
	internal class WarningWithDisableOption : System.Windows.Forms.Form
	{
		private static Icon _warningIcon;

		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Label _messageLabel;
		private CheckBox _disableCheckBox;
		/// <summary>
		/// �K�v�ȃf�U�C�i�ϐ��ł��B
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WarningWithDisableOption(string message)
		{
			//
			// Windows �t�H�[�� �f�U�C�i �T�|�[�g�ɕK�v�ł��B
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent �Ăяo���̌�ɁA�R���X�g���N�^ �R�[�h��ǉ����Ă��������B
			//
			_messageLabel.Text = message;
			this.Text = GEnv.Strings.GetString("Form.WarningWithDisableOption.Text");
			this._disableCheckBox.Text = GEnv.Strings.GetString("Form.WarningWithDisableOption._disableCheckBox");
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
			this._okButton = new System.Windows.Forms.Button();
			this._messageLabel = new System.Windows.Forms.Label();
			this._disableCheckBox = new CheckBox();
			this.SuspendLayout();
			// 
			// _okButton
			// 
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._okButton.Location = new System.Drawing.Point(120, 72);
			this._okButton.Name = "_okButton";
			this._okButton.TabIndex = 0;
			this._okButton.FlatStyle = FlatStyle.System;
			this._okButton.Text = "OK";
			// 
			// _messageLabel
			// 
			this._messageLabel.Location = new System.Drawing.Point(56, 8);
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.Size = new System.Drawing.Size(248, 40);
			this._messageLabel.TabIndex = 1;
			this._messageLabel.Text = "a";
			this._messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _disableCheckBox
			// 
			this._disableCheckBox.Location = new System.Drawing.Point(56, 48);
			this._disableCheckBox.Name = "_disableCheckBox";
			this._disableCheckBox.Size = new System.Drawing.Size(248, 24);
			this._disableCheckBox.TabIndex = 2;
			this._disableCheckBox.FlatStyle = FlatStyle.System;
			// 
			// WarningWithDisableOption
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(314, 103);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._disableCheckBox,
																		  this._messageLabel,
																		  this._okButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WarningWithDisableOption";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

		}
		#endregion

		protected override void OnPaint(PaintEventArgs a) {
			base.OnPaint(a);
			//�A�C�R���̕`��@.NET Framework�����ŃV�X�e���Ŏ����Ă���A�C�R���̃��[�h�͂ł��Ȃ��悤��
			if(_warningIcon==null) LoadWarningIcon();
			a.Graphics.DrawIcon(_warningIcon, 12, 24); 
		}

		public bool CheckedDisableOption {
			get {
				return _disableCheckBox.Checked;
			}
		}

		private static void LoadWarningIcon() {
			IntPtr hIcon = Win32.LoadIcon(IntPtr.Zero, new IntPtr(Win32.IDI_EXCLAMATION));
			_warningIcon = Icon.FromHandle(hIcon);
		}

	}
}

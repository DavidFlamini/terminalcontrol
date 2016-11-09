/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: HotKey.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.UI;

namespace Poderosa.Forms
{
	/*
	 * �z�b�g�L�[�R���g���[���b���
	 * 
	 * �L�[�����̂S��ނɋ�ʂ���B
	 * 1. ModifierKey: Ctrl, Shift, Alt�B�����Ƒg�ݍ��킹��L�[�B
	 * 2. CharKey: �����Ɗ֘A�t����ꂽ�L�[�B�e���L�[,�X�y�[�X�܂ށB
	 * 3. TerminalKey: �^�[�~�i���ɂƂ��ĈӖ��̂���L�[�BEnter,BS,ESC,Tab,�J�[�\���L�[,�t�@���N�V�����L�[�B
	 * 4. GenericKey:  ���̑��̃L�[
	 * 
	 * �z�b�g�L�[�ɗL���ȓ��͂�modifier�ɂ��قȂ�A
	 * modifier�Ȃ���Shift�̂݁F4�̂�
	 * Control�܂���Alt����F 2,3,4
	 * �ƂȂ�B
	 * 
	 * �m�F�ς݉����ہFShift+F4, Control+F10�ŕςȃL�[������
	 * 
	 * �f�o�b�O�I�v�V�����ŁA�����L�[�̃��X�g�����X�g�{�b�N�X������Ƀ_���v����@�\������Ƃ悢���낤�B
	*/


	/// <summary>
	/// HotKey �̊T�v�̐����ł��B
	/// </summary>
	internal class HotKey : System.Windows.Forms.TextBox
	{
		/// <summary>
		/// �K�v�ȃf�U�C�i�ϐ��ł��B
		/// </summary>
		private System.ComponentModel.Container components = null;

		private TextBox _debugTextBox;

		private Keys _key;

		public HotKey()
		{
			// ���̌Ăяo���́AWindows.Forms �t�H�[�� �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();

			// TODO: InitForm ���Ăяo���̌�ɏ�����������ǉ����܂��B

		}

		public TextBox DebugTextBox {
			get {
				return _debugTextBox;
			}
			set {
				_debugTextBox = value;
			}
		}

		public Keys Key {
			get {
				return _key;
			}
			set {
				_key = value;
				Text = FormatKey(value);
			}
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

		#region Component Designer generated code
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ImeMode = ImeMode.Disable;
		}
		#endregion

		protected override bool IsInputKey(Keys key) {
			return false;
		}

		protected override void OnKeyUp(KeyEventArgs args) {
			base.OnKeyUp(args);

			Keys body = _key & Keys.KeyCode;
			if(body==Keys.Menu || body==Keys.ShiftKey || body==Keys.ControlKey) { //modifier�݂͔̂F�߂Ȃ�
				_key = Keys.None;
				this.Text = "";
			}
		}
		protected override bool ProcessDialogKey(Keys key) {
			if(_debugTextBox!=null) AppendDebugText(key.ToString()+" "+(int)key);
			string t = FormatKey(key);
			if(t!=null) {
				this.Text = t;
			}
			else
				_key = Keys.None;
			return true;
		}

		private string FormatKey(Keys key) {
			Keys body = key & Keys.KeyCode;
			Keys modifiers = key & Keys.Modifiers;

			//modifier�͏�ɕ\������
			StringBuilder b = new StringBuilder();
			if((modifiers & Keys.Control)!=Keys.None) {
				b.Append("Ctrl");
			}
			if((modifiers & Keys.Shift)!=Keys.None) {
				if(b.Length>0) b.Append('+');
				b.Append("Shift");
			}
			if((modifiers & Keys.Alt)!=Keys.None) {
				if(b.Length>0) b.Append('+');
				b.Append("Alt");
			}
			if(b.Length>0)
				b.Append('+');

			if(IsCharKey(body)) {
				if(modifiers!=Keys.None && modifiers!=Keys.Shift) {
					if(modifiers==Keys.Alt && (Keys.D0 <= body && body <= Keys.D9))
						_key = Keys.None;
					else {
						b.Append(UILibUtil.KeyString(body));
						_key = key;
					}
				}
				else
					_key = Keys.None;
			}
			else if(IsTerminalKey(body)) {
				if(modifiers!=Keys.None) {
					//�J�X�^�}�C�Y�s�\�ŌŒ肳�ꂽ�V���[�g�J�b�g�L�[�͓o�^�ł��Ȃ�
					if(modifiers==Keys.Control && IsScrollKey(body))
						_key = Keys.None;
					else {
						b.Append(UILibUtil.KeyString(body));
						_key = key;
					}
				}
				else
					_key = Keys.None;
			}
			else if(IsFunctionKey(body)) {
				b.Append(UILibUtil.KeyString(body));
				_key = key;
			}
			else if(!IsModifierKey(body)) {
				_key = Keys.None;
			}

			return b.ToString();
		}

		private static bool IsCharKey(Keys key) {
			int n = (int)key;
			return ((int)Keys.A <= n && n <= (int)Keys.Z) ||
				((int)Keys.D0 <= n && n <= (int)Keys.D9) ||
				((int)Keys.NumPad0 <= n && n <= (int)Keys.NumPad9) ||
				((int)Keys.OemSemicolon <=n && n <= (int)Keys.Oemtilde) ||
				((int)Keys.OemOpenBrackets <=n && n <= (int)Keys.OemQuotes) ||
				key==Keys.Divide || key==Keys.Multiply || key==Keys.Subtract || key==Keys.Add || key==Keys.Decimal ||
				key==Keys.Space || key==Keys.Enter;
		}
		private static bool IsModifierKey(Keys key) {
			return key==Keys.Menu || key==Keys.ShiftKey || key==Keys.ControlKey ||
				key==Keys.LMenu || key==Keys.RMenu ||
				key==Keys.LShiftKey || key==Keys.RShiftKey ||
				key==Keys.LControlKey || key==Keys.RControlKey;
		}
		private static bool IsFunctionKey(Keys key) {
			return (int)Keys.F1 <= (int)key && (int)key <= (int)Keys.F24;
		}
		private static bool IsTerminalKey(Keys key) {
			return key==Keys.Escape || key==Keys.Back || key==Keys.Tab ||
				key==Keys.Up || key==Keys.Down || key==Keys.Left || key==Keys.Right ||
				((int)Keys.F1 <= (int)key && (int)key <= (int)Keys.F12) || 
				key==Keys.Home || key==Keys.End || key==Keys.Next || key==Keys.Prior || key==Keys.PageDown || key==Keys.PageUp ||
				key==Keys.Insert || key==Keys.Delete;
		}
		private static bool IsScrollKey(Keys key) { //TerminalKey�̃T�u�Z�b�g�ŁACtrl�Ƃ̑g�ݍ��킹�Ńo�b�t�@�̃X�N���[��������
			return key==Keys.Up || key==Keys.Down ||
				key==Keys.Home || key==Keys.End ||
				key==Keys.PageDown || key==Keys.PageUp;
		}


		private void AppendDebugText(string text) {
			string[] data = _debugTextBox.Lines;
			if(data.Length>=5) {
				string[] n = new string[5];
				Array.Copy(data, data.Length-4, n, 0, 4);
				n[4] = text;
				_debugTextBox.Lines = n;
			}
			else {
				string[] n = new string[data.Length+1];
				Array.Copy(data, 0, n, 0, data.Length);
				n[data.Length] = text;
				_debugTextBox.Lines = n;
			}
		}
	}
}

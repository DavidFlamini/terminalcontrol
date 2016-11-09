/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalDataReceiver.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.Config;
using Poderosa.Forms;
using Poderosa.Communication;
using Poderosa.Text;
using Poderosa.Log;

namespace Poderosa.Terminal
{
	/// <summary>
	/// TerminalDataReceiver �̊T�v�̐����ł��B
	/// </summary>
	public class TerminalDataReceiver : IDataReceiver {
		private ConnectionTag _tag;
		//��M�X���b�h�ł����̒l��ݒ肵�A����OnPaint�����C���X���b�h�ł̎��s��Commit����
		private bool _transientScrollBarDirty; //���ꂪ�����Ă���Ɨv�ݒ�
		private bool _transientScrollBarEnabled;
		private int  _transientScrollBarValue;
		private int  _transientScrollBarLargeChange;
		private int  _transientScrollBarMaximum;

		public TerminalDataReceiver(ConnectionTag ct) {
			_tag = ct;
		}
		public void Listen() {
			_tag.Connection.RepeatAsyncRead(this);
		}

		public void DataArrived(byte[] data, int offset, int count) {
			try {
				TerminalConnection con = _tag.Connection;
				con.AddReceivedDataStats(count);
				con.BinaryLogger.Append(data, offset, count);

				if(_tag.ModalTerminalTask!=null && _tag.ModalTerminalTask.CanReceive)
					_tag.ModalTerminalTask.Input(data, offset, count);
				else {
					TerminalDocument document = _tag.Document;
					lock(document) {
						_tag.InvalidateParam.Reset();
						_tag.Terminal.Input(data, offset, count);

						//�E�[�ɃL�����b�g�������Ƃ��͕֋X�I�Ɏ��s�̓��ɂ����Ă���
						if(document.CaretColumn==_tag.Connection.TerminalWidth) {
							document.CurrentLineNumber++; //����ɂ���Ď��s�̑��݂�ۏ�
							document.CaretColumn = 0;
						}

						CheckDiscardDocument();
						AdjustTransientScrollBar();

						int n = document.CurrentLineNumber-_tag.Connection.TerminalHeight+1-document.FirstLineNumber;
						if(n < 0) n = 0;

						//Debug.WriteLine(String.Format("E={0} C={1} T={2} H={3} LC={4} MAX={5} n={6}", _transientScrollBarEnabled, _tag.Document.CurrentLineNumber, _tag.Document.TopLineNumber, _tag.Connection.TerminalHeight, _transientScrollBarLargeChange, _transientScrollBarMaximum, n));
						if(IsAutoScrollMode(n)) {
							_transientScrollBarValue = n;
							document.TopLineNumber = n + document.FirstLineNumber;
						}
						else
							_transientScrollBarValue = document.TopLineNumber - document.FirstLineNumber;

						_tag.NotifyUpdate();
					}

					//Invalidate��lock�̊O�ɏo���B���̂ق������S�Ǝv��ꂽ
					if(_tag.Pane!=null) _tag.InvalidateParam.InvokeFor(_tag.Pane);

					ITerminalTextLogger tl = con.TextLogger;
					if(tl!=null) {
						tl.PacketDelimiter();
						tl.Flush();
					}
				}

				ITerminalBinaryLogger bl = con.BinaryLogger;
				if(bl!=null)
					bl.Flush();
			}
			catch(Exception ex) {
				GEnv.InterThreadUIService.ReportCriticalError(ex);
			}
		}
		private bool IsAutoScrollMode(int value_candidate) {
			return _tag.Terminal.TerminalMode==TerminalMode.Normal && 
				_tag.Document.CurrentLineNumber>=_tag.Document.TopLineNumber+_tag.Connection.TerminalHeight-1 &&
				(!_transientScrollBarEnabled || value_candidate+_transientScrollBarLargeChange>_transientScrollBarMaximum);
		}
		private void CheckDiscardDocument() {
			if(_tag==null || _tag.Terminal.TerminalMode==TerminalMode.Application) return;

			TerminalDocument document = _tag.Document;
			int del = document.DiscardOldLines(GEnv.Options.TerminalBufferSize+_tag.Connection.TerminalHeight);
			if(del > 0) {
				_tag.NotifyUpdate();
				TextSelection sel = GEnv.TextSelection;
				if(sel.Owner==_tag.Pane)
					sel.ClearIfOverlapped(document.FirstLineNumber);
				int newvalue = _transientScrollBarValue - del;
				if(newvalue<0) newvalue=0;
				_transientScrollBarValue = newvalue;
				document.InvalidateAll(); //�{���͂����܂ł��Ȃ��Ă��ǂ����������O�̂���
			}
		}

		internal void AdjustTransientScrollBar() {
			TerminalDocument document = _tag.Document;
			int paneheight = _tag.Connection.TerminalHeight;
			int docheight = Math.Max(document.LastLineNumber, document.TopLineNumber+paneheight-1)-document.FirstLineNumber+1;

			_transientScrollBarDirty = true;
			if((_tag.Terminal.TerminalMode==TerminalMode.Application && !GEnv.Options.AllowsScrollInAppMode)
				|| paneheight >= docheight) {
				_transientScrollBarEnabled = false;
				_transientScrollBarValue = 0;
			}
			else {
				_transientScrollBarEnabled = true;
				_transientScrollBarMaximum = docheight-1;
				_transientScrollBarLargeChange = paneheight;
			}
			//Debug.WriteLine(String.Format("E={0} V={1}", _transientScrollBarEnabled, _transientScrollBarValue));
		}

		public void SetTransientScrollBarValue(int value) {
			_transientScrollBarValue = value;
			_transientScrollBarDirty = true;
		}

		public void CommitScrollBar(VScrollBar sb, bool dirty_only) {
			if(dirty_only && !_transientScrollBarDirty) return;

			sb.Enabled = _transientScrollBarEnabled;
			sb.Maximum = _transientScrollBarMaximum;
			sb.LargeChange = _transientScrollBarLargeChange;
			//!!�{������if���͕s�v�Ȃ͂������A�͈̓G���[�ɂȂ�P�[�X�����󂯂�ꂽ�B���̌�����T���ă����[�X���O�ɂ��낢�낢����̂͊댯�Ȃ̂ł����͓�����B��ł����Ɖ𖾂���B
			if(_transientScrollBarValue < _transientScrollBarMaximum)
				sb.Value = _transientScrollBarValue;
			_transientScrollBarDirty = false;
		}

		public void ErrorOccurred(string msg) {
			if(GEnv.Frame.IgnoreErrors) return;

			Debug.WriteLine("Closed="+_tag.Connection.IsClosed);
			if(!_tag.Connection.IsClosed) { //����w�߂��o������̃G���[�͕\�����Ȃ�
				GEnv.InterThreadUIService.Warning(String.Format(GEnv.Strings.GetString("Message.TerminalDataReceiver.GenericError"),_tag.Connection.Param.ShortDescription, msg));
				try {
					_tag.Connection.Close();
				}
				catch(Exception) { //�G���[�ʒm���Close�Ɏ��s����͖̂����B
				}
				GEnv.Frame.RefreshConnection(_tag);
				_tag.NotifyDisconnect();
			}
		}

		public void DisconnectedFromServer() {
			if(GEnv.Frame.IgnoreErrors) return;
			_tag.NotifyDisconnect();

			TerminalConnection c = _tag.Connection;
			GEnv.InterThreadUIService.DisconnectedFromServer(c);
		}



		public void IndicateBell() {
			GEnv.InterThreadUIService.IndicateBell(_tag.Document);
			if(GEnv.Options.BeepOnBellChar) Win32.MessageBeep(-1);
		}

	}

	//��������������I�u�W�F�N�g��ConnectionTag�ɃZ�b�g���Ă���Ԃ́A�f�[�^�������ł���B
	public interface IModalTerminalTask {
		void Input(byte[] data, int offset, int count);
		bool CanReceive { get; }
		string Caption { get; }
	}
}

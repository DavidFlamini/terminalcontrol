/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: XTerm.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.Text;

namespace Poderosa.Terminal
{
	internal class XTerm : VT100Terminal
	{
		protected bool _wrapAroundMode;
		protected bool[] _tabStops;
		protected ArrayList _savedScreen; //�ʂ̃o�b�t�@�Ɉȍ~�����Ƃ���GLine��ޔ����Ă���

		public XTerm(ConnectionTag tag, ICharDecoder decoder) : base(tag, decoder) {
			_wrapAroundMode = true;
			_tabStops = new bool[tag.Connection.TerminalWidth];
			InitTabStops();
		}

		protected override ProcessCharResult ProcessNormalChar(char ch) {
			//WrapAround��false�ŁA�L�����b�g���E�[�̂Ƃ��͉������Ȃ�
			if(!_wrapAroundMode && _manipulator.CaretColumn>=GetConnection().TerminalWidth-1)
				return ProcessCharResult.Processed;

			if(_insertMode)
				_manipulator.InsertBlanks(_manipulator.CaretColumn, GLine.CalcDisplayLength(ch));
			return base.ProcessNormalChar(ch);
		}
		protected override ProcessCharResult ProcessControlChar(char ch) {
			return base.ProcessControlChar(ch);
			/* �����R�[�h������Ă���Ƃ��̂������s�ӂɎ��s���Ă��܂����Ƃ�����A��낵���Ȃ��B
			switch(ch) {
				//�P���ȕϊ��Ȃ瑼�ɂ��ł��邪�A�T�|�[�g���Ă���̂͂��܂̂Ƃ��낱�ꂵ���Ȃ�
				case (char)0x8D:
					base.ProcessChar((char)0x1B);
					base.ProcessChar('M');
					return ProcessCharResult.Processed;
				case (char)0x9B:
					base.ProcessChar((char)0x1B);
					base.ProcessChar('[');
					return ProcessCharResult.Processed;
				case (char)0x9D:
					base.ProcessChar((char)0x1B);
					base.ProcessChar(']');
					return ProcessCharResult.Processed;
				default:
					return base.ProcessControlChar(ch);
			}
			*/
		}

		protected override ProcessCharResult ProcessEscapeSequence(char code, char[] seq, int offset) {
			ProcessCharResult v = base.ProcessEscapeSequence(code, seq, offset);
			if(v!=ProcessCharResult.Unsupported) return v;

			switch(code) {
				case 'F':
					if(seq.Length==offset) { //�p�����[�^�Ȃ��̏ꍇ
						ProcessCursorPosition(1, 1);
						return ProcessCharResult.Processed;
					}
					else if(seq.Length>offset && seq[offset]==' ')
						return ProcessCharResult.Processed; //7/8�r�b�g�R���g���[���͏�ɗ������T�|�[�g
					break;
				case 'G':
					if(seq.Length>offset && seq[offset]==' ')
						return ProcessCharResult.Processed; //7/8�r�b�g�R���g���[���͏�ɗ������T�|�[�g
					break;
				case 'L':
					if(seq.Length>offset && seq[offset]==' ')
						return ProcessCharResult.Processed; //VT100�͍ŏ�����OK
					break;
				case 'H':
					SetTabStop(_manipulator.CaretColumn, true);
					return ProcessCharResult.Processed;
			}

			return ProcessCharResult.Unsupported;
		}
		protected override ProcessCharResult ProcessAfterCSI(string param, char code) {
			ProcessCharResult v = base.ProcessAfterCSI(param, code);
			if(v!=ProcessCharResult.Unsupported) return v;

			switch(code) {
				case 'd':
					ProcessLinePositionAbsolute(param);
					return ProcessCharResult.Processed;
				case 'G':
				case '`': //CSI G�͎��ۂɗ������Ƃ����邪�A����͗������Ƃ��Ȃ��B�����̂��H
					ProcessLineColumnAbsolute(param);
					return ProcessCharResult.Processed;
				case 'X':
					ProcessEraseChars(param);
					return ProcessCharResult.Processed;
				case 'P':
					_manipulator.DeleteChars(_manipulator.CaretColumn, ParseInt(param,1));
					return ProcessCharResult.Processed;
				case 'p':
					return SoftTerminalReset(param);
				case '@':
					_manipulator.InsertBlanks(_manipulator.CaretColumn, ParseInt(param,1));
					return ProcessCharResult.Processed;
				case 'I':
					ProcessForwardTab(param);
					return ProcessCharResult.Processed;
				case 'Z':
					ProcessBackwardTab(param);
					return ProcessCharResult.Processed;
				case 'S':
					ProcessScrollUp(param);
					return ProcessCharResult.Processed;
				case 'T':
					ProcessScrollDown(param);
					return ProcessCharResult.Processed;
				case 'g':
					ProcessTabClear(param);
					return ProcessCharResult.Processed;
				case 't':
					//!!�p�����[�^�ɂ���Ė������Ă悢�ꍇ�ƁA������Ԃ��ׂ��ꍇ������B�����̕Ԃ������悭�킩��Ȃ��̂ŕۗ���
					return ProcessCharResult.Processed;
				case 'U': //�����SFU�ł����m�F�ł��ĂȂ�
					base.ProcessCursorPosition(GetConnection().TerminalHeight, 1);
					return ProcessCharResult.Processed;
				case 'u': //SFU�ł̂݊m�F�B����b�͑����������J��Ԃ��炵�����A�Ӗ��̂��铮��ɂȂ��Ă���Ƃ�������Ă��Ȃ�
				case 'b':
					return ProcessCharResult.Processed;
				default:
					return ProcessCharResult.Unsupported;
			}
		}

		protected override void ProcessDeviceAttributes(string param) {
			if(param.StartsWith(">")) {
				byte[] data = Encoding.ASCII.GetBytes(" [>82;1;0c");
				data[0] = 0x1B; //ESC
				GetConnection().Write(data);
			}
			else
				base.ProcessDeviceAttributes(param);
		}


		protected override ProcessCharResult ProcessAfterOSC(string param, char code) {
			ProcessCharResult v = base.ProcessAfterOSC(param, code);
			if(v!=ProcessCharResult.Unsupported) return v;

			int semicolon = param.IndexOf(';');
			if(semicolon==-1) return ProcessCharResult.Unsupported;

			string ps = param.Substring(0, semicolon);
			string pt = param.Substring(semicolon+1);
			if(ps=="0" || ps=="2") {
				if(pt!=_tag.WindowTitle) {
					_tag.WindowTitle = pt;
					if(GEnv.Options.AdjustsTabTitleToWindowTitle)
						_tag.Connection.Param.Caption = pt;

					GEnv.InterThreadUIService.RefreshConnection(_tag);
				}
				return ProcessCharResult.Processed;
			}
			else if(ps=="1")
				return ProcessCharResult.Processed; //Set Icon Name�Ƃ�������������ł悳����
			else
				return ProcessCharResult.Unsupported;

		}

		protected override ProcessCharResult ProcessDECSET(string param, char code) {
			ProcessCharResult v = base.ProcessDECSET(param, code);
			if(v!=ProcessCharResult.Unsupported) return v;
			
			if(param=="1047") { //Screen Buffer: �Ƃ��ɓ��ʂȂ��Ƃ͂��Ȃ�
				return ProcessCharResult.Processed;
			}
			else if(param=="1048" || param=="1049") { //Save/Restore Cursor
				return ProcessCharResult.Processed;
			}
			else if(param=="1000" || param=="1001" || param=="1002" || param=="1003") { //�}�E�X�֌W�͖���
				return ProcessCharResult.Processed;
			}
			else if(param=="3") { //132 Column Mode
				return ProcessCharResult.Processed;
			}
			else if(param=="4") { //Smooth Scroll �Ȃ�̂��Ƃ��
				return ProcessCharResult.Processed;
			}
			else if(param=="6") { //Origin Mode
				_scrollRegionRelative = code=='h';
				return ProcessCharResult.Processed;
			}
			else if(param=="7") {
				_wrapAroundMode = code=='h';
				return ProcessCharResult.Processed;
			}
			else if(param=="47") {
				if(code=='h')
					SaveScreen();
				else
					RestoreScreen();
				return ProcessCharResult.Processed;
			}
			else
				return ProcessCharResult.Unsupported;
		}


		private void ProcessLinePositionAbsolute(string param) {
			foreach(string p in param.Split(';')) {
				int row = ParseInt(p,1);
				if(row<1) row = 1;
				if(row>GetConnection().TerminalHeight) row = GetConnection().TerminalHeight;

				int col = _manipulator.CaretColumn;

				//�ȉ���CSI H�Ƃقړ���
				GetDocument().ReplaceCurrentLine(_manipulator.Export());
				GetDocument().CurrentLineNumber = (GetDocument().TopLineNumber + row - 1);
				_manipulator.Load(GetDocument().CurrentLine, col);
			}
		}
		private void ProcessLineColumnAbsolute(string param) {
			foreach(string p in param.Split(';')) {
				int n = ParseInt(p,1);
				if(n<1) n = 1;
				if(n>GetConnection().TerminalWidth) n = GetConnection().TerminalWidth;
				_manipulator.CaretColumn = n-1;
			}
		}
		private void ProcessEraseChars(string param) {
			int n = ParseInt(param, 1);
			int s = _manipulator.CaretColumn;
			for(int i=0; i<n; i++) {
				_manipulator.PutChar(' ', _currentdecoration);
				if(_manipulator.CaretColumn>=_manipulator.BufferSize)
					break;
			}
			_manipulator.CaretColumn = s;
		}
		private void ProcessScrollUp(string param) {
			int d = ParseInt(param, 1);

			TerminalDocument doc = GetDocument();
			int caret_col = _manipulator.CaretColumn;
			int offset = doc.CurrentLineNumber - doc.TopLineNumber;
			GLine nl = _manipulator.Export();
			doc.ReplaceCurrentLine(nl);
			if(doc.ScrollingBottom==-1)
				doc.SetScrollingRegion(0, GetConnection().TerminalHeight-1);
			for(int i=0; i<d; i++) {
				doc.ScrollUp(doc.CurrentLineNumber, doc.ScrollingBottom);
				doc.CurrentLineNumber = doc.TopLineNumber + offset;
			}
			_manipulator.Load(doc.CurrentLine, caret_col);
		}
		private void ProcessScrollDown(string param) {
			int d = ParseInt(param, 1);

			TerminalDocument doc = GetDocument();
			int caret_col = _manipulator.CaretColumn;
			int offset = doc.CurrentLineNumber - doc.TopLineNumber;
			GLine nl = _manipulator.Export();
			doc.ReplaceCurrentLine(nl);
			if(doc.ScrollingBottom==-1)
				doc.SetScrollingRegion(0, GetConnection().TerminalHeight-1);
			for(int i=0; i<d; i++) {
				doc.ScrollDown(doc.CurrentLineNumber, doc.ScrollingBottom);
				doc.CurrentLineNumber = doc.TopLineNumber + offset;
			}
			_manipulator.Load(doc.CurrentLine, caret_col);
		}
		private void ProcessForwardTab(string param) {
			int n = ParseInt(param, 1);
			
			int t = _manipulator.CaretColumn;
			for(int i=0; i<n; i++)
				t = GetNextTabStop(t);
			if(t >= GetConnection().TerminalWidth) t = GetConnection().TerminalWidth-1;
			_manipulator.CaretColumn = t;
		}
		private void ProcessBackwardTab(string param) {
			int n = ParseInt(param, 1);
			
			int t = _manipulator.CaretColumn;
			for(int i=0; i<n; i++)
				t = GetPrevTabStop(t);
			if(t < 0) t = 0;
			_manipulator.CaretColumn = t;
		}
		private void ProcessTabClear(string param) {
			if(param=="0")
				SetTabStop(_manipulator.CaretColumn, false);
			else if(param=="3")
				ClearAllTabStop();
		}

		private void InitTabStops() {
			for(int i=0; i<_tabStops.Length; i++) {
				_tabStops[i] = (i % 8)==0;
			}
		}
		private void EnsureTabStops(int length) {
			if(length>=_tabStops.Length) {
				bool[] newarray = new bool[Math.Max(length, _tabStops.Length*2)];
				Array.Copy(_tabStops, 0, newarray, 0, _tabStops.Length);
				for(int i=_tabStops.Length; i<newarray.Length; i++) {
					newarray[i] = (i % 8)==0;
				}
				_tabStops = newarray;
			}
		}
		private void SetTabStop(int index, bool value) {
			EnsureTabStops(index+1);
			_tabStops[index] = value;
		}
		private void ClearAllTabStop() {
			for(int i=0; i<_tabStops.Length; i++) {
				_tabStops[i] = false;
			}
		}
		protected override int GetNextTabStop(int start) {
			EnsureTabStops(Math.Max(start+1, _tag.Connection.TerminalWidth));

			int index = start+1;
			while(index<_tag.Connection.TerminalWidth) {
				if(_tabStops[index]) return index;
				index++;
			}
			return _tag.Connection.TerminalWidth-1;
		}
		//�����vt100�ɂ͂Ȃ��̂�override���Ȃ�
		protected int GetPrevTabStop(int start) {
			EnsureTabStops(start+1);

			int index = start-1;
			while(index>0) {
				if(_tabStops[index]) return index;
				index--;
			}
			return 0;
		}

		protected void SaveScreen() {
			_savedScreen = new ArrayList();
			GLine l = GetDocument().TopLine;
			int m = l.ID + GetConnection().TerminalHeight;
			while(l!=null && l.ID<m) {
				_savedScreen.Add(l.Clone());
				l = l.NextLine;
			}
		}
		protected void RestoreScreen() {
			if(_savedScreen==null) return;
			TerminalDocument doc = GetDocument();
			int w = GetConnection().TerminalWidth;
			int m = GetConnection().TerminalHeight;
			GLine t = doc.TopLine;
			foreach(GLine l in _savedScreen) {
				l.ExpandBuffer(w);
				if(t==null)
					doc.AddLine(l);
				else {
					doc.Replace(t, l);
					t = l.NextLine;
				}
				if(--m == 0) break;
			}

			_savedScreen = null;
		}
		private ProcessCharResult SoftTerminalReset(string param){
			if(param=="!") {
				FullReset();
				return ProcessCharResult.Processed;
			}
			else
				return ProcessCharResult.Unsupported;
		}

		public override byte[] SequenceKeyData(Keys modifier, Keys key) {
			if((int)Keys.F1 <= (int)key && (int)key <= (int)Keys.F12)
				return base.SequenceKeyData(modifier, key);
			else if(GUtil.IsCursorKey(key))
				return base.SequenceKeyData(modifier, key);
			else {
				byte[] r = new byte[4];
				r[0] = 0x1B;
				r[1] = (byte)'[';
				r[3] = (byte)'~';
				//���̂������xterm�ł͊��ƈႤ�悤��
				if(key==Keys.Insert)
					r[2] = (byte)'2';
				else if(key==Keys.Home)
					r[2] = (byte)'7';
				else if(key==Keys.PageUp)
					r[2] = (byte)'5';
				else if(key==Keys.Delete)
					r[2] = (byte)'3';
				else if(key==Keys.End)
					r[2] = (byte)'8';
				else if(key==Keys.PageDown)
					r[2] = (byte)'6';
				else
					throw new ArgumentException("unknown key " + key.ToString());
				return r;
			}
		}

		public override void FullReset() {
			InitTabStops();
			base.FullReset();
		}
	}
}

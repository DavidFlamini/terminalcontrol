/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: VT100.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Text;
using Poderosa.UI;

namespace Poderosa.Terminal
{
	internal class VT100Terminal : EscapeSequenceTerminal {

		protected int _savedRow;
		protected int _savedCol;
		protected bool _insertMode;
		protected bool _scrollRegionRelative;

		//�ڑ��̎�ނɂ���ăG�X�P�[�v�V�[�P���X�̉��߂�ς��镔��
		protected bool _homePositionOnCSIJ2;

		public VT100Terminal(ConnectionTag tag, ICharDecoder decoder) : base(tag, decoder) {
			_insertMode = false;
			_scrollRegionRelative = false;
			bool sfu = tag.Connection.Param is SFUTerminalParam;
			_homePositionOnCSIJ2 = sfu;
		}
		protected override void ResetInternal() {
			base.ResetInternal();
			_insertMode = false;
			_scrollRegionRelative = false;
		}


		protected override ProcessCharResult ProcessEscapeSequence(char code, char[] seq, int offset) {
			string param;
			switch(code) {
				case '[':
					if(seq.Length-offset-1>=0) {
						param = new string(seq, offset, seq.Length-offset-1);
						return ProcessAfterCSI(param, seq[seq.Length-1]);
					}
					break;
					//throw new UnknownEscapeSequenceException(String.Format("unknown command after CSI {0}", code));
				case ']':
					if(seq.Length-offset-1>=0) {
						param = new string(seq, offset, seq.Length-offset-1);
						return ProcessAfterOSC(param, seq[seq.Length-1]);
					}
					break;
				case '=':
					ChangeMode(TerminalMode.Application);
					return ProcessCharResult.Processed;
				case '>':
					ChangeMode(TerminalMode.Normal);
					return ProcessCharResult.Processed;
				case 'E':
					ProcessNextLine();
					return ProcessCharResult.Processed;
				case 'M': 
					ReverseIndex();
					return ProcessCharResult.Processed;
				case 'D': 
					Index();
					return ProcessCharResult.Processed;
				case '7':
					SaveCursor();
					return ProcessCharResult.Processed;
				case '8':
					RestoreCursor();
					return ProcessCharResult.Processed;
				case 'c':
					FullReset();
					return ProcessCharResult.Processed;
			}
			return ProcessCharResult.Unsupported;
		}

		protected virtual ProcessCharResult ProcessAfterCSI(string param, char code) {

			switch(code) {
				case 'c':
					ProcessDeviceAttributes(param);
					break;
				case 'm': //SGR
					ProcessSGR(param);
					break;
				case 'h':
				case 'l':
					return ProcessDECSETMulti(param, code);
				case 'r':
					if(param.Length>0 && param[0]=='?')
						return ProcessRestoreDECSET(param, code);
					else
						ProcessSetScrollingRegion(param);
					break;
				case 's':
					return ProcessSaveDECSET(param, code);
				case 'n':
					ProcessDeviceStatusReport(param);
					break;
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
					ProcessCursorMove(param, code);
					break;
				case 'H':
				case 'f': //f�͖{����xterm�ŗL
					ProcessCursorPosition(param);
					break;
				case 'J':
					ProcessEraseInDisplay(param);
					break;
				case 'K':
					ProcessEraseInLine(param);
					break;
				case 'L':
					ProcessInsertLines(param);
					break;
				case 'M':
					ProcessDeleteLines(param);
					break;
				default:
					return ProcessCharResult.Unsupported; 
			}

			return ProcessCharResult.Processed;
		}
		protected virtual ProcessCharResult ProcessAfterOSC(string param, char code) {
			return ProcessCharResult.Unsupported;
		}

		protected void ProcessSGR(string param) {
			string[] ps = param.Split(';');

			foreach(string cmd in ps) {

				TextDecoration dec = (TextDecoration)_currentdecoration.Clone();
				int code = ParseSGRCode(cmd);
				if(code>=30 && code<=37) {
					//���ꂾ�ƐF��ύX�����Ƃ����ɉ�ʂɂ�����̂͘A�����Ȃ��Ȃ邪�A�����������Ƃ���͍̂���ł���
					dec.TextColor = _tag.GetCurrentRenderProfile().ESColorSet[code % 10];
				}
				else if(code>=40 && code<=47) {
					Color c = _tag.GetCurrentRenderProfile().ESColorSet[code % 10];
					dec.BackColor = DrawUtil.DarkColor(c); //�w�i�F�͈Â߂�
				}
				else {
					switch(code) {
						case 0:
							dec = TextDecoration.ClonedDefault();
							break;
						case 1:
						case 5:
							dec.Bold = true;
							break;
						case 4:
							dec.Underline = true;
							break;
						case 7:
							dec.Inverse();
							break;
						case 2:
							dec = TextDecoration.ClonedDefault(); //�s������SGR 2�ŏI����Ă���Ⴊ������
							break;
						case 22:
						case 25:
						case 27:
						case 28:
							dec = TextDecoration.ClonedDefault();
							break;
						case 24:
							dec.Underline = false;
							break;
						case 39:
							dec.TextColorType = ColorType.DefaultText;
							break;
						case 49:
							dec.BackColorType = ColorType.DefaultBack;
							break;
						case 10:
						case 11:
						case 12:
							break; //'konsole'�Ƃ�����炵���B�����Ŗ��Ȃ�����
						default:
							throw new UnknownEscapeSequenceException(String.Format("unknown SGR command {0}", param));
					}
				}

				_currentdecoration = dec;
				//_manipulator.SetDecoration(dec);
			}
		}
		private static int ParseSGRCode(string param) {
			if(param.Length==0)
				return 0;
			else if(param.Length==1)
				return param[0]-'0';
			else if(param.Length==2)
				return (param[0]-'0')*10 + (param[1]-'0');
			else
				throw new UnknownEscapeSequenceException(String.Format("unknown SGR parameter {0}", param));
		}

		protected virtual void ProcessDeviceAttributes(string param) {
			byte[] data = Encoding.ASCII.GetBytes(" [?1;2c"); //�Ȃ񂩂悭�킩��Ȃ���MindTerm�����݂�Ƃ���ł����炵��
			data[0] = 0x1B; //ESC
			GetConnection().Write(data);
		}
		protected virtual void ProcessDeviceStatusReport(string param) {
			string response;
			if(param=="5")
				response = " [0n"; //�����OK�̈Ӗ��炵��
			else if(param=="6")
				response = String.Format(" [{0};{1}R", GetDocument().CurrentLineNumber-GetDocument().TopLineNumber+1, _manipulator.CaretColumn+1);
			else
				throw new UnknownEscapeSequenceException("DSR " + param);

			byte[] data = Encoding.ASCII.GetBytes(response);
			data[0] = 0x1B; //ESC
			GetConnection().Write(data);
		}

		protected void ProcessCursorMove(string param, char method) {
			int count = ParseInt(param, 1); //�p�����[�^���ȗ����ꂽ�Ƃ��̈ړ��ʂ͂P

			int column = _manipulator.CaretColumn;
			switch(method) {
				case 'A':
					GetDocument().ReplaceCurrentLine(_manipulator.Export());
					GetDocument().CurrentLineNumber = (GetDocument().CurrentLineNumber - count);
					_manipulator.Load(GetDocument().CurrentLine, column);
					break;
				case 'B':
					GetDocument().ReplaceCurrentLine(_manipulator.Export());
					GetDocument().CurrentLineNumber = (GetDocument().CurrentLineNumber + count);
					_manipulator.Load(GetDocument().CurrentLine, column);
					break;
				case 'C': {
					int newvalue = column + count;
					if(newvalue >= GetConnection().TerminalWidth) newvalue = GetConnection().TerminalWidth-1;
					_manipulator.ExpandBuffer(newvalue);
					_manipulator.CaretColumn = newvalue;
				}
					break;
				case 'D': {
					int newvalue = column - count;
					if(newvalue < 0) newvalue = 0;
					_manipulator.CaretColumn = newvalue;
				}
					break;
			}
		}

		//CSI H
		protected void ProcessCursorPosition(string param) {
			IntPair t = ParseIntPair(param, 1, 1);
			int row = t.first, col = t.second;
			if(_scrollRegionRelative && GetDocument().ScrollingTop!=-1) {
				row += GetDocument().ScrollingTop;
			}

			if(row<1) row=1;
			else if(row>GetConnection().TerminalHeight) row = GetConnection().TerminalHeight;
			if(col<1) col=1;
			else if(col>GetConnection().TerminalWidth) col = GetConnection().TerminalWidth;
			ProcessCursorPosition(row, col);
		}
		protected void ProcessCursorPosition(int row, int col) {
			GetDocument().ReplaceCurrentLine(_manipulator.Export());
			GetDocument().CurrentLineNumber = (GetDocument().TopLineNumber + row - 1);
			//int cc = GetDocument().CurrentLine.DisplayPosToCharPos(col-1);
			//Debug.Assert(cc>=0);
			_manipulator.Load(GetDocument().CurrentLine, col-1);
		}

		//CSI J
		protected void ProcessEraseInDisplay(string param) {
			int d = ParseInt(param, 0);

			TerminalDocument doc = GetDocument();
			int col = _manipulator.CaretColumn;
			switch(d) {
				case 0: //erase below
					_manipulator.RemoveAfterCaret(); 
					doc.ReplaceCurrentLine(_manipulator.Export());
					doc.RemoveAfter(doc.TopLineNumber+GetConnection().TerminalHeight);
					if(_currentdecoration.IsDefault)
						doc.ClearAfter(doc.CurrentLineNumber+1);
					else
						doc.ClearAfter(doc.CurrentLineNumber+1, _currentdecoration);
					_manipulator.Load(doc.CurrentLine, col);
					break;
				case 1: //erase above
					_manipulator.FillSpace(0, _manipulator.CaretColumn);
					doc.ReplaceCurrentLine(_manipulator.Export());
					if(_currentdecoration.IsDefault)
						doc.ClearRange(doc.TopLineNumber, doc.CurrentLineNumber);
					else
						doc.ClearRange(doc.TopLineNumber, doc.CurrentLineNumber, _currentdecoration);
					_manipulator.Load(doc.CurrentLine, col);
					break;
				case 2: //erase all
					doc.ReplaceCurrentLine(_manipulator.Export());
					if(_homePositionOnCSIJ2) { //SFU�ł͂����Ȃ�
						ProcessCursorPosition(1,1); 
						col = 0;
					}
					if(_currentdecoration.IsDefault)
						doc.ClearAfter(doc.TopLineNumber);
					else
						doc.ClearAfter(doc.TopLineNumber, _currentdecoration);
					_manipulator.Load(doc.CurrentLine, col);
					break;
				default:
					throw new UnknownEscapeSequenceException(String.Format("unknown ED option {0}", param));
			}

		}

		//CSI K
		private void ProcessEraseInLine(string param) {
			int d = ParseInt(param, 0);
			
			switch(d) {
				case 0: //erase right
					if(_currentdecoration.IsDefault)
						_manipulator.RemoveAfterCaret();
					else 
						_manipulator.FillSpace(_manipulator.CaretColumn, _tag.Connection.TerminalWidth, _currentdecoration);
					break;
				case 1: //erase left
					_manipulator.FillSpace(0, _manipulator.CaretColumn);
					break;
				case 2: //erase all
					_manipulator.Clear(_tag.Connection.TerminalWidth);
					break;
				default:
					throw new UnknownEscapeSequenceException(String.Format("unknown EL option {0}", param));
			}
		}

		protected void SaveCursor() {
			_savedRow = GetDocument().CurrentLineNumber - GetDocument().TopLineNumber;
			_savedCol = _manipulator.CaretColumn;
		}
		protected void RestoreCursor() {
			GLine nl = _manipulator.Export();
			GetDocument().ReplaceCurrentLine(nl);
			GetDocument().CurrentLineNumber = GetDocument().TopLineNumber + _savedRow;
			_manipulator.Load(GetDocument().CurrentLine, _savedCol);
		}

		protected void Index() {
			GLine nl = _manipulator.Export();
			GetDocument().ReplaceCurrentLine(nl);
			int current = GetDocument().CurrentLineNumber;
			if(current==GetDocument().TopLineNumber+GetConnection().TerminalHeight-1 || current==GetDocument().ScrollingBottom)
				GetDocument().ScrollDown();
			else
				GetDocument().CurrentLineNumber = current+1;
			_manipulator.Load(GetDocument().CurrentLine, _manipulator.CaretColumn);
		}
		protected void ReverseIndex() {
			GLine nl = _manipulator.Export();
			GetDocument().ReplaceCurrentLine(nl);
			int current = GetDocument().CurrentLineNumber;
			if(current==GetDocument().TopLineNumber || current==GetDocument().ScrollingTop)
				GetDocument().ScrollUp();
			else
				GetDocument().CurrentLineNumber = current-1;
			_manipulator.Load(GetDocument().CurrentLine, _manipulator.CaretColumn);
		}

		protected void ProcessSetScrollingRegion(string param) {
			int height = _tag.Connection.TerminalHeight;
			IntPair v = ParseIntPair(param, 1, height);
			
			if(v.first<1) v.first = 1;
			else if(v.first>height) v.first = height;
			if(v.second<1) v.second = 1;
			else if(v.second>height) v.second = height;
			if(v.first>v.second) { //�ⓚ���p�ŃG���[���ǂ��悤�ɂ��v����
				int t = v.first;
				v.first = v.second;
				v.second = t;
			}

			//�w���1-origin����������0-origin
			GetDocument().SetScrollingRegion(v.first-1, v.second-1);
		}

		protected void ProcessNextLine() {
			GetDocument().ReplaceCurrentLine(_manipulator.Export());
			GetDocument().CurrentLineNumber = (GetDocument().CurrentLineNumber + 1);
			_manipulator.Load(GetDocument().CurrentLine, 0);
		}

		protected override void ChangeMode(TerminalMode mode) {
			if(_terminalMode==mode) return;

			if(mode==TerminalMode.Normal) {
				GetDocument().ClearScrollingRegion();
				GetConnection().Resize(GetConnection().TerminalWidth, GetConnection().TerminalHeight); //���Ƃ���emacs�N�����Ƀ��T�C�Y���A�V�F���֖߂�ƃV�F���͐V�����T�C�Y��F�����Ă��Ȃ�
				//RMBox�Ŋm�F���ꂽ���Ƃ����A���p�Ɍ���Ƀh�L�������g���L���Ă���z������B�J�[�\����123�����ցA�ȂǁB
				//�ꓖ����I�����A�m�[�}�����[�h�ɖ߂�ۂɌ��̋�s���폜���邱�ƂőΉ�����B
				GLine l = GetDocument().LastLine;
				while(l!=null && l.DisplayLength==0 && l.ID>GetDocument().CurrentLineNumber)
					l = l.PrevLine;

				if(l!=null)	l = l.NextLine;
				if(l!=null)	GetDocument().RemoveAfter(l.ID);
			}
			else
				GetDocument().SetScrollingRegion(0, _tag.Connection.TerminalHeight-1);

			_terminalMode = mode;
		}

		private ProcessCharResult ProcessDECSETMulti(string param, char code) {
			if(param.Length==0) return ProcessCharResult.Processed;
			bool question = param[0]=='?';
			string[] ps = question? param.Substring(1).Split(';') : param.Split(';');
			bool unsupported = false;
			foreach(string p in ps) {
				ProcessCharResult r = question? ProcessDECSET(p, code) : ProcessSetMode(p, code);
				if(r==ProcessCharResult.Unsupported) unsupported = true;
			}
			return unsupported? ProcessCharResult.Unsupported : ProcessCharResult.Processed;
		}

		//CSI ? Pm h, CSI ? Pm l
		protected virtual ProcessCharResult ProcessDECSET(string param, char code) {
			//Debug.WriteLine(String.Format("DECSET {0} {1}", param, code));
			if(param=="25") {
				return ProcessCharResult.Processed; //!!Show/Hide Cursor�����Ƃ肠��������
			}
			else if(param=="1") {
				ChangeCursorKeyMode(code=='h'? TerminalMode.Application : TerminalMode.Normal);
				return ProcessCharResult.Processed;
			}
			else
				return ProcessCharResult.Unsupported;
		}
		protected virtual ProcessCharResult ProcessSetMode(string param, char code) {
			if(param=="4") {
				_insertMode = code=='h'; //h�Ŏn�܂���l�ŏI���
				return ProcessCharResult.Processed;
			}
			else if(param=="20") {
				return ProcessCharResult.Processed; //!!WinXP��Telnet�Ŋm�F����
			}
			else
				return ProcessCharResult.Unsupported;
		}

		//����͂��ڂ�B�����ƕۑ����Ȃ��Ƃ����Ȃ���Ԃ͂قƂ�ǂȂ��̂�
		protected virtual ProcessCharResult ProcessSaveDECSET(string param, char code) {
			//����param�͕����p�����[�^
			return ProcessCharResult.Processed;
		}
		protected virtual ProcessCharResult ProcessRestoreDECSET(string param, char code) {
			//����param�͕����p�����[�^
			return ProcessCharResult.Processed;
		}

		//����𑗂��Ă���A�v���P�[�V������ vi�ŏ���X�N���[��
		protected void ProcessInsertLines(string param) {
			int d = ParseInt(param, 1);

			/*
			TerminalDocument doc = GetDocument();
			for(int i=0; i<d; i++)
				doc.InsertAfter(doc.CurrentLine, new GLine(GetConnection().TerminalWidth));
			*/
			TerminalDocument doc = GetDocument();
			int caret_pos = _manipulator.CaretColumn;
			int offset = doc.CurrentLineNumber - doc.TopLineNumber;
			GLine nl = _manipulator.Export();
			doc.ReplaceCurrentLine(nl);
			if(doc.ScrollingBottom==-1)
				doc.SetScrollingRegion(0, GetConnection().TerminalHeight-1);

			for(int i=0; i<d; i++) {
				doc.ScrollUp(doc.CurrentLineNumber, doc.ScrollingBottom);
				doc.CurrentLineNumber = doc.TopLineNumber + offset;
			}
			_manipulator.Load(doc.CurrentLine, caret_pos);
		}

		//����𑗂��Ă���A�v���P�[�V������ vi�ŉ����X�N���[��
		protected void ProcessDeleteLines(string param) {
			int d = ParseInt(param, 1);

			/*
			TerminalDocument doc = GetDocument();
			_manipulator.Clear(GetConnection().TerminalWidth);
			GLine target = doc.CurrentLine;
			for(int i=0; i<d; i++) {
				target.Clear();
				target = target.NextLine;
			}
			*/

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



		private static string[] FUNCTIONKEY_MAP = { 
		//     F1    F2    F3    F4    F5    F6    F7    F8    F9    F10   F11  F12
			  "11", "12", "13", "14", "15", "17", "18", "19", "20", "21", "23", "24",
	    //     F13   F14   F15   F16   F17  F18   F19   F20   F21   F22
              "25", "26", "28", "29", "31", "32", "33", "34", "23", "24" };
		//����̃f�[�^�𗬂��^�C�v�B���݁A�J�[�\���L�[�ƃt�@���N�V�����L�[���Y������         
		public override byte[] SequenceKeyData(Keys modifier, Keys body) {
			if((int)Keys.F1 <= (int)body && (int)body <= (int)Keys.F12) {
				byte[] r = new byte[5];
				r[0] = 0x1B;
				r[1] = (byte)'[';
				int n = (int)body - (int)Keys.F1;
				if((modifier & Keys.Shift) != Keys.None) n += 10; //shift�͒l��10���炷
				char tail;
				if(n>=20)
					tail = (modifier & Keys.Control) != Keys.None? '@' : '$';
				else
					tail = (modifier & Keys.Control) != Keys.None? '^' : '~';
				string f = FUNCTIONKEY_MAP[n];
				r[2] = (byte)f[0];
				r[3] = (byte)f[1];
				r[4] = (byte)tail;
				return r;
			}
			else if(GUtil.IsCursorKey(body)) {
				byte[] r = new byte[3];
				r[0] = 0x1B;
				if(_cursorKeyMode==TerminalMode.Normal)
					r[1] = (byte)'[';
				else
					r[1] = (byte)'O';

				switch(body) {
					case Keys.Up:
						r[2] = (byte)'A';
						break;
					case Keys.Down:
						r[2] = (byte)'B';
						break;
					case Keys.Right:
						r[2] = (byte)'C';
						break;
					case Keys.Left:
						r[2] = (byte)'D';
						break;
					default:
						throw new ArgumentException("unknown cursor key code", "key");
				}
				return r;
			}
			else {
				byte[] r = new byte[4];
				r[0] = 0x1B;
				r[1] = (byte)'[';
				r[3] = (byte)'~';
				if(body==Keys.Insert)
					r[2] = (byte)'1';
				else if(body==Keys.Home)
					r[2] = (byte)'2';
				else if(body==Keys.PageUp)
					r[2] = (byte)'3';
				else if(body==Keys.Delete)
					r[2] = (byte)'4';
				else if(body==Keys.End)
					r[2] = (byte)'5';
				else if(body==Keys.PageDown)
					r[2] = (byte)'6';
				else
					throw new ArgumentException("unknown key " + body.ToString());
				return r;
			}
		}
	}
}

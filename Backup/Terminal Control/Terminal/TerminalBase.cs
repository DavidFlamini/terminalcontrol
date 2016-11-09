/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalBase.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Log;
using Poderosa.Config;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Text;
using Poderosa.Communication;

namespace Poderosa.Terminal
{
	/// <summary>
	/// �^�[�~�i��
	/// �f�[�^����M���ăh�L�������g�𑀍삷��@�\�����B
	/// </summary>
	internal abstract class AbstractTerminal : ITerminal {
		public abstract void ProcessChar(char ch);
		public abstract ProcessCharResult State { get; }
		public abstract byte[] SequenceKeyData(Keys modifier, Keys body);

		protected StringBuilder _bufferForMacro;
		protected AutoResetEvent _signalForMacro; //�}�N���X���b�h�Ƀf�[�^�̓ǂݎ��\��m�点��
		protected ICharDecoder     _decoder;
		protected GLineManipulator _manipulator;
		protected ITerminalTextLogger _logger; //Logger�v���p�e�B�Ƃ̏d�������Ƃ�������

		public ITerminalTextLogger Logger {
			get {
				return _tag.Connection.TextLogger;
			}
		}

		protected ConnectionTag  _tag;
		protected TextDecoration _currentdecoration;
		
		protected TerminalMode _terminalMode;
		protected TerminalMode _cursorKeyMode; //_terminalMode�͕ʕ��BAIX�ł�vi�ŁA�J�[�\���L�[�͕s�ςƂ����Ⴊ�m�F����Ă���

		public TerminalMode TerminalMode {
			get {
				return _terminalMode;
			}
		}
		public TerminalMode CursorKeyMode {
			get {
				return _cursorKeyMode;
			}
		}
		protected TerminalConnection GetConnection() {
			return _tag.Connection;
		}
		protected TerminalDocument GetDocument() {
			return _tag.Document;
		}

		protected abstract void ChangeMode(TerminalMode tm);
		protected abstract void ResetInternal();

		protected virtual void ChangeCursorKeyMode(TerminalMode tm) {
			_cursorKeyMode = tm;
		}

		/// <summary>
		/// ����̑ΏۂɂȂ�h�L�������g�ƕ����̃G���R�[�f�B���O���w�肵�č\�z
		/// </summary>
		public AbstractTerminal(ConnectionTag tag, ICharDecoder decoder) {
			_tag = tag;
			_decoder = decoder;
			_terminalMode = TerminalMode.Normal;
			_currentdecoration = TextDecoration.Default;
			_manipulator = new GLineManipulator(80);
			_bufferForMacro = new StringBuilder();
			_signalForMacro = new AutoResetEvent(false);
		}
		public void Input(byte[] data, int offset, int length) {
			_manipulator.Load(GetDocument().CurrentLine, 0);
			_manipulator.CaretColumn = GetDocument().CaretColumn;
			_manipulator.DefaultDecoration = _currentdecoration;
			
			_decoder.Input(this, data, offset, length);

			GetDocument().ReplaceCurrentLine(_manipulator.Export());
			GetDocument().CaretColumn = _manipulator.CaretColumn;
		}
		public void Input(char[] data, int offset, int length) {
			_manipulator.Load(GetDocument().CurrentLine, 0);
			_manipulator.CaretColumn = GetDocument().CaretColumn;
			_manipulator.DefaultDecoration = _currentdecoration;
			
			for(int i=0; i<length; i++)
				ProcessChar(data[offset+i]);

			GetDocument().ReplaceCurrentLine(_manipulator.Export());
			GetDocument().CaretColumn = _manipulator.CaretColumn;
		}

		public void UnsupportedCharSetDetected(char code) {
			string desc;
			if(code=='0')
				desc = "0 (DEC Special Character)"; //����͂悭����̂ŒA��������
			else
				desc = new String(code, 1);

			if(GEnv.Options.WarningOption!=WarningOption.Ignore) {
				GEnv.InterThreadUIService.UnsupportedCharSetDetected(GetDocument(), String.Format(GEnv.Strings.GetString("Message.AbstractTerminal.UnsupportedCharSet"), desc));
			}
		}
		public void InvalidCharDetected(Encoding enc, byte[] buf) {
			if(GEnv.Options.WarningOption!=WarningOption.Ignore) {
				GEnv.InterThreadUIService.InvalidCharDetected(GetDocument(), String.Format(GEnv.Strings.GetString("Message.AbstractTerminal.UnexpectedChar"), enc.WebName));
			}
		}
		public void Reset() {
			//Encoding���������͊ȒP�ɍς܂��邱�Ƃ��ł���
			if(_decoder.Encoding.Type==_tag.Connection.Param.Encoding)
				_decoder.Reset(_decoder.Encoding);
			else
				_decoder = new JapaneseCharDecoder(_tag.Connection);
		}
		//public void SetEncoding(EncodingProfile enc) {
		//	_decoder.SetEncoding(enc);
		//}

		public void ClearMacroBuffer() {
			_bufferForMacro.Remove(0, _bufferForMacro.Length);
			_signalForMacro.Reset();
		}
		public void SignalData() {
			_signalForMacro.Set();
		}
		protected void AppendMacroBuffer(char ch) {
			if(ch!='\r' && ch!='\0') {
				lock(_bufferForMacro) {
					_bufferForMacro.Append(ch); //!!�����ɏ���������ق������S����
				}
			}
		}
		//�}�N�����s�X���b�h����Ă΂��P�s�ǂݏo�����\�b�h
		public string ReadLineFromMacroBuffer() {
			do {
				int l = _bufferForMacro.Length;
				int i=0;
				for(i=0; i<l; i++) {
					if(_bufferForMacro[i]=='\n') break;
				}

				if(l>0 && i<l) { //�߂ł����s�����݂�����
					int j=i;
					if(i>0 && _bufferForMacro[i-1]=='\r') j=i-1; //CRLF�̂Ƃ��͏����Ă��
					string r;
					lock(_bufferForMacro) {
						r = _bufferForMacro.ToString(0, j);
						_bufferForMacro.Remove(0, i+1);
					}
					return r;
				}
				else {
					_signalForMacro.Reset();
					_signalForMacro.WaitOne();
				}
			} while(true);
		}
		//�}�N�����s�X���b�h����Ă΂��A�u�����f�[�^������ΑS�������Ă����v���\�b�h
		public string ReadAllFromMacroBuffer() {
			if(_bufferForMacro.Length==0) {
				_signalForMacro.Reset();
				_signalForMacro.WaitOne();
			}

			lock(_bufferForMacro) {
				string r = _bufferForMacro.ToString();
				_bufferForMacro.Remove(0, _bufferForMacro.Length);
				return r;
			}
		}

		//����̓��C���X���b�h����Ăяo������
		public virtual void FullReset() {
			lock(_tag.Document) {
				ChangeMode(TerminalMode.Normal);
				_tag.Document.ClearScrollingRegion();
				ResetInternal();
				_decoder = new JapaneseCharDecoder(_tag.Connection);
			}
		}

		public void DumpCurrentText() {
			Debug.WriteLine(_manipulator.ToString());
		}

	}
	
	//Escape Sequence���g���^�[�~�i��
	internal abstract class EscapeSequenceTerminal : AbstractTerminal {
		public EscapeSequenceTerminal(ConnectionTag tag, ICharDecoder decoder) : base(tag, decoder) {
			_escapeSequence = new StringBuilder();
			_processCharResult = ProcessCharResult.Processed;
		}

		private StringBuilder _escapeSequence;
		private ProcessCharResult _processCharResult;

		public override ProcessCharResult State {
			get {
				return _processCharResult;
			}
		}
		protected override void ResetInternal() {
			_escapeSequence = new StringBuilder();
			_processCharResult = ProcessCharResult.Processed;
		}

		public override void ProcessChar(char ch) {
			
			_logger = Logger; //_logger�͂���ProcessChar�̏������ł̂ݗL���B
			
			if(_processCharResult != ProcessCharResult.Escaping) {
				if(ch==0x1B) {
					_processCharResult = ProcessCharResult.Escaping;
				} else {
					//!!�v���Ԃ�̂��̂����������Ƃ��������������򂾂�
					_logger.Append(ch);
					if(GEnv.Frame.MacroIsRunning) AppendMacroBuffer(ch);

					if(ch < 0x20 || (ch>=0x80 && ch<0xA0))
						_processCharResult = ProcessControlChar(ch);
					else
						_processCharResult = ProcessNormalChar(ch);
				}
			}
			else {
				if(ch=='\0') return; //�V�[�P���X����NULL�����������Ă���P�[�X���m�F���ꂽ
				_escapeSequence.Append(ch);
				bool end_flag = false; //escape sequence�̏I��肩�ǂ����������t���O
				if(_escapeSequence.Length==1) { //ESC+�P�����ł���ꍇ
					end_flag = ('0'<=ch && ch<='9') || ('a'<=ch && ch<='z') || ('A'<=ch && ch<='Z') || ch=='>' || ch=='=' || ch=='|' || ch=='}' || ch=='~';
				}
				else if(_escapeSequence[0]==']') { //OSC�̏I�[��BEL��ST(String Terminator)
					end_flag = ch==0x07 || ch==0x9c; 
				}
				else {
					end_flag = ('a'<=ch && ch<='z') || ('A'<=ch && ch<='Z') || ch=='@' || ch=='~' || ch=='|' || ch=='{';
				}
				
				if(end_flag) { //�V�[�P���X�̂����
					char[] seq = _escapeSequence.ToString().ToCharArray();
					_logger.BeginEscapeSequence();
					_logger.Append(seq, 0, seq.Length);
					_logger.CommitEscapeSequence();
					_logger.Flush();
					try {
						char code = seq[0];
						_processCharResult = ProcessCharResult.Unsupported; //ProcessEscapeSequence�ŗ�O��������ŏ�Ԃ�Escaping�͂Ђǂ����ʂ������̂�
						_processCharResult = ProcessEscapeSequence(code, seq, 1);
						if(_processCharResult==ProcessCharResult.Unsupported)
							throw new UnknownEscapeSequenceException(String.Format("ESC {0}", new string(seq)));
					}
					catch(UnknownEscapeSequenceException ex) {
						if(GEnv.Options.WarningOption!=Poderosa.Config.WarningOption.Ignore)
							GEnv.InterThreadUIService.UnsupportedEscapeSequence(GetDocument(), GEnv.Strings.GetString("Message.EscapesequenceTerminal.UnsupportedSequence")+ex.Message);
					}
					finally {
						_escapeSequence.Remove(0, _escapeSequence.Length);
					}
				}
				else
					_processCharResult = ProcessCharResult.Escaping;
			}
		}

		protected virtual ProcessCharResult ProcessControlChar(char ch) {
			if(ch=='\n' || ch==0xB) { //Vertical Tab��LF�Ɠ�����
				LineFeedRule rule = GetConnection().Param.LineFeedRule;
				if(rule==LineFeedRule.Normal || rule==LineFeedRule.LFOnly) {
					if(rule==LineFeedRule.LFOnly) //LF�݂̂̓���ł���Ƃ�
						DoCarriageReturn();
					DoLineFeed();
				}
				return ProcessCharResult.Processed;
			}
			else if(ch=='\r') {
				LineFeedRule rule = GetConnection().Param.LineFeedRule;
				if(rule==LineFeedRule.Normal || rule==LineFeedRule.CROnly) {
					DoCarriageReturn();
					if(rule==LineFeedRule.CROnly)
						DoLineFeed();
				}
				return ProcessCharResult.Processed;
			}
			else if(ch==0x07) {
				_tag.Receiver.IndicateBell();
				return ProcessCharResult.Processed;
			}
			else if(ch==0x08) {
				//�s���ŁA���O�s�̖������p���ł������ꍇ�s��߂�
				if(_manipulator.CaretColumn==0) {
					TerminalDocument doc = GetDocument();
					int line = doc.CurrentLineNumber-1;
					if(line>=0 && doc.FindLineOrEdge(line).EOLType==EOLType.Continue) {
						doc.InvalidateLine(doc.CurrentLineNumber);
						doc.CurrentLineNumber = line;
						if(doc.CurrentLine==null)
							_manipulator.Clear(GetConnection().TerminalWidth);
						else
							_manipulator.Load(doc.CurrentLine, doc.CurrentLine.CharLength-1);
						doc.InvalidateLine(doc.CurrentLineNumber);
					}
				}
				else
					_manipulator.BackCaret();

				return ProcessCharResult.Processed;
			}
			else if(ch==0x09) {
				_manipulator.CaretColumn = GetNextTabStop(_manipulator.CaretColumn);
				return ProcessCharResult.Processed;
			}
			else if(ch==0x0E) {
				return ProcessCharResult.Processed; //�ȉ��Q��CharDecoder�̒��ŏ�������Ă���͂��Ȃ̂Ŗ���
			}
			else if(ch==0x0F) {
				return ProcessCharResult.Processed;
			}
			else if(ch==0x00) {
				return ProcessCharResult.Processed; //null char�͖��� !!CR NUL��CR LF�Ƃ݂Ȃ��d�l�����邪�ACR LF CR NUL�Ƃ��邱�Ƃ������ē��
			}
			else {
				//Debug.WriteLine("Unknown char " + (int)ch);
				//�K���ȃO���t�B�b�N�\���ق���
				return ProcessCharResult.Unsupported;
			}
		}
		private void DoLineFeed() {
			GLine nl = _manipulator.Export();
			nl.EOLType = (nl.EOLType==EOLType.CR || nl.EOLType==EOLType.CRLF)? EOLType.CRLF : EOLType.LF;
			_logger.WriteLine(nl); //���O�ɍs��commit
			GetDocument().ReplaceCurrentLine(nl);
			GetDocument().LineFeed();
				
			//�J�����ێ��͕K�v�B�T���v��:linuxconf.log
			int col = _manipulator.CaretColumn;
			_manipulator.Load(GetDocument().CurrentLine, col);
		}
		private void DoCarriageReturn() {
			_manipulator.CarriageReturn();
		}

		protected virtual int GetNextTabStop(int start) {
			int t = start;
			//t���ōŏ��̂W�̔{���ւ����Ă���
			t += (8 - t % 8);
			if(t >= _tag.Connection.TerminalWidth) t = _tag.Connection.TerminalWidth-1;
			return t;
		}
		
		protected virtual ProcessCharResult ProcessNormalChar(char ch) {
			//���ɉ�ʉE�[�ɃL�����b�g������̂ɕ�������������s������
			int tw = _tag.Connection.TerminalWidth;
			if(_manipulator.CaretColumn+GLine.CalcDisplayLength(ch) > tw) {
				GLine l = _manipulator.Export();
				l.EOLType = EOLType.Continue;
				GetDocument().ReplaceCurrentLine(l);
				GetDocument().LineFeed();
				_manipulator.Load(GetDocument().CurrentLine, 0);
			}

			//��ʂ̃��T�C�Y���������Ƃ��́A_manipulator�̃o�b�t�@�T�C�Y���s���̉\��������
			if(tw > _manipulator.BufferSize)
				_manipulator.ExpandBuffer(tw);

			//�ʏ핶���̏���
			_manipulator.PutChar(ch, _currentdecoration);
			
			return ProcessCharResult.Processed;
		}

		protected abstract ProcessCharResult ProcessEscapeSequence(char code, char[] seq, int offset);

		//FormatException�̂ق���OverflowException�̉\��������̂�
		protected static int ParseInt(string param, int default_value) {
			try {
				if(param.Length>0)
					return Int32.Parse(param);
				else
					return default_value;
			}
			catch(Exception ex) {
				throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", param, ex.Message));
			}
		}

		protected static IntPair ParseIntPair(string param, int default_first, int default_second) {
			IntPair ret = new IntPair(default_first, default_second);

			string[] s = param.Split(';');
			
			if(s.Length >= 1 && s[0].Length>0) {
				try {
					ret.first = Int32.Parse(s[0]);
				}
				catch(Exception ex) {
					throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", s[0], ex.Message));
				}
			}

			if(s.Length >= 2 && s[1].Length>0) {
				try {
					ret.second = Int32.Parse(s[1]);
				}
				catch(Exception ex) {
					throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", s[1], ex.Message));
				}
			}

			return ret;
		}
	}
}

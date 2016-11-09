/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: SelectionKeyProcessor.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa;
using Poderosa.Terminal;
using Poderosa.Forms;

namespace Poderosa.Text
{
	/// <summary>
	/// �e�L�X�g�I�����[�h�ɓ�������ŃL�[���������đI��̈�̃R���g���[�����s��
	/// </summary>
	internal class SelectionKeyProcessor {
		private TerminalPane _owner;
		//���݂̃J�[�\���ʒu
		private GLine _currentLine;
		private TerminalDocument _document;
		private int _caretPos;

		public TerminalPane Owner {
			get {
				return _owner;
			}
		}
		public GLine CurrentLine {
			get {
				return _currentLine;
			}
		}

		public int CaretPos {
			get {
				return _caretPos;
			}
		}
		//�\����̃L�����b�g�ʒu�B�s�̉E�[�����������ɕ\�����Ȃ��悤�ɂ��邽��
		public int UICaretPos {
			get {
				if(_caretPos>_currentLine.CharLength)
					return _currentLine.CharLength;
				else
					return _caretPos;
			}
		}


		public SelectionKeyProcessor(TerminalPane owner, TerminalDocument doc, GLine line, int pos) {
			_owner = owner;
			_document = doc;
			Debug.Assert(line!=null);
			_currentLine = line;
			_caretPos = pos;
		}

		public bool ProcessKey(Keys key) {
			Keys body = key & Keys.KeyCode;
			bool shift = (key & Keys.Shift) != Keys.None;
			bool control = (key & Keys.Control) != Keys.None;
			bool processed = false;

			//�ړ���̍s�ƌ��̌v�Z
			GLine nextLine = _currentLine;
			_document.InvalidateLine(nextLine.ID);
			if(body==Keys.Up) {
				if(_currentLine.PrevLine!=null) nextLine = _currentLine.PrevLine;
				_document.InvalidateLine(nextLine.ID);
				processed = true;
			}
			else if(body==Keys.Down) {
				if(_currentLine.NextLine!=null) nextLine = _currentLine.NextLine;
				_document.InvalidateLine(nextLine.ID);
				processed = true;
			}
			else if(body==Keys.PageUp) {
				int n = _currentLine.ID - _owner.Connection.TerminalHeight;
				nextLine = n<=_document.FirstLineNumber? _document.FirstLine : _document.FindLine(n);
				_document.InvalidateAll();
				processed = true;
			}
			else if(body==Keys.PageDown) {
				int n = _currentLine.ID + _owner.Connection.TerminalHeight;
				nextLine = n>=_document.LastLineNumber? _document.LastLine : _document.FindLine(n);
				_document.InvalidateAll();
				processed = true;
			}

			int nextPos = _caretPos;
			if(body==Keys.Home) {
				nextPos = 0;
				processed = true;
			}
			else if(body==Keys.End) {
				nextPos = _currentLine.CharLength-1;
				processed = true;
			}
			else if(body==Keys.Left) {
				if(nextPos>0) {
					if(control)
						nextPos = _currentLine.FindPrevWordBreak(nextPos-1)+1;
					else
						nextPos--;
				}
				processed = true;
			}
			else if(body==Keys.Right) {
				if(nextPos<_currentLine.CharLength-1) {
					if(control)
						nextPos = _currentLine.FindNextWordBreak(nextPos+1);
					else
						nextPos++;
				}
				processed = true;
			}

			//�I��̈�̒���
			TextSelection sel = GEnv.TextSelection;
			if(shift && processed) {
				if(sel.IsEmpty)
					sel.StartSelection(_owner, _currentLine, _caretPos, RangeType.Char, -1, -1);
				sel.ExpandTo(nextLine, nextPos, RangeType.Char);
			}
			else if(processed || body==Keys.Menu || body==Keys.ControlKey || body==Keys.ShiftKey) {
				if(processed)
					sel.Clear();
				processed = true;
			}
			else {
				//��ʃL�[�̓��͂��������瑦���I������
				sel.Clear();
			}

			Debug.Assert(nextLine!=null);
			_currentLine = nextLine;
			_caretPos = nextPos;
			return processed;
		}
	}
}

/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TextSelection.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Text;
using System.Diagnostics;

using Poderosa.Terminal;

namespace Poderosa.Text
{
	public enum RangeType {
		Char,
		Word,
		Line
	}
	public enum SelectionState {
		Empty,
		Pivot,
		Expansion,
		Fixed
	}

	/// <summary>
	/// �e�L�X�g�̑I��̈��\������
	/// </summary>
	public class TextSelection {
		
		//�[�_
		internal struct TextPoint {
			public int   _line;
			public int   _position;

			public void Clear() {
				_line = -1;
				_position = 0;
			}
		}

		private SelectionState _state;

		private TerminalPane _owner;
		//�ŏ��̑I��_�B�P���s��I�������Ƃ��̂��߂ɂQ�݂���
		private TextPoint _forwardPivot;
		private TextPoint _backwardPivot;
		//�I���̍ŏI�_
		private TextPoint _forwardDestination;
		private TextPoint _backwardDestination;

		//pivot�̏��
		private RangeType _pivotType;

		//�I�����J�n�����Ƃ��̃}�E�X���W
		private int _startX;
		private int _startY;

		//������Ɖ����t���O
		private bool _disabledTemporary;

		public TextSelection() {
			_owner = null;
		}

		public TerminalPane Owner {
			get {
				return _owner;
			}
		}
		public SelectionState State {
			get {
				return _state;
			}
		}
		public RangeType PivotType {
			get {
				return _pivotType;
			}
		}
		public int StartX {
			get {
				return _startX;
			}
		}
		public int StartY {
			get {
				return _startY;
			}
		}


		public void Clear() {
			if(_owner!=null) {
				_owner.ExitTextSelection();
			}
			_owner = null;
			_state = SelectionState.Empty;
			_pivotType = RangeType.Char;
			_forwardPivot.Clear();
			_backwardPivot.Clear();
			_forwardDestination.Clear();
			_backwardDestination.Clear();
			_disabledTemporary = false;
		}

		public void DisableTemporary() {
			_disabledTemporary = true;
		}
		
		//�h�L�������g��Discard���ꂽ�Ƃ��ɌĂ΂��Bfirst_line���O�ɑI��̈悪�d�Ȃ��Ă�����N���A����
		public void ClearIfOverlapped(int first_line) {
			if(_forwardPivot._line!=-1 && _forwardPivot._line<first_line) {
				_forwardPivot._line = first_line;
				_forwardPivot._position = 0;
				_backwardPivot._line = first_line;
				_backwardPivot._position = 0;
			}
			
			if(_forwardDestination._line!=-1 && _forwardDestination._line<first_line) {
				_forwardDestination._line = first_line;
				_forwardDestination._position = 0;
				_backwardDestination._line = first_line;
				_backwardDestination._position = 0;
			}
		}

		public bool IsEmpty {
			get {
				return _owner==null || _forwardPivot._line==-1 || _backwardPivot._line==-1 |
					_forwardDestination._line==-1 || _backwardDestination._line==-1 || _disabledTemporary;
			}
		}
		
		internal bool StartSelection(TerminalPane owner, GLine line, int position, RangeType type, int x, int y) {
			Debug.Assert(owner!=null);
			Debug.Assert(position>=0);
			line.ExpandBuffer(position+1);

			_disabledTemporary = false;
			_owner = owner;
			_pivotType = type;
			_forwardPivot._line = line.ID;
			_backwardPivot._line = line.ID;
			_forwardDestination._line = line.ID;
			_forwardDestination._position = position;
			_backwardDestination._line = line.ID;
			_backwardDestination._position = position;
			switch(type) {
				case RangeType.Char:
					_forwardPivot._position = position;
					_backwardPivot._position = position;
					break;
				case RangeType.Word:
					_forwardPivot._position = line.FindPrevWordBreak(position)+1;
					_backwardPivot._position = line.FindNextWordBreak(position);
					break;
				case RangeType.Line:
					_forwardPivot._position = 0;
					_backwardPivot._position = line.CharLength;
					break;
			}
			_state = SelectionState.Pivot;
			_startX = x;
			_startY = y;
			return true;
		}

		internal bool ExpandTo(GLine line, int position, RangeType type) {
			line.ExpandBuffer(position+1);
			_disabledTemporary = false;
			_state = SelectionState.Expansion;

			_forwardDestination._line = line.ID;
			_backwardDestination._line = line.ID;
			//Debug.WriteLine(String.Format("ExpandTo Line{0} Position{1}", line.ID, position));
			switch(type) {
				case RangeType.Char:
					_forwardDestination._position = position;
					_backwardDestination._position = position;
					break;
				case RangeType.Word:
					_forwardDestination._position = line.FindPrevWordBreak(position)+1;
					_backwardDestination._position = line.FindNextWordBreak(position);
					break;
				case RangeType.Line:
					_forwardDestination._position = 0;
					_backwardDestination._position = line.CharLength;
					break;
			}

			return true;
		}

		internal void SelectAll(TerminalPane owner) {
			_disabledTemporary = false;
			Debug.Assert(owner!=null);
			_owner = owner;
			_forwardPivot._line = _owner.Document.FirstLine.ID;
			_forwardPivot._position = 0;
			_backwardPivot = _forwardPivot;
			_forwardDestination._line = _owner.Document.LastLine.ID;
			_forwardDestination._position = _owner.Document.LastLine.CharLength-1;
			_backwardDestination = _forwardDestination;

			_state = SelectionState.Fixed;
			_pivotType = RangeType.Char;
		}

		//�y�C���O�փ}�E�X���h���b�O���Ă������ꍇ�Ɉʒu���C������
		internal void ConvertSelectionPosition(ref int line_id, ref int col) {
			if(_pivotType==RangeType.Line) {
				if(line_id<=_forwardPivot._line)
					col = 0;
				else 
					col = _owner.Connection.TerminalWidth;
			}
			else {
				if(line_id<_forwardPivot._line) {
					if(col<0)
						col = 0;
					else if(col>=_owner.Connection.TerminalWidth) {
						line_id++;
						col = 0;
					}
				}
				else if(line_id==_forwardPivot._line) {
					if(col<0)
						col = 0;
					else if(col>=_owner.Connection.TerminalWidth)
						col = _owner.Connection.TerminalWidth;
				}
				else {
					if(col<0) {
						line_id--;
						col = _owner.Connection.TerminalWidth;
					}
					else if(col>=_owner.Connection.TerminalWidth)
						col = _owner.Connection.TerminalWidth;
				}
			}
		}


		internal void FixSelection() {
			_state = SelectionState.Fixed;
		}

		public string GetSelectedText() {
			return GetSelectedText(NLOption.Default);
		}
		public string GetSelectedTextAsLook() {
			return GetSelectedText(NLOption.AsLook);
		}
		
		private enum NLOption {
			Default,
			AsLook
		}

		private string GetSelectedText(NLOption opt) {
			if(_owner==null || _disabledTemporary) return null;

			StringBuilder bld = new StringBuilder();
			TextPoint a = HeadPoint;
			TextPoint b = TailPoint;

			GLine l = _owner.Document.FindLineOrEdge(a._line);
			int pos = a._position;

			do {
				bool eol_required = (opt==NLOption.AsLook || l.EOLType!=EOLType.Continue);
				if(l.ID==b._line) { //�ŏI�s
					//������NULL����������P�[�X������悤��
					AppendTrim(bld, l.Text, pos, b._position-pos);
					if(_pivotType==RangeType.Line && eol_required)
						bld.Append("\r\n");
					break;
				}
				else { //�ŏI�ȊO�̍s
					if(l.CharLength-pos>0) { //l.CharLength==pos�ƂȂ�P�[�X���������B�^�̗��R�͔[�����Ă��Ȃ���
						AppendTrim(bld, l.Text, pos, l.CharLength-pos);
					}
					if(eol_required && bld.Length>0) //bld.Length>0�͍s�P�ʑI���ŗ]�v�ȉ��s������̂�����邽�߂̏��u
						bld.Append("\r\n"); //LF�݂̂��N���b�v�{�[�h�Ɏ����Ă����Ă����̃A�v���̍��������邾���Ȃ̂ł�߂Ă���
					l = l.NextLine;
					if(l==null) break; //!!�{������͂Ȃ��͂������N���b�V�����|�[�g�̂��߉��
					pos = 0;
				}
			} while(true);

			//Debug.WriteLine("Selected Text Len="+bld.Length);

			return bld.ToString();
		}
		private void AppendTrim(StringBuilder bld, char[] text, int pos, int length) {
			Debug.Assert(pos>=0);
			if(text[pos]==GLine.WIDECHAR_PAD) { //���{�ꕶ���̉E�[����̂Ƃ��͊g�傷��
				pos--;
				length++;
			}

			while(length-- > 0) {
				if(pos>=text.Length) break;
				char ch = text[pos++];
				if(ch!=GLine.WIDECHAR_PAD && ch!='\0') bld.Append(ch);
			}
		}

		internal TextPoint HeadPoint {
			get {
				return Min(ref _forwardPivot, ref _forwardDestination);
			}
		}
		internal TextPoint TailPoint {
			get {
				return Max(ref _backwardPivot, ref _backwardDestination);
			}
		}
		private static TextPoint Min(ref TextPoint p1, ref TextPoint p2) {
			int id1 = p1._line;
			int id2 = p2._line;
			if(id1==id2) {
				int pos1 = p1._position;
				int pos2 = p2._position;
				if(pos1==pos2)
					return p1;
				else
					return pos1<pos2? p1 : p2;
			}
			else
				return id1<id2? p1 : p2;
				
		}
		private static TextPoint Max(ref TextPoint p1, ref TextPoint p2) {
			int id1 = p1._line;
			int id2 = p2._line;
			if(id1==id2) {
				int pos1 = p1._position;
				int pos2 = p2._position;
				if(pos1==pos2)
					return p1;
				else
					return pos1>pos2? p1 : p2;
			}
			else
				return id1>id2? p1 : p2;
				
		}


	}
}

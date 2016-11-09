/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalPane.cs,v 1.3 2005/04/27 08:48:50 okajima Exp $
*/
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
//using System.Xml;

using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Forms;
using Poderosa.Text;
using Poderosa.Config;
using Poderosa.Communication;

using Granados.SSHC;

namespace Poderosa.Terminal {


	/// <summary>
	/// DocumentPane �̊T�v�̐����ł��B
	/// </summary>
	public class TerminalPane : UserControl, IPoderosaTerminalPane, IInternalTerminalPane {
		private const int BORDER = 2; //

		public System.Windows.Forms.VScrollBar _VScrollBar;

		private System.Windows.Forms.Timer _caretTimer;
		private System.Windows.Forms.Timer _sizeTipTimer;
		private ConnectionTag _tag;
		
		private int _caretState;
		private SelectionKeyProcessor _selectionKeyProcessor;
		private Label _sizeTip;
		private IntPtr _thisHWND;

		private bool _autoSelectionMode; //
		private bool _autoSelectionModeFromCommand; //

		private long _lastInvalidateTime;
		private bool _inIMEComposition; //
		private bool _fakeVisible; //�
		private bool _ignoreValueChangeEvent;
		private bool _criticalErrorRaised;


		public TerminalConnection Connection {
			get {
				return _tag==null? null : GetConnection();
			}
		}
		public ConnectionTag ConnectionTag {
			get {
				return _tag;
			}
		}
		internal TerminalDocument Document {
			get {
				return _tag==null? null : _tag.Document;
			}
		}
		public bool InFreeSelectionMode {
			get {
				return _selectionKeyProcessor!=null;
			}
		}
		public bool InAutoSelectionMode {
			get {
				return _autoSelectionMode;
			}
		}

		
		private System.ComponentModel.Container components = null;
    
		public TerminalPane() {
			// 
			InitializeComponent();

			// TODO: InitForm
			SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.DoubleBuffer, true);

			_caretTimer = new System.Windows.Forms.Timer();
			_caretTimer.Interval = Win32.GetCaretBlinkTime();
			_caretTimer.Tick += new EventHandler(this.OnCaretTimer);

			_sizeTipTimer = new System.Windows.Forms.Timer();
			_sizeTipTimer.Interval = 2000;
			_sizeTipTimer.Tick += new EventHandler(this.OnHideSizeTip);

			FakeVisible = false;

		}

		public void Attach(ConnectionTag tag) {
			_tag = tag;
			_tag.Pane = this;
			lock(_tag.Document) {

				_ignoreValueChangeEvent = true;
				_tag.Receiver.CommitScrollBar(_VScrollBar, false);
				_ignoreValueChangeEvent = false;

				if(!GetConnection().IsClosed) {
					Size ts = TerminalSize;
					if(ts.Width!=GetConnection().TerminalWidth || ts.Height!=GetConnection().TerminalHeight)
						ResizeTerminal(ts.Width, ts.Height);
				}

				if(_fakeVisible)
					this.BackColor = GetRenderProfile().BackColor;
			}

			if(!_caretTimer.Enabled) _caretTimer.Start();
            
			Invalidate(true);
		}
		public void Detach() {
			if(_inIMEComposition) ClearIMEComposition();
			if(InFreeSelectionMode) ExitFreeSelectionMode();
			if(InAutoSelectionMode) ExitAutoSelectionMode();

			if(_tag!=null) _tag.Pane = null;
			_tag = null;
			_caretTimer.Stop();
			_VScrollBar.Enabled = false;
		}
		public bool FakeVisible {
			get {
				return _fakeVisible;
			}
			set {
				_fakeVisible = value;
				_VScrollBar.Visible = value;
				this.Cursor = value? Cursors.IBeam : Cursors.Default;
				this.BackColor = value? GetRenderProfile().BackColor : Color.FromKnownColor(KnownColor.ControlDark);
			}
		}
		private TerminalConnection GetConnection() {
			return _tag.Connection;
		}
		//
		private RenderProfile GetRenderProfile() {
			if(_tag!=null && _tag.RenderProfile!=null)
				return _tag.RenderProfile;
			else
				return GEnv.DefaultRenderProfile;
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
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
		private void InitializeComponent() {
			this._VScrollBar = new System.Windows.Forms.VScrollBar();
			this._sizeTip = new Label();
			this.SuspendLayout();
			// 
			// _VScrollBar
			// 
			this._VScrollBar.Enabled = false;
			this._VScrollBar.Dock = DockStyle.Right;
			this._VScrollBar.LargeChange = 1;
			this._VScrollBar.Location = new System.Drawing.Point(512, 0);
			this._VScrollBar.Minimum = 0;
			this._VScrollBar.Value = 0;
			this._VScrollBar.Maximum = 2;
			this._VScrollBar.Name = "_VScrollBar";
			this._VScrollBar.Size = new System.Drawing.Size(16, 448);
			this._VScrollBar.TabIndex = 0;
			this._VScrollBar.TabStop = false;
			this._VScrollBar.Cursor = Cursors.Default;
			this._VScrollBar.Visible = false;
			this._VScrollBar.ValueChanged += new System.EventHandler(this._VScrollBar_ValueChanged);
			// 
			// _sizeTip
			// 
			this._sizeTip.Visible = false;
			this._sizeTip.BorderStyle = BorderStyle.FixedSingle;
			this._sizeTip.TextAlign = ContentAlignment.MiddleCenter;
			this._sizeTip.BackColor = Color.FromKnownColor(KnownColor.Info);
			this._sizeTip.ForeColor = Color.FromKnownColor(KnownColor.InfoText);
			this._sizeTip.Size = new Size(64, 16);
			// 
			// TerminalPane
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.TabStop = false;
			this.Name = "TerminalPane";
			this.AllowDrop = true;
			this.Size = new System.Drawing.Size(528, 448);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
			
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this._VScrollBar,this._sizeTip
			});
			this.Name = "TerminalPane";
			this.Size = new System.Drawing.Size(528, 448);
			this.ImeMode = ImeMode.NoControl;
			this.ResumeLayout(false);

		}
		#endregion

#if false
		/*
		 * ��  ��M�X���b�h�ɂ����s�̃G���A
		 */ 
		
		public void DataArrived() {
			// UI�X���b�h�Ńf�[�^��M���������s��
			//Win32.SendMessage(this.Handle, GConst.WMG_MAINTHREADTASK, new IntPtr(GConst.WPG_DATA_ARRIVED), IntPtr.Zero);
		}

		/*
		 * ��  ��M�X���b�h�ɂ����s�̃G���A
		 * -------------------------------
		 * ��  UI�X���b�h�ɂ����s�̃G���A
		 */ 

		private void InternalDataArrived() {
			if(_tag == null) return;	// �y�C������鎞�� _tag �� null �ɂȂ��Ă��邱�Ƃ�����
			TerminalDocument document = _tag.Document;
			if(_autoSelectionMode) {
				GLine t = document.CurrentLine.PrevLine;
				if(t!=null && t.ID>=GEnv.TextSelection.HeadPoint._line) {
					GEnv.TextSelection.ExpandTo(t, GetConnection().TerminalWidth-1, RangeType.Line); //���ݍs�̈�O�̍s�̏I�[�܂őI��
					//Debug.WriteLine(String.Format("ExpandTo {0}:{1}-{2}:{3}", GEnv.TextSelection.HeadPoint._line, GEnv.TextSelection.HeadPoint._position, GEnv.TextSelection.TailPoint._line, GEnv.TextSelection.TailPoint._position)); 
				}
				else
					GEnv.TextSelection.DisableTemporary(); //�o�͌��ʂ��P�s���Ȃ��Ƃ��͑I�����Ȃ��悤�ɂ���
			}

			//Debug.WriteLine(String.Format("v={0} l={1} m={2}", _VScrollBar.Value, _VScrollBar.LargeChange, _VScrollBar.Maximum));

			SmartInvalidate();

			//�����ϊ����ł������Ƃ��̂��߂̒���
			if(_inIMEComposition) {
				GEnv.InterThreadUIService.AdjustIMEComposition(this.Handle, document);
			}
		}
#else
		/*
		 * ��  ��M�X���b�h�ɂ����s�̃G���A
		 */ 
		
		public void DataArrived() {
			//
			InternalDataArrived();
		}

		private void InternalDataArrived() {
			if(_tag == null) return;	// �y�C������鎞�� _tag �� null �ɂȂ��Ă��邱�Ƃ�����
			TerminalDocument document = _tag.Document;
			if(_autoSelectionMode) {
				GLine t = document.CurrentLine.PrevLine;
				if(t!=null && t.ID>=GEnv.TextSelection.HeadPoint._line) {
					GEnv.TextSelection.ExpandTo(t, GetConnection().TerminalWidth-1, RangeType.Line); //���ݍs�̈�O�̍s�̏I�[�܂őI��
					//Debug.WriteLine(String.Format("ExpandTo {0}:{1}-{2}:{3}", GEnv.TextSelection.HeadPoint._line, GEnv.TextSelection.HeadPoint._position, GEnv.TextSelection.TailPoint._line, GEnv.TextSelection.TailPoint._position)); 
				}
				else
					GEnv.TextSelection.DisableTemporary(); //�o�͌��ʂ��P�s���Ȃ��Ƃ��͑I�����Ȃ��悤�ɂ���
			}

			//Debug.WriteLine(String.Format("v={0} l={1} m={2}", _VScrollBar.Value, _VScrollBar.LargeChange, _VScrollBar.Maximum));

			SmartInvalidate();

			//�����ϊ����ł������Ƃ��̂��߂̒���
			if(_inIMEComposition) {
				GEnv.InterThreadUIService.AdjustIMEComposition(_thisHWND, document);
			}
		}

#endif

		//�
		private void SmartInvalidate() {
			//�p�P�b�g���א؂�ł����Ƃ���Invalidate���b�V���ɂȂ�̂�h�~���邽�߁A�f�[�^�������āA���O�̃f�[�^����300ms�ȓ��ł����Invalidate���X�L�b�v
			if(MeetSmartInvalidateCondition()) { //�������ׂ��f�[�^���Ȃ��̂ł���Ζ�������Invalidate
				InvalidateBody();
			}
		}

		private bool MeetSmartInvalidateCondition() {
			return !GetConnection().Available || _lastInvalidateTime+1500000<DateTime.Now.Ticks;
		}


		private void InvalidateBody() {
			long tick = DateTime.Now.Ticks;
			_lastInvalidateTime = tick;
			if(_tag==null) return; //���ꂪ�����Ǝv����o�O�P��������
			TerminalDocument document = _tag.Document;
			bool full_invalidate = true;
			Rectangle r = new Rectangle();

			if(!document.InvalidatedAll) {
				full_invalidate = false;
				r.X = GEnv.SystemMetrics.ControlBorderWidth;
				r.Width = this.Width-GEnv.SystemMetrics.ControlBorderWidth*2;
				int y1 = document.InvalidatedFrom - document.TopLineNumber;
				int y2 = document.InvalidatedTo+1 - document.TopLineNumber;
				r.Y = GEnv.SystemMetrics.ControlBorderHeight + (int)(y1 * GetRenderProfile().Pitch.Height);
				r.Height = (int)((y2-y1) * GetRenderProfile().Pitch.Height)+1;
				
			}

			//Invalidate(r);
			if(this.InvokeRequired) {
				//Debug.WriteLine("Invoke Required");
				if(full_invalidate)
					_tag.InvalidateParam.Set(new InvalidateDelegate1(DelInvalidate), null);
				else
					_tag.InvalidateParam.Set(new InvalidateDelegate2(DelInvalidate), new object[1] { r });
			}
			else {
				if(full_invalidate)
					Invalidate();
				else
					Invalidate(r);
			}
		}

		/*
		 * ��  ��M�X���b�h�ɂ����s�̃G���A
		 * -------------------------------
		 * ��  UI�X���b�h�ɂ����s�̃G���A
		 */ 

		private delegate void InvalidateDelegate1();
		private delegate void InvalidateDelegate2(Rectangle rc);
		private void DelInvalidate(Rectangle rc) {
			Invalidate(rc);
		}
		private void DelInvalidate() {
			Invalidate();
		}

		protected override sealed void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			try {
				Rectangle clip = e.ClipRectangle;
				Graphics g = e.Graphics;
				if(CoversBorder(clip)) {
					ControlPaint.DrawBorder3D(g, 0, 0, Width, Height, Border3DStyle.Sunken);
				}

				if(!_fakeVisible) return;
				if(this.DesignMode || _tag==null) return;

				//
				/*if(!_thrownFlag) {
					_thrownFlag = true;
					throw new Exception("Test exception");
				}
				*/

				TerminalDocument document = _tag.Document;
				RenderProfile profile = GetRenderProfile();
				Image img = profile.GetImage();
				if(img!=null) DrawBackgroundImage(g, img, profile.ImageStyle, clip);

				int paneheight = GetConnection().TerminalHeight;

				//
				//
				RenderParameter param = new RenderParameter();
				bool  caret = this.Focused;
				int   caret_pos_x = 0, caret_pos_y = 0;
				ArrayList lines = new ArrayList(paneheight);
				lock(document) {
					CommitTransientScrollBar();
					BuildTransientDocument(e, lines, ref param, ref caret, ref caret_pos_x, ref caret_pos_y);
					document.ResetInvalidatedRegion();
				}

				//Rendering Core
				if(param.LineFrom <= document.LastLineNumber) {
					RenderProfile prof = GetRenderProfile();

					IntPtr hdc = g.GetHdc();
					IntPtr gdipgraphics = IntPtr.Zero;
					try {
						if(prof.UseClearType) {
							Win32.GdipCreateFromHDC(hdc, ref gdipgraphics);
							Win32.GdipSetTextRenderingHint(gdipgraphics, 5); //5��TextRenderingHintClearTypeGridFit
						}

						int t = param.LineFrom+param.LineCount;
						float y = prof.Pitch.Height * param.LineFrom;
						for(int i=param.LineFrom; i<t; i++) {
							if(i>=lines.Count) break;
							GLine line = (GLine)lines[i];
							line.Render(hdc, param, prof, (int)y);
							y += prof.Pitch.Height;
						}
					}
					finally {
						if(gdipgraphics!=IntPtr.Zero) Win32.GdipDeleteGraphics(gdipgraphics);
						g.ReleaseHdc(hdc);
					}

				}

				if(caret) {
					if((GEnv.Options.CaretType & CaretType.StyleMask)==CaretType.Line)
						DrawBarCaret(g, param, caret_pos_y, caret_pos_x);
					else if((GEnv.Options.CaretType & CaretType.StyleMask)==CaretType.Underline)
						DrawUnderLineCaret(g, param, caret_pos_y, caret_pos_x);
				}

			}
			catch(Exception ex) {
				if(!_criticalErrorRaised) { //���̒��ň�x��O����������ƌJ��Ԃ��N�����Ă��܂����Ƃ��܂܂���B�Ȃ̂ŏ���̂ݕ\�����ĂƂ肠�����؂蔲����
					_criticalErrorRaised = true;
					GUtil.ReportCriticalError(ex);
				}
			}
		}
		private void BuildTransientDocument(PaintEventArgs e, ArrayList lines, ref RenderParameter param, ref bool caret, ref int caret_pos_x, ref int caret_pos_y) {
			if(_tag == null) return;	// �y�C������鎞�� _tag �� null �ɂȂ��Ă��邱�Ƃ�����
			
			Rectangle clip = e.ClipRectangle;
			TerminalDocument document = _tag.Document;
			RenderProfile profile = GetRenderProfile();
			int paneheight = GetConnection().TerminalHeight;

			Win32.SystemMetrics sm = GEnv.SystemMetrics;
			param.TargetRect = new Rectangle(sm.ControlBorderWidth+1, sm.ControlBorderHeight,
				this.Width - _VScrollBar.Width - sm.ControlBorderWidth + 8, //���̂W���Ȃ��l�����������A.NET�̕����T�C�Y�ۂߖ��̂��ߍs�̍ŏI�������\������Ȃ����Ƃ�����B�����������邽�߂ɂ�����Ƒ��₷
				this.Height - sm.ControlBorderHeight);

			GLine l = document.TopLine;
			for(int i=0; i<paneheight; i++) {
				lines.Add(l.Clone());
				l = l.NextLine;
				if(l==null) break;
			}

			//�I��̈�̕`��
			TextSelection selection = GEnv.TextSelection;
			if(selection.Owner==this && !selection.IsEmpty) {
				TextSelection.TextPoint from = selection.HeadPoint;
				TextSelection.TextPoint to   = selection.TailPoint;
				l = document.FindLineOrNull(from._line);
				GLine t = document.FindLineOrNull(to._line);
				if(l!=null && t!=null) { //�{����l��null�ł͂����Ȃ��͂������A�������������o�O���|�[�g���������̂ŔO�̂���
					t = t.NextLine;
					int pos = from._position; //���Ƃ��΍��[���z���ăh���b�O�����Ƃ��̑I��͈͂͑O�s���ɂȂ�̂� pos==TerminalWidth�ƂȂ�P�[�X������B
					do {
						int index = l.ID-document.TopLineNumber;
						if(pos>=0 && pos<GetConnection().TerminalWidth && index>=0 && index<lines.Count) {
							GLine r = null; 
							if(l.ID==to._line) {
								if(pos!=to._position) r = ((GLine)lines[index]).InverseRange(pos, to._position);
							}
							else
								r = ((GLine)lines[index]).InverseRange(pos, l.CharLength);
							if(r!=null) {
								lines[l.ID-document.TopLineNumber] = r;
								//if(_selectionKeyProcessor!=null && _selectionKeyProcessor.CurrentLine==l) _selectionKeyProcessor.ReplaceCurrentLine(r);
								document.InvalidateLine(l.ID);
							}
						}
						pos = 0; //�Q�s�ڂ���̑I���͍s������
						l = l.NextLine;
					} while(l!=t);
				}
			}

			//�L�����b�g�ʒu�̌v�Z
			bool blink = (GEnv.Options.CaretType & CaretType.Blink)!=CaretType.None;
			GLine caret_line = null;
			if(_selectionKeyProcessor!=null) {
				caret_pos_x  = _selectionKeyProcessor.UICaretPos;
				caret_pos_y = _selectionKeyProcessor.CurrentLine.ID - document.TopLineNumber;
				if(caret_pos_y>=0 && caret_pos_y < GetConnection().TerminalHeight && caret_pos_y<lines.Count)
					caret_line = (GLine)lines[caret_pos_y];
				if(_caretState==1) caret = false; //�e�L�X�g�I�����[�h�ł͏�Ƀu�����N
			}
			else {
				caret_pos_x  = document.CaretColumn;
				caret_pos_y = document.CurrentLineNumber - document.TopLineNumber;
				if(caret_pos_y>=0 && caret_pos_y < GetConnection().TerminalHeight && caret_pos_y<lines.Count)
					caret_line = (GLine)lines[caret_pos_y];
				if(GetConnection().IsClosed) caret = false;
				if(blink && _caretState==1) caret = false;
			}

			int offset1 = (int)((clip.Top - GEnv.SystemMetrics.ControlBorderHeight) / profile.Pitch.Height);
			param.LineFrom  = offset1;
			int offset2 = (int)((clip.Bottom - GEnv.SystemMetrics.ControlBorderHeight) / profile.Pitch.Height);
			if(offset2 >= GetConnection().TerminalHeight) offset2 = GetConnection().TerminalHeight-1;

			param.LineCount = offset2 - offset1 + 1;
			//Debug.WriteLine(String.Format("{0} {1} {2}", param.LineFrom, param.LineCount, caret_pos_y));

			//Caret��ʊO�ɂ���Ȃ珈���͂��Ȃ��Ă悢�B�Q�Ԗڂ̏����́AAttach-ResizeTerminal�̗���̒��ł���OnPaint�����s�����ꍇ��TerminalHeight>lines.Count�ɂȂ�P�[�X������̂�h�~���邽��
			if(caret_line!=null) { 
				//�q�N�q�N���̂��߁A�L�����b�g��\�����Ȃ��Ƃ��ł����̑���͏Ȃ��Ȃ�
				if((GEnv.Options.CaretType & CaretType.Box)!=CaretType.None) {
					GLine inv = caret_line.InverseCaret(caret_pos_x, caret, false);
					Debug.Assert(caret_pos_y>=0);
					lines[caret_pos_y] = inv;
				}
			}
			else
				caret = false;
		}

		internal void CommitTransientScrollBar() {
			if(_tag != null) {	// TerminalPane
				_ignoreValueChangeEvent = true;
				_tag.Receiver.CommitScrollBar(_VScrollBar, true);	//
				_ignoreValueChangeEvent = false;
			}
		}

		private void DrawBarCaret(Graphics g, RenderParameter param, int y, int pos) {
			RenderProfile profile = GetRenderProfile();
			PointF pt1 = new PointF(GEnv.SystemMetrics.ControlBorderWidth + profile.Pitch.Width * pos,  GEnv.SystemMetrics.ControlBorderHeight + profile.Pitch.Height * y + 1);
			PointF pt2 = new PointF(pt1.X, pt1.Y + profile.Pitch.Height - 2);
			g.DrawLine(new Pen(GEnv.Options.CaretColor==Color.Empty? profile.ForeColor : GEnv.Options.CaretColor), pt1, pt2);
		}
		private void DrawUnderLineCaret(Graphics g, RenderParameter param, int y, int pos) {
			RenderProfile profile = GetRenderProfile();
			PointF pt1 = new PointF(GEnv.SystemMetrics.ControlBorderWidth + profile.Pitch.Width * pos,  GEnv.SystemMetrics.ControlBorderHeight + profile.Pitch.Height * (y+1) - 2);
			PointF pt2 = new PointF(pt1.X + profile.Pitch.Width, pt1.Y);
			g.DrawLine(new Pen(GEnv.Options.CaretColor==Color.Empty? profile.ForeColor : GEnv.Options.CaretColor), pt1, pt2);
		}
		protected void _VScrollBar_ValueChanged(object sender, System.EventArgs e) {
			if(_ignoreValueChangeEvent) return;
			TerminalDocument document = _tag.Document;
			lock(document) {
				//if(_tag.Terminal.TerminalMode==TerminalMode.Normal) ����if���̈Ӑ}���܂����s��
				document.TopLineNumber = document.FirstLineNumber + _VScrollBar.Value;
				_tag.Receiver.SetTransientScrollBarValue(_VScrollBar.Value);
				Invalidate();
			}
		}

		private bool CoversBorder(Rectangle rect) {
			int w = GEnv.SystemMetrics.ControlBorderWidth;
			int h = GEnv.SystemMetrics.ControlBorderHeight;
			return rect.Left<w || rect.Top<h || rect.Right>=Width-w || rect.Bottom>=Height-h;
		}

		private void DrawBackgroundImage(Graphics g, Image img, ImageStyle style, Rectangle clip) {
			int clip_left = clip.Left;
			int clip_top = clip.Top;
			int clip_right = clip.Right;
			int clip_bottom= clip.Bottom;
			if(clip_left < BORDER) clip_left = BORDER;
			if(clip_top  < BORDER) clip_top = BORDER;
			if(clip_right >=this.Width-BORDER)  clip_right  = this.Width-BORDER;
			if(clip_bottom>=this.Height-BORDER) clip_bottom = this.Height-BORDER;
			Rectangle clip2 = new Rectangle(clip_left, clip_top, clip_right-clip_left, clip_bottom-clip_top);

			if(style==ImageStyle.Scaled)
				DrawBackgroundImage_Scaled(g, img, clip2);
			else
				DrawBackgroundImage_Normal(g, img, style, clip2);
		}
		private void DrawBackgroundImage_Scaled(Graphics g, Image img, Rectangle clip) {
			float scale;
			int offset_x, offset_y;
			float sw = (float)(this.Width-_VScrollBar.Width) / img.Width;
			float sh = (float)this.Height / img.Height;
			if(sw < sh) {
				scale = sw;
				offset_x = BORDER;
				offset_y = (int)(this.Height/2 - img.Height*scale/2);
			}
			else {
				scale = sh;
				offset_x = (int)((this.Width-_VScrollBar.Width)/2 - img.Width*scale/2);
				offset_y = BORDER;
			}

			RectangleF target = RectangleF.Intersect(new RectangleF((clip.Left-offset_x)/scale, (clip.Top-offset_y)/scale, clip.Width/scale, clip.Height/scale), new RectangleF(0,0,img.Width,img.Height));
			if(target!=RectangleF.Empty)
				g.DrawImage(img, new RectangleF(target.Left*scale + offset_x, target.Top*scale + offset_y, target.Width*scale, target.Height*scale), target, GraphicsUnit.Pixel);
		}

		private void DrawBackgroundImage_Normal(Graphics g, Image img, ImageStyle style, Rectangle clip) {
			int offset_x, offset_y;
			if(style==ImageStyle.Center) {
				offset_x = (this.Width-_VScrollBar.Width - img.Width) / 2;
				offset_y = (this.Height - img.Height) / 2;
			}
			else {
				offset_x = (style==ImageStyle.TopLeft || style==ImageStyle.BottomLeft)? BORDER : this.Width - _VScrollBar.Width - img.Width;
				offset_y = (style==ImageStyle.TopLeft || style==ImageStyle.TopRight)? BORDER : this.Height - img.Height - BORDER;
			}
			//if(offset_x < BORDER) offset_x = BORDER;
			//if(offset_y < BORDER) offset_y = BORDER;

			//�摜���̃R�s�[�J�n���W
			Rectangle target = Rectangle.Intersect(new Rectangle(clip.Left-offset_x, clip.Top-offset_y, clip.Width, clip.Height), new Rectangle(0,0,img.Width,img.Height));
			if(target!=Rectangle.Empty)
				g.DrawImage(img, new Rectangle(target.Left + offset_x, target.Top + offset_y, target.Width, target.Height), target, GraphicsUnit.Pixel);
		}

		protected override bool IsInputKey(Keys key) {
			Keys mod = key & Keys.Modifiers;
			Keys body = key & Keys.KeyCode;
			if(mod==Keys.None && (body==Keys.Tab || body==Keys.Escape))
				return true;
			else
				return false;
		}

		protected override bool ProcessDialogKey(Keys key) {
			//�}�N�����s���͋����I�ɊO���ɓn��
			if(GEnv.Frame.MacroIsRunning) {
				GEnv.Frame.ProcessShortcutKey(key);
				return true;
			}

			//Debug.WriteLine(string.Format("Pane ProcessDialogKey {0}", key & Keys.KeyCode));
			//�J�[�\���L�[��ContainerControl#ProcessDialogKey�ɓn���ƃt�H�[�J�X���ړ������Ⴄ
			Keys modifiers = key & Keys.Modifiers;
			Keys keybody = key & Keys.KeyCode;
			bool cursor = GUtil.IsCursorKey(keybody);
			
			//�e�L�X�g�I�����ǂ����Ɋ֌W�Ȃ������L�[
			if(key==Keys.Apps) { //�R���e�L�X�g���j���[
				TerminalDocument document = _tag.Document;
				int x = document.CaretColumn;
				int y = document.CurrentLineNumber - document.TopLineNumber;
				SizeF p = GetRenderProfile().Pitch;
				ShowContextMenu(new Point((int)(p.Width * x), (int)(p.Height * y)));
				return true;
			}

			if(_selectionKeyProcessor!=null)
				return ProcessDialogKeyInTextSelectionMode(key, keybody, modifiers, cursor);
			else
				return ProcessDialogKeyInNormalMode(key, keybody, modifiers, cursor);
		}

		private bool ProcessDialogKeyInTextSelectionMode(Keys key, Keys keybody, Keys modifiers, bool cursor) {
			bool processed = false;
			if(!cursor) {
				processed = base.ProcessDialogKey(key);
				if(processed)
					return true;
			}

			processed = _selectionKeyProcessor.ProcessKey(key);
			if(processed) {
				ResetCaretBlink();
				_caretState = 0;
				TerminalDocument document = _tag.Document;
				int id = _selectionKeyProcessor==null? document.CurrentLineNumber : _selectionKeyProcessor.CurrentLine.ID;
				if(_VScrollBar.Enabled) {
					if(document.TopLineNumber>id)
						_VScrollBar.Value = id - document.FirstLineNumber;
					else if(document.TopLineNumber+GetConnection().TerminalHeight-1<id)
						_VScrollBar.Value = id+1-GetConnection().TerminalHeight - document.FirstLineNumber;
				}

				Invalidate();
			}
			else
				ExitFreeSelectionMode();

			return processed;
		}

		private bool ProcessDialogKeyInNormalMode(Keys key, Keys keybody, Keys modifiers, bool cursor) {
			if((modifiers & Keys.Alt)!=Keys.None && _fakeVisible) {
				//Alt�����Ă����ꍇ
				if(System.Environment.OSVersion.Platform==PlatformID.Win32NT) {
					if(GEnv.Options.LeftAltKey!=AltKeyAction.Menu && (Win32.GetKeyState(Win32.VK_LMENU) & 0x8000)!=0) {
						ProcessSpecialAltKey(GEnv.Options.LeftAltKey, modifiers, keybody);
						return true;
					}
					else if(GEnv.Options.RightAltKey!=AltKeyAction.Menu && (Win32.GetKeyState(Win32.VK_RMENU)& 0x8000)!=0) {
						ProcessSpecialAltKey(GEnv.Options.RightAltKey, modifiers, keybody);
						return true;
					}
				}
				else { //
					if(GEnv.Options.RightAltKey!=AltKeyAction.Menu) {
						ProcessSpecialAltKey(GEnv.Options.RightAltKey, modifiers, keybody);
						return true;
					}
				}
				return base.ProcessDialogKey(key);
			}
			else if(IsSequenceKey(keybody)) { //
				CommandResult res = GEnv.Frame.ProcessShortcutKey(key);
				if(res!=CommandResult.NOP) return true;

				if(IsScrollKey(keybody) && (modifiers!=Keys.None && GEnv.Options.LocalBufferScrollModifier==modifiers)) {
					if(_VScrollBar.Enabled) ProcessScrollKey(keybody); //
				}
				else
					ProcessSequenceKey(modifiers, keybody);
				return true;
			}
			else
				return base.ProcessDialogKey(key);
		}

		private static bool IsSequenceKey(Keys key) {
			Keys body = key & Keys.KeyCode;
			return ((int)Keys.F1 <= (int)body && (int)body <= (int)Keys.F12) ||
				body==Keys.Insert || body==Keys.Delete || IsScrollKey(key);
		}
		private static bool IsScrollKey(Keys key) {
			return key==Keys.Up || key==Keys.Down ||
				key==Keys.Left || key==Keys.Right ||
				key==Keys.PageUp || key==Keys.PageDown ||
				key==Keys.Home || key==Keys.End;
		}
		private void ProcessSpecialAltKey(AltKeyAction act, Keys modifiers, Keys body) {
			if(!_fakeVisible) return;
			char ch = KeyboardInfo.Scan(body, (modifiers & Keys.Shift)!=Keys.None);
			if(ch=='\0') return; //���蓖�Ă��Ă��Ȃ���͖���

			if((modifiers & Keys.Control)!=Keys.None)
				ch = (char)((int)ch % 32); //Control���������琧�䕶��

			if(act==AltKeyAction.ESC) {
				byte[] t = new byte[2];
				t[0] = 0x1B;
				t[1] = (byte)ch;
				SendBytes(t);
			}
			else { //Meta
				ch = (char)(0x80 + ch);
				byte[] t = new byte[1];
				t[0] = (byte)ch;
				SendBytes(t);
			}
		}
        protected override void OnKeyDown(KeyEventArgs e)
        {
            //Console.WriteLine(e.KeyData.ToString());
            //if (e.KeyCode == Keys.Insert)
            //    return;
            base.OnKeyDown(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            //Console.WriteLine(e.KeyChar);
            base.OnKeyPress(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {

            //Console.WriteLine(e.KeyCode.ToString());

            #region all this gay shit is just to allow "Shift + Insert" to paste from clipboard
            if (e.KeyCode == Keys.Insert && e.Shift == true)
            {
                //Console.WriteLine("Shift" + e.KeyCode.ToString());
                string value = (string)Clipboard.GetDataObject().GetData("Text");
                if (value == null || value.Length == 0 || this.ConnectionTag == null) return;

                PasteProcessor p = new PasteProcessor(this.ConnectionTag, value);
                p.Perform();
            }
            #endregion

            base.OnKeyUp(e);
        }
		private void OnKeyPress(object sender, KeyPressEventArgs e) {
            //Console.WriteLine(e.KeyChar + "|");
            if(!IsAcceptableUserInput()) return;

			char ch = e.KeyChar;
			char[] chars;
			if(ch=='\r') {
				chars = TerminalUtil.NewLineChars(GetConnection().Param.TransmitNL);
				if(Control.ModifierKeys==Keys.Shift) {
					EnterAutoSelectionMode(false);
				}
			}
			else {
				if(ch==' ' && Control.ModifierKeys==Keys.Control) ch = '\0'; //Ctrl+Space��NUL���M
				//Debug.WriteLine((int)ch);
				chars = new char[1] { ch };
			}

			SendBytes(GetConnection().Param.EncodingProfile.GetBytes(chars));
		}
		private void SendBytes(byte[] data) {
			if(!IsAcceptableUserInput()) return;

			TerminalDocument doc = _tag.Document;
			lock(doc) {
				//
				if(GEnv.TextSelection.Owner==this) {
					if(!_autoSelectionMode || (!_autoSelectionModeFromCommand && (Control.ModifierKeys & Keys.Shift)==Keys.None)) {
						ExitAutoSelectionMode();
						GEnv.TextSelection.Clear();
						Invalidate();
					}
				}
				GEnv.Connections.KeepAlive.SetTimerToConnectionTag(_tag);

				//
				ResetCaretBlink();

				MakeCurrentLineVisible();

				//GEnv.Frame.StatusBar.IndicateSendData();
				if(GetConnection().Param.LocalEcho) {
					_tag.Terminal.Input(data, 0, data.Length);
					doc.InvalidateLine(doc.CurrentLineNumber);
					InvalidateBody();
				}
			}
			Write(data);
		}
		private bool IsAcceptableUserInput() {
			if(!_fakeVisible || GetConnection().IsClosed || GEnv.Frame.MacroIsRunning || _tag.ModalTerminalTask!=null)
				return false;
			else
				return true;

		}

		private void ProcessScrollKey(Keys key) {
			int current = _tag.Document.TopLineNumber - _tag.Document.FirstLineNumber;
			int newvalue = 0;
			switch(key) {
				case Keys.Up:
					newvalue = current-1;
					break;
				case Keys.Down:
					newvalue = current+1;
					break;
				case Keys.PageUp:
					newvalue = current-_tag.Connection.TerminalHeight;
					break;
				case Keys.PageDown:
					newvalue = current+_tag.Connection.TerminalHeight;
					break;
				case Keys.Home:
					newvalue = 0;
					break;
				case Keys.End:
					newvalue = _tag.Document.LastLineNumber - _tag.Document.FirstLineNumber + 1 - _tag.Connection.TerminalHeight;
					break;
			}
			
			if(newvalue < 0) newvalue = 0;
			else if(newvalue > _VScrollBar.Maximum+1-_VScrollBar.LargeChange) newvalue = _VScrollBar.Maximum+1-_VScrollBar.LargeChange;

			_VScrollBar.Value = newvalue; //
		}

		private void ProcessSequenceKey(Keys modifier, Keys body) {
			if(!_fakeVisible) return;
			//
			if(GetConnection().IsClosed) return;

			ResetCaretBlink();
			byte[] data;
			if(body==Keys.Delete && GEnv.Options.Send0x7FByDel) {
				data = new byte[1];
				data[0] = 0x7F;
			}
			else
				data = _tag.Terminal.SequenceKeyData(modifier, body);
			Write(data);
		}
		private void Write(byte[] data) {
            
            if (data.Length == 4)
            {
                // this is the insert key.... this is fucking stupid i have to do this
                byte[] b = { 0x1B, 0x5B, 0x32, 0x7E };

                if (data[0] == b[0] && data[1] == b[1] && data[2] == b[2] && data[3] == b[3])
                    return;

                #region what i think of some of this poderosa shit
                /*
                 *  ######  #    #   ####   #    #     #    #    #   ####
                 *  #       #    #  #    #  #   #      #    ##   #  #    #
                 *  #####   #    #  #       ####       #    # #  #  #
                 *  #       #    #  #       #  #       #    #  # #  #  ###
                 *  #       #    #  #    #  #   #      #    #   ##  #    #
                 *  #        ####    ####   #    #     #    #    #   ####


  ####    #####  #    #  #####      #    #####
 #          #    #    #  #    #     #    #    #
  ####      #    #    #  #    #     #    #    #
      #     #    #    #  #####      #    #    #
 #    #     #    #    #  #          #    #    #
  ####      #     ####   #          #    #####
                 * 
                 */
                #endregion
            }

            //if (data == {27; 91; 50; 126})
            //{}



			try {
				GetConnection().Write(data);
			}
			catch(Exception) {
				GUtil.Warning(GEnv.Frame, GEnv.Strings.GetString("Message.TerminalPane.FailedToSend"));
				try {
					GetConnection().Close();
					GEnv.Frame.RefreshConnection(GEnv.Connections.FindTag(GetConnection()));
				}
				catch(Exception ) { //
					//GUtil.ReportCriticalError(ex2);
				}
			}
		}


		public void MakeCurrentLineVisible() {
			//Debug.WriteLine(string.Format("MCV S={0},CL={1},H={2}", _VScrollBar.Value, document.CurrentLineNumber, GetConnection().Param.Height));

			TerminalDocument document = _tag.Document;
			if(document.CurrentLineNumber-document.FirstLineNumber < _VScrollBar.Value) { //��ɉB�ꂽ
				document.TopLineNumber = document.CurrentLineNumber;
				_tag.Receiver.SetTransientScrollBarValue(document.TopLineNumber-document.FirstLineNumber);
			}
			else if(_VScrollBar.Value + GetConnection().TerminalHeight <= document.CurrentLineNumber-document.FirstLineNumber) { //���ɉB�ꂽ
				int n = document.CurrentLineNumber-document.FirstLineNumber - GetConnection().TerminalHeight + 1;
				if(n < 0) n = 0;
				_tag.Receiver.SetTransientScrollBarValue(n);
				document.TopLineNumber = n + document.FirstLineNumber;
			}
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad (e);
			_thisHWND = this.Handle;
		}


		protected override void OnResize(EventArgs args) {
			base.OnResize(args);
			Invalidate();
			//�ŏ������ɂ͂Ȃ������g�̕��������O�ɂȂ��Ă��܂�
			if(this.DesignMode || this.ParentForm==null || this.ParentForm.WindowState==FormWindowState.Minimized) return;

			Size ts = TerminalSize;

			if(_tag!=null && !GetConnection().IsClosed && (ts.Width!=GetConnection().TerminalWidth || ts.Height!=GetConnection().TerminalHeight)) {
				ResizeTerminal(ts.Width, ts.Height);
				CommitTransientScrollBar();
			}

			ShowSizeTip(ts.Width, ts.Height);
			if(!_caretTimer.Enabled) _caretTimer.Start();
		}
		private void OnHideSizeTip(object sender, EventArgs args) {
			_sizeTip.Visible = false;
			_sizeTipTimer.Stop();
		}

		public Size TerminalSize {
			get {
				//�V�����s�ƌ����v�Z
				return CalcTerminalSize(GetRenderProfile().Pitch);
			}
		}
		private Size CalcTerminalSize(SizeF charPitch) {
			Win32.SystemMetrics sm = GEnv.SystemMetrics;
			int width  = (int)Math.Floor(((float)this.Width - sm.ScrollBarWidth - sm.ControlBorderWidth*2) / charPitch.Width);
			int height = (int)Math.Floor((float)(this.Height - sm.ControlBorderHeight*2) / charPitch.Height);
			if(width <= 0) width = 1; //�ɒ[�ȃ��T�C�Y������ƕ��̒l�ɂȂ邱�Ƃ�����
			if(height <= 0) height = 1;
			return new Size(width, height);
		}

		private void ShowSizeTip(int width, int height) {
			const int MARGIN = 8;
			Form form = GEnv.Frame.AsForm();
			if(form==null || !form.Visible) return; //�N�����ɂ͕\�����Ȃ�

			Point pt = new Point(this.Width-_VScrollBar.Width-_sizeTip.Width-MARGIN, this.Height-_sizeTip.Height-MARGIN);

			_sizeTip.Text = String.Format("{0} * {1}", width, height);
			_sizeTip.Location = pt;
			_sizeTip.Visible = true;

			_sizeTipTimer.Stop();
			_sizeTipTimer.Start();
		}
		//
		public void SplitterDragging(int width, int height) {
			_caretTimer.Stop();
			SizeF charSize = GetRenderProfile().Pitch;
			Win32.SystemMetrics sm = GEnv.SystemMetrics;
			width  = (int)Math.Floor(((float)width - sm.ScrollBarWidth - sm.ControlBorderWidth*2) / charSize.Width);
			height = (int)Math.Floor((float)(height - sm.ControlBorderHeight*2) / charSize.Height);
			ShowSizeTip(width, height);
		}

		public void ResizeTerminal(int width, int height) {
			//Debug.WriteLine(String.Format("Resize {0} {1}", width, height));
			if(_tag.ModalTerminalTask!=null) return; //
			if(_tag.Terminal.TerminalMode==TerminalMode.Application) //
				_tag.Document.SetScrollingRegion(0, height-1);
			_tag.Terminal.Reset();
			if(_VScrollBar.Enabled) 
            {
				bool scroll = IsAutoScrollMode();
				_VScrollBar.LargeChange = height;
				if(scroll)
					MakeCurrentLineVisible();
			}
            
			try {
                //TerminalConnection con = _tag.Connection;
				GEnv.GetConnectionCommandTarget(_tag.Connection).Resize(width, height);
			}
			catch(Exception ex) {
				GUtil.WriteDebugLog("resize error\n"+ex.StackTrace);
                
			}
			SmartInvalidate();
		}
		//���ݍs��������悤�Ɏ����I�ɒǐ����Ă����ׂ����ǂ����̔���
		private bool IsAutoScrollMode() {
			return _tag.Terminal.TerminalMode==TerminalMode.Normal && 
				_tag.Document.CurrentLineNumber>=_tag.Document.TopLineNumber+GetConnection().TerminalHeight-1 &&
				(!_VScrollBar.Enabled || _VScrollBar.Value+_VScrollBar.LargeChange>_VScrollBar.Maximum);
		}


		public void ToggleFreeSelectionMode() {
			if(_tag==null) return;
			if(InAutoSelectionMode) ExitAutoSelectionMode();

			if(_selectionKeyProcessor==null) {
				EnterFreeSelectionMode();
				TerminalDocument document = _tag.Document;
				_selectionKeyProcessor = new SelectionKeyProcessor(this, document, document.CurrentLine, document.CaretColumn);
				_caretTimer.Interval = Win32.GetCaretBlinkTime()/2;
				//GEnv.Frame.StatusBar.IndicateSelectionMode();
			}
			else {
				ExitFreeSelectionMode();
				_selectionKeyProcessor = null;
				GEnv.Frame.SetSelectionStatus(0);
				GEnv.TextSelection.Clear();
				_caretTimer.Interval = Win32.GetCaretBlinkTime();
			}

			if(_inIMEComposition) {
				ClearIMEComposition();
			}
		}
		public void ToggleAutoSelectionMode() {
			if(_tag==null) return;
			if(InFreeSelectionMode) ExitFreeSelectionMode();

			if(!_autoSelectionMode)
				EnterAutoSelectionMode(true);
			else
				ExitAutoSelectionMode();

			if(_inIMEComposition) {
				ClearIMEComposition();
			}
		}

		private void ResetCaretBlink() {
			_caretTimer.Stop();
			_caretState = 0;
			_caretTimer.Start();
		}


		//IME�̈ʒu���킹�Ȃ�
		private void AdjustIMEComposition(int charwidth) {
			TerminalDocument document = _tag.Document;
			IntPtr hIMC = Win32.ImmGetContext(this.Handle);
			RenderProfile prof = GetRenderProfile();

			//�t�H���g�̃Z�b�g�͂P����΂悢�̂��H
			Win32.LOGFONT lf = new Win32.LOGFONT();
			prof.CalcFont(null,CharGroup.TwoBytes).ToLogFont(lf);
			Win32.ImmSetCompositionFont(hIMC, lf);

			Win32.COMPOSITIONFORM form = new Win32.COMPOSITIONFORM();
			form.dwStyle = Win32.CFS_POINT;
			Win32.SystemMetrics sm = GEnv.SystemMetrics;
			//Debug.WriteLine(String.Format("{0} {1} {2}", document.CaretColumn, charwidth, document.CurrentLine.CharPosToDisplayPos(document.CaretColumn)));
			form.ptCurrentPos.x = sm.ControlBorderWidth  + (int)(prof.Pitch.Width * (document.CaretColumn + charwidth));
			form.ptCurrentPos.y = sm.ControlBorderHeight + (int)(prof.Pitch.Height * (document.CurrentLineNumber - document.TopLineNumber));
			bool r = Win32.ImmSetCompositionWindow(hIMC, ref form);
			Debug.Assert(r);
			Win32.ImmReleaseContext(this.Handle, hIMC);
		}
		private void ClearIMEComposition() {
			IntPtr hIMC = Win32.ImmGetContext(this.Handle);
			Win32.ImmNotifyIME(hIMC, Win32.NI_COMPOSITIONSTR, Win32.CPS_CANCEL, 0);
			Win32.ImmReleaseContext(this.Handle, hIMC);
			_inIMEComposition = false;
		}

		private void OnCaretTimer(object sender, EventArgs args) {
			if(_tag==null || _inIMEComposition) return; //���̃y�C���ł̐ڑ����Ȃ����AIME�N�����ł���Ζ���

			if(++_caretState==2) _caretState = 0;

			bool blink = (GEnv.Options.CaretType & CaretType.Blink)!=CaretType.None;
			TerminalDocument document = _tag.Document;
			lock(document) {
				if(_selectionKeyProcessor!=null)
					document.InvalidateLine(_selectionKeyProcessor.CurrentLine.ID);
				else if(blink)
					document.InvalidateLine(document.CurrentLineNumber);

				SmartInvalidate();
			}
			//if(this.Focused) Invalidate(); //�_�ł�Invalidate����̂̓t�H�[�J�X������Ƃ������ł悢�͂������AFocused���������Ȓl��Ԃ��Ă���炵���@�\���Ȃ����Ƃ�����
		}
		//SP1 issue�ň�x�����������^�C�}�[�͎��͕K�v�Ȃ��̂Ŕp�~
		protected void PostOnTimer() {
		}

		private void ShowContextMenu(Point pt) {
			pt = GEnv.Frame.AsForm().PointToClient(this.PointToScreen(pt));
			GEnv.Frame.ShowContextMenu(pt, _tag);
		}
		
		public Control AsControl() {
			return this;
		}
		public void ApplyOptions(CommonOptions opt) {
			if(_tag!=null && _tag.RenderProfile!=null) return; //�ŗL��Profile������Ȃ�I�v�V�����ɂ͔������Ȃ�
			ApplyRenderProfile(new RenderProfile(opt));
		}
		public void ApplyRenderProfile(RenderProfile prof) {
			if(_fakeVisible) {
				this.BackColor = prof.BackColor;
				SizeF charPitch = prof.Pitch;
				Size ts = CalcTerminalSize(charPitch);
				if(!GetConnection().IsClosed && (ts.Width!=GetConnection().TerminalWidth || ts.Height!=GetConnection().TerminalHeight)) {
					ResizeTerminal(ts.Width, ts.Height);
				}
				Invalidate();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e) {
			if(_tag!=null && !GEnv.Options.AllowsScrollInAppMode && _tag.Terminal.TerminalMode==TerminalMode.Application && _selectionKeyProcessor==null) { //�A�v���P�[�V�������[�h�ł͒ʏ폈������߂ăJ�[�\���㉺�Ɠ����̏����ɂ���
				int m = GEnv.Options.WheelAmount;
				for(int i=0; i<m; i++) 
					ProcessSequenceKey(Keys.None, e.Delta>0? Keys.Up : Keys.Down);
			}
			else {
				base.OnMouseWheel(e);
				if(!_VScrollBar.Enabled) return;

				int d = e.Delta / 120; //�J��������Delta��120�B�����1��-1������͂�
				d *= GEnv.Options.WheelAmount;

				int newval = _VScrollBar.Value - d; 
				if(newval<0) newval=0;
				if(newval>_VScrollBar.Maximum-_VScrollBar.LargeChange) newval=_VScrollBar.Maximum-_VScrollBar.LargeChange+1;
				_VScrollBar.Value = newval;
			}
		}

		/*
		 *�e�L�X�g�̑I���֌W 
		 */
		protected override void OnMouseDown(MouseEventArgs args) {
			base.OnMouseDown(args);
			if(args.Button!=MouseButtons.Left) return;
			if(!_fakeVisible) return;
			CommitTransientScrollBar();
            if (_tag == null) return;
			TerminalDocument document = _tag.Document;
			lock(document) {
				SizeF pitch = GetRenderProfile().Pitch;
				int row = (int)Math.Floor(args.Y / pitch.Height);
				int col = (int)Math.Floor(args.X / pitch.Width);;
				int target_id = document.TopLine.ID+row;
			
				TextSelection sel = GEnv.TextSelection;
				if(sel.Owner!=this || sel.State==SelectionState.Fixed) sel.Clear(); //�ςȂƂ����MouseDown�����Ƃ��Ă�Clear�����͂���
				if(target_id <= document.LastLineNumber) {
					if(InFreeSelectionMode) ExitFreeSelectionMode();
					if(InAutoSelectionMode) ExitAutoSelectionMode();
					RangeType rt;
					//Debug.WriteLine(String.Format("MouseDown {0} {1}", sel.State, sel.PivotType));
					if(sel.State==SelectionState.Empty || sel.StartX!=args.X || sel.StartY!=args.Y)
						rt = RangeType.Char;
					else
						rt = sel.PivotType==RangeType.Char? RangeType.Word : sel.PivotType==RangeType.Word? RangeType.Line : RangeType.Char;

					//�}�E�X�𓮂����Ă��Ȃ��Ă��AMouseDown�ƂƂ���MouseMove�����Ă��܂��悤��
					GLine tl = document.FindLine(target_id);
					sel.StartSelection(this, tl, col, rt, args.X, args.Y);
				}
			}
			Invalidate();
		}
		protected override void OnMouseMove(MouseEventArgs args) {
			base.OnMouseMove(args);
			if(!_fakeVisible) return;

			TextSelection sel = GEnv.TextSelection;
			if(sel.Owner!=this || sel.State==SelectionState.Fixed || args.Button!=MouseButtons.Left) return;
			//�N���b�N�����ł��Ȃ���MouseDown�̒����MouseMove�C�x���g������̂ł��̂悤�ɂ��ăK�[�h
			if(sel.StartX==args.X && sel.StartY==args.Y) return;

			TerminalDocument document = _tag.Document;
			lock(document) {
				SizeF pitch = GetRenderProfile().Pitch;
				int row = (int)Math.Floor(args.Y / pitch.Height);
				int col = (int)Math.Floor(args.X / pitch.Width);
				int target_id = document.TopLineNumber+row;

				sel.ConvertSelectionPosition(ref target_id, ref col);

				if(target_id > document.LastLineNumber)
					target_id = document.LastLineNumber;
				else if(target_id < document.FirstLineNumber)
					target_id = document.FirstLineNumber;
			
				if(_tag.Terminal.TerminalMode==TerminalMode.Normal) {
					if(target_id < document.TopLineNumber)
						_VScrollBar.Value = target_id - document.FirstLineNumber;
					else if(target_id >= document.TopLineNumber+GetConnection().TerminalHeight) {
						int newval = target_id - document.FirstLineNumber - GetConnection().TerminalHeight + 1;
						if(newval<0) newval = 0;
						if(newval>_VScrollBar.Maximum-_VScrollBar.LargeChange) newval = _VScrollBar.Maximum-_VScrollBar.LargeChange+1;
						_VScrollBar.Value = newval;
					}
				}
				else {
					if(target_id < document.TopLineNumber)
						target_id = document.TopLineNumber;
					else if(target_id >= document.TopLineNumber+GetConnection().TerminalHeight)
						target_id = document.TopLineNumber+GetConnection().TerminalHeight-1;
				}

				//Debug.WriteLine(String.Format("MouseMove {0} {1} {2}", sel.State, sel.PivotType, args.X));
				RangeType rt = sel.PivotType;
				if((Control.ModifierKeys & Keys.Control)!=Keys.None)
					rt = RangeType.Word;
				else if((Control.ModifierKeys & Keys.Shift)!=Keys.None)
					rt = RangeType.Line;

				GLine tl = document.FindLine(target_id);
				sel.ExpandTo(tl, col, rt);
			}
			Invalidate();
		}


		protected override void OnMouseUp(MouseEventArgs args) {
			base.OnMouseUp(args);
			if(!_fakeVisible) return;

			if(args.Button==MouseButtons.Left) {
				TextSelection sel = GEnv.TextSelection;
				//Debug.WriteLine(String.Format("MouseUp {0} {1}", sel.State, sel.PivotType));
				if(sel.Owner==this && sel.State==SelectionState.Expansion)
					sel.FixSelection();
				if(GEnv.Options.AutoCopyByLeftButton)
					GEnv.GlobalCommandTarget.Copy();
			}
			else if(args.Button==MouseButtons.Right) {
				if(GEnv.Options.RightButtonAction==RightButtonAction.ContextMenu)
					ShowContextMenu(new Point(args.X, args.Y));
				else
					GEnv.GetConnectionCommandTarget(GetConnection()).Paste();
			}
		}

#if true
		protected override void OnGotFocus(EventArgs args) {
			base.OnGotFocus(args);
            /* Taking this out b/c Poderosa keeps fucking with my terminals
			if(!_fakeVisible) return;

			//Debug.WriteLine("OnGotFocus");
			if(_tag!=null) { //�������ߒ��̂Ƃ��͖���
				//.NET1.1 SP1 issue
				GEnv.Frame.ActivateConnection(_tag);
				if(!_caretTimer.Enabled) _caretTimer.Start(); //�X�v���b�^�̃h���b�O���L�����Z��������̓��T�C�Y������Ȃ��̂ł��̃^�C�~���O�ŃL�����b�g���ĉғ�����

				if(_selectionKeyProcessor != null) GEnv.Frame.SetSelectionStatus(SelectionStatus.Free);
				else if(_autoSelectionMode) GEnv.Frame.SetSelectionStatus(SelectionStatus.Auto);
			}
             */
		}
		protected void PostOnGotFocus() {
		}
#else
		private bool _recursiveFocusEventBlockFlag;
		protected override void OnGotFocus(EventArgs args) {
			base.OnGotFocus(args);
			if(!_fakeVisible) return;

			//Debug.WriteLine("OnGotFocus");
			if(_tag!=null && !_recursiveFocusEventBlockFlag) { //�������ߒ��̂Ƃ��͖���
				//OnGotFocus���Ŗ{���̏����������Framework���Ƃ̃C���^���N�V�����ł܂������Ƃ��N����炵�����Ƃ����������̂�PostMessage�ŏ������O���ɏo��
				_recursiveFocusEventBlockFlag = true;
				Win32.PostMessage(this.Handle, GConst.WMG_MAINTHREADTASK, new IntPtr(GConst.WPG_POST_ONGOTFOCUS), IntPtr.Zero);
			}
		}
		protected void PostOnGotFocus() {
			//.NET1.1 SP1 issue
			GEnv.Frame.ActivateConnection(_tag);
			if(!_caretTimer.Enabled) _caretTimer.Start(); //�X�v���b�^�̃h���b�O���L�����Z��������̓��T�C�Y������Ȃ��̂ł��̃^�C�~���O�ŃL�����b�g���ĉғ�����

			if(_selectionKeyProcessor != null) GEnv.Frame.SetSelectionStatus(SelectionStatus.Free);
			else if(_autoSelectionMode) GEnv.Frame.SetSelectionStatus(SelectionStatus.Auto);
			
			//�����̖����Ńt���O���N���A
			_recursiveFocusEventBlockFlag = false;
		}
#endif

		protected override void OnLostFocus(EventArgs args) {
			base.OnLostFocus(args);
			if(!_fakeVisible) return;

			if(_inIMEComposition) ClearIMEComposition();
		}
		private void EnterFreeSelectionMode() {
			TerminalDocument document = _tag.Document;
			_selectionKeyProcessor = new SelectionKeyProcessor(this, document, document.CurrentLine, document.CaretColumn);
			_caretTimer.Interval = Win32.GetCaretBlinkTime()/2;
			GEnv.Frame.SetSelectionStatus(SelectionStatus.Free);
		}
		private void ExitFreeSelectionMode() {
			_selectionKeyProcessor = null;
			GEnv.Frame.SetSelectionStatus(SelectionStatus.None);
			GEnv.TextSelection.Clear();
			_caretTimer.Interval = Win32.GetCaretBlinkTime();
		}

		private void EnterAutoSelectionMode(bool from_command) {
			_autoSelectionMode = true;
			_autoSelectionModeFromCommand = from_command;
			if(from_command)
				GEnv.Frame.SetSelectionStatus(SelectionStatus.Auto);
			
			//���̍s������I���J�n
			_tag.Document.EnsureLine(_tag.Document.CurrentLineNumber+1);
			GEnv.TextSelection.StartSelection(this, _tag.Document.CurrentLine.NextLine, 0, RangeType.Line, -1, -1);
		}
		private void ExitAutoSelectionMode() {
			GEnv.Frame.SetSelectionStatus(SelectionStatus.None);
			_autoSelectionMode = false;
			_autoSelectionModeFromCommand = false;
			GEnv.TextSelection.Clear();
			Invalidate();
		}
		public void ExitTextSelection() {
			if(InFreeSelectionMode) ExitFreeSelectionMode();
			if(InAutoSelectionMode) ExitAutoSelectionMode();
		}

		//Drag&Drop�֌W
		protected override void OnDragEnter(DragEventArgs args) {
			base.OnDragEnter(args);
			ConnectionTag ct = args.Data.GetData(typeof(ConnectionTag)) as ConnectionTag;
			if(ct!=null)
				args.Effect = DragDropEffects.Move;
			else
				GEnv.Frame.OnDragEnter(args);
		}
		protected override void OnDragDrop(DragEventArgs args) {
			base.OnDragDrop(args);
			ConnectionTag ct = args.Data.GetData(typeof(ConnectionTag)) as ConnectionTag;
			if(ct!=null) {
				if(ct==_tag)
					this.Focus();
				else
					GEnv.GlobalCommandTarget.SetConnectionLocation(ct, this);
			}
			else
				GEnv.Frame.OnDragDrop(args);
		}
		private void ProcessVScrollMessage(int cmd) {
			int newval = _VScrollBar.Value;
			switch(cmd) {
				case 0: //SB_LINEUP
					newval--;
					break;
				case 1: //SB_LINEDOWN
					newval++;
					break;
				case 2: //SB_PAGEUP
					newval -= GetConnection().TerminalHeight;
					break;
				case 3: //SB_PAGEDOWN
					newval += GetConnection().TerminalHeight;
					break;
			}

			if(newval<0) newval=0;
			if(newval>_VScrollBar.Maximum-_VScrollBar.LargeChange) newval=_VScrollBar.Maximum-_VScrollBar.LargeChange+1;
			_VScrollBar.Value = newval;
		}

#if true
		protected override void WndProc(ref Message msg) {
			base.WndProc(ref msg);
#if true
			if(msg.Msg==Win32.WM_VSCROLL) {
				//Debug.WriteLine("VSCROLL MSG");
				if(msg.LParam==IntPtr.Zero && _VScrollBar.Enabled)
					ProcessVScrollMessage(msg.WParam.ToInt32()); //���ɂ���Ă�WHEEL�̂�����VSCROLL������Ǝv����
			}
			else if(msg.Msg==Win32.WM_IME_STARTCOMPOSITION) {
				_inIMEComposition = true; //_inIMEComposition��WM_IME_STARTCOMPOSITION�ł����Z�b�g���Ȃ�
				AdjustIMEComposition(0);
			}
			else if(msg.Msg==Win32.WM_IME_ENDCOMPOSITION)
				_inIMEComposition = false;
			else if(msg.Msg==GConst.WMG_MAINTHREADTASK) {
				switch(msg.WParam.ToInt32()) {
					case GConst.WPG_ADJUSTIMECOMPOSITION:
						if(_inIMEComposition) AdjustIMEComposition(0); //�����m�肵���Ƃ��ɑΉ��BDataArrived�̎��_�ł͑S�m�肩�����m�肩�킩��Ȃ��̂łǂ݂̂������ւ���
						break;
					case GConst.WPG_TOGGLESELECTIONMODE:
						ToggleFreeSelectionMode();
						break;
					case GConst.WPG_DATA_ARRIVED:
						InternalDataArrived();
						break;
					case GConst.WPG_POST_ONGOTFOCUS:
						PostOnGotFocus();
						break;
					case GConst.WPG_POST_ONTIMER:
						PostOnTimer();
						break;
						/*
					case GConst.WPG_INVALIDATERECT:
						if(msg.LParam.ToInt32()==0)
							Invalidate();
						else
							Invalidate(_rect_to_invalidate);
						break;
						*/
				}
			}
#endif
		}
#endif

	}


}
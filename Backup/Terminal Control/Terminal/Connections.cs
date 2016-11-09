/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: Connections.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using ThTimer = System.Threading.Timer;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

using Poderosa;
using Poderosa.Text;
using Poderosa.Terminal;
using Poderosa.Communication;

namespace Poderosa.Connection
{
	/// <summary>
	/// �A�v���P�[�V�����S�̂ł̃R�l�N�V�����̃��X�g�ƁA���ꂪ�ǂ̃y�C���Ɋ֘A�t�����Ă��邩���Ǘ�����
	/// </summary>
	public class Connections : IEnumerable {
		//�ڑ����J�������Ɋi�[�����ConnectionTag�z��
		private ArrayList _connections;
		private int _activeIndex;

		//Active�ȋt���Ɋi�[�����z��
		private ArrayList _activatedOrder;

		private KeepAlive _keepAlive;

		internal Connections() {
			_connections = new ArrayList();
			_activeIndex = -1;
			_keepAlive = new KeepAlive();

			_activatedOrder = new ArrayList();
		}
		internal KeepAlive KeepAlive {
			get {
				return _keepAlive;
			}
		}

		public void Add(ConnectionTag t) {
			Debug.Assert(t!=null);
			t.PositionIndex = GEnv.Frame.PositionForNextConnection;
			t.PreservedPositionIndex = t.PositionIndex;
			_connections.Add(t);
			_activatedOrder.Add(t);
		}

		internal void Remove(TerminalConnection con) {
			int i = IndexOf(con);
			if(i==-1) return; //�{���͂��������̂͂�낵���Ȃ���

			ConnectionTag ct = this.TagAt(i);
			_connections.RemoveAt(i);
			_activatedOrder.Remove(ct);
			_activeIndex = Math.Min(_activeIndex, _connections.Count-1);
		}
		public void Replace(ConnectionTag old, ConnectionTag ct) {
			int i = IndexOf(old);
			Debug.Assert(i!=-1);
			_connections[i] = ct;

			i = _activatedOrder.IndexOf(old);
			Debug.Assert(i!=-1);
			_activatedOrder[i] = ct;
		}
		public void Clear() {
			_connections.Clear();
			_activatedOrder.Clear();
			_activeIndex = -1;
		}
		public void CloseAllConnections() {
			ArrayList target = new ArrayList(_connections); //���[�J���R�s�[�����Ȃ��Ɣ񓯊��ɃR���N�V�������ύX���ꂩ�˂Ȃ�
			foreach(ConnectionTag ct in target) {
				ct.Connection.Close();
				ct.IsTerminated = true;
			}
		}
		public ArrayList GetSnapshot() {
			return new ArrayList(_connections);
		}
		public int Count {
			get {
				return _connections.Count;
			}
		}
		public bool LiveConnectionsExist {
			get {
				foreach(ConnectionTag ct in _connections)
					if(!ct.Connection.IsClosed) return true;
				return false;
			}
		}

		public IEnumerator GetEnumerator() {
			return _connections.GetEnumerator();
		}
		public IEnumerable OrderedConnections {
			get {
				ArrayList t = new ArrayList(_activatedOrder);
				t.Reverse();
				return t;
			}
		}
		public int ActiveIndex {
			get {
				return _activeIndex;
			}
		}
		public void BringToActivationOrderTop(ConnectionTag ct) {
			_activeIndex = _connections.IndexOf(ct);
			_activatedOrder.Remove(ct);
			_activatedOrder.Add(ct);
		}
		public TerminalConnection ActiveConnection {
			get {
				if(_activeIndex==-1)
					return null;
				else
					return TagAt(_activeIndex).Connection;
			}
		}

		//positionIndex����v���Aexcluding�ł͂Ȃ����Ŏ���Active�ɂȂ����Ԃ��B�Ȃ����null
		public ConnectionTag GetCandidateOfActivation(int positionIndex, ConnectionTag excluding) {
			for(int i=_activatedOrder.Count-1; i>=0; i--) {
				ConnectionTag ct = (ConnectionTag)_activatedOrder[i];
				if(ct.PositionIndex==positionIndex && ct!=excluding) return ct;
			}
			return null;
		}
		//Preserved�Ȃ���݂Č���
		public ConnectionTag GetCandidateOfLocation(int positionIndex, ConnectionTag excluding) {
			for(int i=_activatedOrder.Count-1; i>=0; i--) {
				ConnectionTag ct = (ConnectionTag)_activatedOrder[i];
				if(ct.PreservedPositionIndex==positionIndex && ct!=excluding) return ct;
			}
			return null;
		}


		public ConnectionTag ActiveTag {
			get {
				if(_activeIndex==-1)
					return null;
				else
					return TagAt(_activeIndex);
			}
		}

		public ConnectionTag TagAt(int index) {
			return (ConnectionTag)_connections[index];
		}
		public ConnectionTag FindTag(TerminalConnection con) {
			foreach(ConnectionTag t in _connections) {
				if(t.Connection==con) return t;
			}
			return null;
		}
		public int IndexOf(TerminalConnection con) {
			int i = 0;
			foreach(ConnectionTag t in _connections) {
				if(t.Connection==con) return i;
				i++;
			}
			return -1;
		}
		public int IndexOf(ConnectionTag tag) {
			int i = 0;
			foreach(ConnectionTag t in _connections) {
				if(t==tag) return i;
				i++;
			}
			return -1;
		}


		public ConnectionTag NextConnection(ConnectionTag c) {
			int i = IndexOf(c);
			return TagAt(i==_connections.Count-1? 0 : i+1);
		}
		public ConnectionTag PrevConnection(ConnectionTag c) {
			int i = IndexOf(c);
			return TagAt(i==0? _connections.Count-1 : i-1);
		}
		public void Reorder(int index, int newindex) {
			ConnectionTag ct = (ConnectionTag)_connections[index];
			_connections.RemoveAt(index);
			_connections.Insert(newindex, ct);
			if(_activeIndex==index) _activeIndex = newindex;
		}

		public ConnectionTag FirstMatch(TagCondition cond) {
			foreach(ConnectionTag ct in _connections) {
				if(cond(ct)) return ct;
			}
			return null;
		}

		internal void Dump() {
			Debug.WriteLine("Connection List");
			foreach(ConnectionTag ct in _connections) {
				Debug.WriteLine(String.Format("pos={0} close={1} pane={2} visible={3}", ct.PositionIndex, ct.Connection.IsClosed, (ct.Pane!=null), (ct.Pane!=null && ct.Pane.FakeVisible)));
				if(ct.Pane!=null) Debug.Assert(ct.Pane.Connection == ct.Connection);
			}
		}

	}

	public delegate bool TagCondition(ConnectionTag pane);

	//Invalidate�ɕK�v�ȃp�����[�^
	internal class InvalidateParam {
		private Delegate _delegate;
		private object[] _param;
		private bool _set;
		public void Set(Delegate d, object[] p) {
			_delegate = d;
			_param = p;
			_set = true;
		}
		public void Reset() {
			_set = false;
		}
		public void InvokeFor(Control c) {
			if(_set) c.Invoke(_delegate, _param);
		}
	}


	//�ڑ��ɑ΂��Ċ֘A�t����f�[�^
    [Serializable]
	public class ConnectionTag {
		private TerminalConnection _connection;
		private Control _tabButton; //�^�u�̒��ɓ���{�^��

		private TerminalPane _pane; //�y�C���B��\���̂Ƃ���null
		private TerminalDocument _document;
		private ITerminal _terminal;
		private TerminalDataReceiver _receiver;
		private RenderProfile _renderProfile;
		private IModalTerminalTask _modalTerminalTask;
		private Process _childProcess;

		private InvalidateParam _invalidateParam;
		
		//�E�B���h�E�̕\���p�e�L�X�g
		internal string _windowTitle; //�z�X�gOSC�V�[�P���X�Ŏw�肳�ꂽ�^�C�g��
		
		private bool _terminated;
		private int _positionIndex;
		private int _preservedPositionIndex; //Single���[�h�ɂȂ����Ƃ��̂��߂̑ޔ�p

		private ThTimer _timer;

		public ConnectionTag(TerminalConnection c) {
			_connection = c;
			_pane = null;
			_invalidateParam = new InvalidateParam();
			_tabButton = null;
			_document = new TerminalDocument(_connection);
			_receiver = new TerminalDataReceiver(this);
			_terminated = false;
			_timer = null;
			_windowTitle = "";

			//null�̂Ƃ��̓f�t�H���g�v���t�@�C�����g��
			_renderProfile = c.Param.RenderProfile;

			//VT100�w��ł�xterm�V�[�P���X�𑗂��Ă���A�v���P�[�V��������������Ȃ��̂�
			_terminal = new XTerm(this, new JapaneseCharDecoder(_connection));
			/*
			if(c.Param.TerminalType==TerminalType.XTerm || c.Param.TerminalType==TerminalType.KTerm)
				_terminal = new XTerm(this, new JapaneseCharDecoder(_connection));
			else
				_terminal = new VT100Terminal(this, new JapaneseCharDecoder(_connection));
			*/
			GEnv.Connections.KeepAlive.SetTimerToConnectionTag(this);
		}

		//�h�L�������g�X�V�ʒm ��M�X���b�h�ł̎��s�Ȃ̂Œ���
		public interface IEventReceiver {
			void OnUpdate();
			void OnDisconnect();
		}
		private IEventReceiver _eventReceiver;

		public IEventReceiver EventReceiver {
			get {
				return _eventReceiver;
			}
			set {
				_eventReceiver = value;
			}
		}
		public IModalTerminalTask ModalTerminalTask {
			get {
				return _modalTerminalTask;
			}
			set {
				_modalTerminalTask = value;
			}
		}
		public Process ChildProcess {
			get {
				return _childProcess;
			}
			set {
				_childProcess = value;
			}
		}

		internal InvalidateParam InvalidateParam {
			get {
				return _invalidateParam;
			}
		}	  


		public string WindowTitle {
			get {
				return _windowTitle;
			}
			set {
				_windowTitle = value;
			}
		}
		public string FormatTabText() {
			string t = _connection.Param.Caption;
			if(t==null || t.Length==0) t = _connection.Param.ShortDescription;
			if(_connection.IsClosed) t += GEnv.Strings.GetString("Caption.ConnectionTag.Disconnected");
			if(_modalTerminalTask!=null) t += "("+_modalTerminalTask.Caption+")";
			return t;
		}
		//Frame�̃L���v�V�����p������
		public string FormatFrameText() {
			string t = FormatTabText();
			if(_windowTitle.Length!=0 && _connection.Param.Caption!=_windowTitle) //TabCaption��WindowTitle�Ɉ�v������I�v�V�������g���Ă���ƟT�������Ȃ�̂ňقȂ�Ƃ��̂ݕ\��
				t += "[" + _windowTitle + "]";
			return t;
		}

		public Control Button {
			get {
				return _tabButton;
			}
			set {
				_tabButton = value;
			}
		}
		public TerminalDocument Document {
			get {
				return _document;
			}
		}
		public TerminalConnection Connection {
			get {
				return _connection;
			}
		}
		public TerminalPane AttachedPane {
			get {
				return _pane;
			}
		}
		public ITerminal Terminal {
			get {
				return _terminal;
			}
		}
		public TerminalDataReceiver Receiver {
			get {
				return _receiver;
			}
		}
        
		public RenderProfile RenderProfile {
			get {
				return _renderProfile;
			}
			set {
				_renderProfile = value;
				_connection.Param.RenderProfile = value;
			}
		}
		internal RenderProfile GetCurrentRenderProfile() {
			return _renderProfile==null? GEnv.DefaultRenderProfile : _renderProfile;
		}
		
		internal TerminalPane Pane {
			get {
				return _pane;
			}
			set {
				_pane = value;
			}
		}
		internal ThTimer Timer {
			get {
				return _timer;
			}
			set {
				_timer = value;
			}
		}

		internal void NotifyUpdate() {
			if(_pane!=null)
				_pane.DataArrived();

			_terminal.SignalData();

			if(_eventReceiver!=null)
				_eventReceiver.OnUpdate();
		}
		internal void NotifyDisconnect() {
			if(_eventReceiver!=null)
				_eventReceiver.OnDisconnect();
		}


		public int PositionIndex {
			get {
				return _positionIndex;
			}
			set {
				_positionIndex = value;
			}
		}

		public int PreservedPositionIndex {
			get {
				return _preservedPositionIndex;
			}
			set {
				_preservedPositionIndex = value;
			}
		}
		public bool IsTerminated {
			get {
				return _terminated;
			}
			set {
				_terminated = value;
				if(value && _childProcess!=null) {
					try {
						//_childProcess.Kill(); ����Kill����Ƃ܂���bash�c���ł܂���
						_childProcess = null;
					}
					catch(Exception) { //���Ƀ\�P�b�g�ؒf�ɋN������Η�O�ɂȂ邱�Ƃ����邩������Ȃ�
					}
				}
				if(value) GEnv.Connections.KeepAlive.ClearTimerToConnectionTag(this);
			}
		}

		public void ImportProperties(ConnectionTag src) {
			_renderProfile = src.RenderProfile;
			_positionIndex = src.PositionIndex;
			_preservedPositionIndex = src.PreservedPositionIndex;

			if(src.Button!=null)
				src.Button.Tag = this;
			_tabButton = src.Button;
		}
	}
}

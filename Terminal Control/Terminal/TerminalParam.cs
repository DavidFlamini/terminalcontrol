/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalParam.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

using Poderosa.Terminal;
#if !MACRODOC
using Poderosa.Config;
using Poderosa.Communication;
using Poderosa.Text;
using Poderosa.LocalShell;
#endif
using Poderosa.Toolkit;

namespace Poderosa.ConnectionParam {

	/*
	 * TerminalParam�̓}�N��������t���ɃA�N�Z�X�\�ɂ��邽��public�ɂ���
	 * ���J����K�v�̂Ȃ����\�b�h��internal�ɂ���
	 */ 

	//Granados����AuthenticationType�Ɠ��ꂾ���A�N���̍������̂��ߎg��Ȃ�
	
	/// <summary>
	/// <ja>SSH�ł̔F�ؕ��@�������܂��B</ja>
	/// <en>Specifies the authemtication method of SSH.</en>
	/// </summary>
	[EnumDesc(typeof(AuthType))]
	public enum AuthType {
		/// <summary>
		/// <ja>�p�X���[�h�F��</ja>
		/// <en>Authentication using password.</en>
		/// </summary>
		[EnumValue(Description="Enum.AuthType.Password")]
		Password,

		/// <summary>
		/// <ja>�茳�̔閧���ƃ����[�g�z�X�g�ɓo�^�������J�����g�����F��</ja>
		/// <en>Authentication using the local private key and the remote public key.</en>
		/// </summary>
		[EnumValue(Description="Enum.AuthType.PublicKey")]
		PublicKey,

		/// <summary>
		/// <ja>�R���\�[����Ńp�X���[�h����͂���F��</ja>
		/// <en>Authentication by sending the password through the console.</en>
		/// </summary>
		[EnumValue(Description="Enum.AuthType.KeyboardInteractive")]
		KeyboardInteractive
	}

	/// <summary>
	/// <ja>�G���R�[�f�B���O�������܂��B</ja>
	/// <en>Specifies the encoding of the connection.</en>
	/// <seealso cref="TerminalParam.Encoding"/>
	/// </summary>
	[EnumDesc(typeof(EncodingType))]
	public enum EncodingType {
		/// <summary>
		/// <ja>iso-8859-1</ja>
		/// <en>iso-8859-1</en>
		/// </summary>
		[EnumValue(Description="Enum.EncodingType.ISO8859_1")] ISO8859_1,
		/// <summary>
		/// <ja>utf-8</ja>
		/// <en>utf-8</en>
		/// </summary>
		[EnumValue(Description="Enum.EncodingType.UTF8")] UTF8,
		/// <summary>
		/// <ja>euc-jp</ja>
		/// <en>euc-jp (This encoding is primarily used with Japanese characters.)</en>
		/// </summary>
		[EnumValue(Description="Enum.EncodingType.EUC_JP")] EUC_JP,
		/// <summary>
		/// <ja>shift-jis</ja>
		/// <en>shift-jis (This encoding is primarily used with Japanese characters.)</en>
		/// </summary>
		[EnumValue(Description="Enum.EncodingType.SHIFT_JIS")] SHIFT_JIS
	}

	/// <summary>
	/// <ja>���O�̎�ނ������܂��B</ja>
	/// <en>Specifies the log type.</en>
	/// </summary>
	[EnumDesc(typeof(LogType))]
	public enum LogType {
		/// <summary>
		/// <ja>���O�͂Ƃ�܂���B</ja>
		/// <en>The log is not recorded.</en>
		/// </summary>
		[EnumValue(Description="Enum.LogType.None")] None,
		/// <summary>
		/// <ja>�e�L�X�g���[�h�̃��O�ł��B���ꂪ�W���ł��B</ja>
		/// <en>The log is a plain text file. This is standard.</en>
		/// </summary>
		[EnumValue(Description="Enum.LogType.Default")] Default,
		/// <summary>
		/// <ja>�o�C�i�����[�h�̃��O�ł��B</ja>
		/// <en>The log is a binary file.</en>
		/// </summary>
		[EnumValue(Description="Enum.LogType.Binary")] Binary,
		/// <summary>
		/// <ja>XML�ŕۑ����܂��B�܂������I�ȃo�O�ǐՂɂ����Ă��̃��[�h�ł̃��O�̎�����肢���邱�Ƃ�����܂��B</ja>
		/// <en>The log is an XML file. We may ask you to record the log in this type for debugging.</en>
		/// </summary>
		[EnumValue(Description="Enum.LogType.Xml")] Xml
	}

	/// <summary>
	/// <ja>���M���̉��s�̎�ނ������܂��B</ja>
	/// <en>Specifies the new-line characters for transmission.</en>
	/// </summary>
	[EnumDesc(typeof(NewLine))]
	public enum NewLine {
		/// <summary>
		/// CR
		/// </summary>
		[EnumValue(Description="Enum.NewLine.CR")] CR,
		/// <summary>
		/// LF
		/// </summary>
		[EnumValue(Description="Enum.NewLine.LF")] LF,
		/// <summary>
		/// CR+LF
		/// </summary>
		[EnumValue(Description="Enum.NewLine.CRLF")] CRLF
	}

	/// <summary>
	/// <ja>�^�[�~�i���̎�ʂ������܂��B</ja>
	/// <en>Specifies the type of the terminal.</en>
	/// </summary>
	/// <remarks>
	/// <ja>XTerm�ɂ�VT100�ɂ͂Ȃ��������̃G�X�P�[�v�V�[�P���X���܂܂�Ă��܂��B</ja>
	/// <en>XTerm supports several escape sequences in addition to VT100.</en>
	/// <ja>KTerm�͒��g��XTerm�ƈꏏ�ł����ASSH��Telnet�̐ڑ��I�v�V�����ɂ����ă^�[�~�i���̎�ނ�����������Ƃ���"kterm"���Z�b�g����܂��B</ja>
	/// <en>Though the functionality of KTerm is identical to XTerm, the string "kterm" is used for specifying the type of the terminal in the connection of Telnet or SSH.</en>
	/// <ja>���̐ݒ�́A�����̏ꍇTERM���ϐ��̒l�ɉe�����܂��B</ja>
	/// <en>In most cases, this setting affects the TERM environment variable.</en>
	/// </remarks>
	[EnumDesc(typeof(TerminalType))]
	public enum TerminalType {
		/// <summary>
		/// vt100
		/// </summary>
		[EnumValue(Description="Enum.TerminalType.VT100")] VT100,
		/// <summary>
		/// xterm
		/// </summary>
		[EnumValue(Description="Enum.TerminalType.XTerm")] XTerm,
		/// <summary>
		/// kterm
		/// </summary>
		[EnumValue(Description="Enum.TerminalType.KTerm")] KTerm
}

	/// <summary>
	/// <ja>�ڑ��̎�ނ������܂��B</ja>
	/// <en>Specifies the type of the connection.</en>
	/// </summary>
	public enum ConnectionMethod {
		/// <summary>
		/// Telnet
		/// </summary>
		Telnet,
		/// <summary>
		/// SSH1
		/// </summary>
		SSH1,
		/// <summary>
		/// SSH2
		/// </summary>
		SSH2
	}

	/// <summary>
	/// <ja>�w�i�摜�̈ʒu���w�肵�܂��B</ja>
	/// <en>Specifies the position of the background image.</en>
	/// </summary>
	[EnumDesc(typeof(ImageStyle))]
	public enum ImageStyle {
		/// <summary>
		/// <ja>����</ja>
		/// <en>Center</en>
		/// </summary>
		[EnumValue(Description="Enum.ImageStyle.Center")] Center,
		/// <summary>
		/// <ja>����</ja>
		/// <en>Upper left corner</en>
		/// </summary>
		[EnumValue(Description="Enum.ImageStyle.TopLeft")] TopLeft,
		/// <summary>
		/// <ja>�E��</ja>
		/// <en>Upper right corner</en>
		/// </summary>
		[EnumValue(Description="Enum.ImageStyle.TopRight")] TopRight,
		/// <summary>
		/// <ja>����</ja>
		/// <en>Lower left corner</en>
		/// </summary>
		[EnumValue(Description="Enum.ImageStyle.BottomLeft")] BottomLeft,
		/// <summary>
		/// <ja>�E��</ja>
		/// <en>Lower right corner</en>
		/// </summary>
		[EnumValue(Description="Enum.ImageStyle.BottomRight")] BottomRight,
		/// <summary>
		/// <ja>�L�k���đS�̂ɕ\��</ja>
		/// <en>The image covers the whole area of the console by expansion</en>
		/// </summary>
		[EnumValue(Description="Enum.ImageStyle.Scaled")] Scaled
	}

	/// <summary>
	/// <ja>��M���������ɑ΂�����s���@�������܂��B</ja>
	/// <en>Specifies line breaking style.</en>
	/// </summary>
	[EnumDesc(typeof(LineFeedRule))]
	public enum LineFeedRule {
		/// <summary>
		/// <ja>�W��</ja>
		/// <en>Standard</en>
		/// </summary>
		[EnumValue(Description="Enum.LineFeedRule.Normal")] Normal,
		/// <summary>
		/// <ja>LF�ŉ��s��CR�𖳎�</ja>
		/// <en>LF:Line Break, CR:Ignore</en>
		/// </summary>
		[EnumValue(Description="Enum.LineFeedRule.LFOnly")] LFOnly,
		/// <summary>
		/// <ja>CR�ŉ��s��LF�𖳎�</ja>
		/// <en>CR:Line Break, LF:Ignore</en>
		/// </summary>
		[EnumValue(Description="Enum.LineFeedRule.CROnly")] CROnly
	}


	/// <summary>
	/// <ja>�ڑ����J���Ƃ��̃p�����[�^�̊��N���X�ł��B</ja>
	/// <en>Implements the basic functionality common to connections.</en>
	/// <seealso cref="TCPTerminalParam"/>
	/// <seealso cref="TelnetTerminalParam"/>
	/// <seealso cref="SSHTerminalParam"/>
	/// <seealso cref="SerialTerminalParam"/>
	/// <seealso cref="CygwinTerminalParam"/>
	/// </summary>
	[Serializable()]
	public abstract class TerminalParam : ICloneable {

		internal EncodingType _encoding;
		internal TerminalType _terminalType;
		internal LogType _logtype;
		internal string _logpath;
		internal bool _logappend;
		internal bool _localecho;
		internal LineFeedRule _lineFeedRule;
		internal NewLine _transmitnl;
		internal string _caption;
		internal RenderProfile _renderProfile;
		
		internal TerminalParam() {
			_encoding = EncodingType.EUC_JP;
			_logtype = LogType.None;
			_terminalType = TerminalType.XTerm;
			_localecho = false;
			_lineFeedRule = LineFeedRule.Normal;
			_transmitnl = NewLine.CR;
			_renderProfile = null;
		}

		internal TerminalParam(TerminalParam r) {
			Import(r);
		}
		internal void Import(TerminalParam r) {
			_encoding = r._encoding;
			_logtype = r._logtype;
			_logpath = r._logpath;
			_localecho = r._localecho;
			_transmitnl = r._transmitnl;
			_lineFeedRule = r._lineFeedRule;
			_terminalType = r._terminalType;
			_renderProfile = r._renderProfile==null? null : new RenderProfile(r._renderProfile);
			_caption = r._caption;
		}

		/// 
		/// 
		/// 
		/// 
		public override bool Equals(object t_) {
			TerminalParam t = t_ as TerminalParam;
			if(t==null) return false;

			return
				_encoding==t.Encoding &&
				_localecho==t.LocalEcho &&
				_transmitnl==t.TransmitNL &&
				_lineFeedRule==t.LineFeedRule &&
				_terminalType==t.TerminalType;
		}

		/// 
		/// 
		/// 
		public override int GetHashCode() {
			return _encoding.GetHashCode() + _localecho.GetHashCode()*2 + _transmitnl.GetHashCode()*3 + _lineFeedRule.GetHashCode()*4 + _terminalType.GetHashCode()*5;
		}

		/// 
		/// 
		/// 
		public abstract object Clone();

#if !MACRODOC
		public abstract string Description { get; }

		public abstract string ShortDescription { get; }

		public abstract string MethodName { get; }

		public virtual void Export(ConfigNode node) {
			node["encoding"] = EnumDescAttribute.For(typeof(EncodingType)).GetDescription(_encoding);
			node["terminal-type"] = EnumDescAttribute.For(typeof(TerminalType)).GetName(_terminalType);
			node["transmit-nl"] = EnumDescAttribute.For(typeof(NewLine)).GetName(_transmitnl);
			node["localecho"] = _localecho.ToString();
			node["linefeed"] = EnumDescAttribute.For(typeof(LineFeedRule)).GetName(_lineFeedRule);
			if(_caption!=null && _caption.Length>0) node["caption"] = _caption;
			if(_renderProfile!=null) _renderProfile.Export(node);
		}
		public virtual void Import(ConfigNode data) {
			_encoding = ParseEncoding(data["encoding"]);
			_terminalType = (TerminalType)EnumDescAttribute.For(typeof(TerminalType)).FromName(data["terminal-type"], TerminalType.VT100);
			_transmitnl = (NewLine)EnumDescAttribute.For(typeof(NewLine)).FromName(data["transmit-nl"], NewLine.CR);
			_localecho = GUtil.ParseBool(data["localecho"], false);
			//_lineFeedByCR = GUtil.ParseBool((string)data["linefeed-by-cr"], false);
			_lineFeedRule = (LineFeedRule)EnumDescAttribute.For(typeof(LineFeedRule)).FromName(data["linefeed"], LineFeedRule.Normal);
			_caption = data["caption"];
			if(data.Contains("font-name")) //���ڂ��Ȃ���΋�̂܂�
				_renderProfile = new RenderProfile(data);
		}
		
		public EncodingProfile EncodingProfile {
			get {
				return EncodingProfile.Get(_encoding);
			}
			set {
				_encoding = value.Type;
			}
		}

#endif
		/// <summary>
		/// <ja>����TerminalParam�Őڑ����J�����Ƃ��̐F�E�t�H���g�Ȃǂ̐ݒ�����^�����I�u�W�F�N�g�ł��B</ja>
		/// <en>Gets or sets the appearances of the console such as colors or fonts.</en>
		/// </summary>
		/// <remarks>
		/// <ja>���ɉ����w�肵�Ȃ�����null���Z�b�g����ƁA�I�v�V�����_�C�A���O�Őݒ肵�����e���g�p����܂��B</ja>
		/// <en>If you do not set anything or set null, the appearance is same as the setting of the option dialo.</en>
		/// </remarks>
		/// <seealso cref="RenderProfile"/>
		public RenderProfile RenderProfile {
			get {
				return _renderProfile;
			}
			set {
				_renderProfile = value;
			}
		}
		

		/// <summary>
		/// <ja>���̐ڑ��̃G���R�[�f�B���O�ł��B</ja>
		/// <en>Gets or sets the encoding of the connection.</en>
		/// </summary>
		public EncodingType Encoding {
			get {
				return _encoding;
			}
			set {
				_encoding = value;
			}
		}

		/// <summary>
		/// <ja>�^�[�~�i���̎�ʂł��B</ja>
		/// <en>Gets or sets the type of the terminal.</en>
		/// </summary>
		public TerminalType TerminalType {
			get {
				return _terminalType;
			}
			set {
				_terminalType = value;
			}
		}

		/// <summary>
		/// <ja>���O�̎�ʂł��B</ja>
		/// <en>Gets or sets the type of the log.</en>
		/// </summary>
		public LogType LogType {
			get {
				return _logtype;
			}
			set {
				_logtype = value;
			}
		}
		/// <summary>
		/// <ja>���O�t�@�C���̃t���p�X�ł��B</ja>
		/// <en>Gets or sets the full path of the log file.</en>
		/// </summary>
		public string LogPath {
			get {
				return _logpath;
			}
			set {
				_logpath = value;
			}
		}

		/// <summary>
		/// <ja>���O�̎�ʂƃt�@�C�������I�v�V�����_�C�A���O�Ŏw�肵�����e�Ɋ�Â������I�ɐݒ肵�܂��B</ja>
		/// <en>Sets the type and the file name of the log based on the settings in the option dialog.</en>
		/// </summary>
		public void AutoFillLogPath() {
#if !MACRODOC
			_logtype = GEnv.Options.DefaultLogType;
			if(_logtype!=LogType.None)
				_logpath = GUtil.CreateLogFileName(this.ShortDescription);
			else
				_logpath = "";
#endif
		}

		/// <summary>
		/// <ja>�����t�@�C��������ꍇ�A���O�t�@�C���ɒǋL���邩�㏑�����邩���w�肵�܂��B</ja>
		/// <en>Specifies whether the connection appends or overwrites the log file in case that the file exists already.</en>
		/// </summary>
		public bool LogAppend {
			get {
				return _logappend;
			}
			set {
				_logappend = false;
			}
		}

		/// <summary>
		/// <ja>���M���̉��s�ݒ�ł��B</ja>
		/// <en>Gets or sets the new-line characters for transmission.</en>
		/// </summary>
		public NewLine TransmitNL {
			get {
				return _transmitnl;
			}
			set {
				_transmitnl = value;
			}
		}

		/// <summary>
		/// <ja>���[�J���G�R�[�����邩�ǂ����ł��B</ja>
		/// <en>Specifies whether the local echo is performed.</en>
		/// </summary>
		public bool LocalEcho {
			get {
				return _localecho;
			}
			set {
				_localecho = value;
			}
		}

		/// <summary>
		/// <ja>��M���������ɑ΂��ĉ��s���邩�ǂ����ł��B</ja>
		/// <en>Specifies line breaking style corresponding to received characters.</en>
		/// </summary>
		public LineFeedRule LineFeedRule {
			get {
				return _lineFeedRule;
			}
			set {
				_lineFeedRule = value;
			}
		}

		/// <summary>
		/// <ja>�^�u�Ȃǂɕ\�����邽�߂̌��o���ł��B</ja>
		/// <en>Gets or sets the caption of the tab.</en>
		/// <ja>���ɃZ�b�g���Ȃ��ꍇ�A�ڑ���̃z�X�g���𗘗p���Ď����I�ɂ����܂��B</ja>
		/// <en>If you do not specify anything, the caption is set automatically using the host name.</en>
		/// </summary>
		public string Caption {
			get {
				return _caption;
			}
			set {
				_caption = value;
			}
		}

#if !MACRODOC

		public void FeedLogOption() {
			if(GEnv.Options.DefaultLogType!=LogType.None) {
				_logtype = GEnv.Options.DefaultLogType;
				_logpath = GUtil.CreateLogFileName(this.ShortDescription);
			}
		}
		public static TerminalParam CreateFromConfigNode(ConfigNode sec) {
			string type = sec["type"];
			TerminalParam param;
			if(type=="serial")
				param = new SerialTerminalParam();
			else if(type=="tcp") {
				ConnectionMethod cm = ParseMethod(sec["method"]);
				if(cm==ConnectionMethod.Telnet)
					param = new TelnetTerminalParam();
				else
					param = new SSHTerminalParam();
			}
			else if(type=="cygwin")
				param = new CygwinTerminalParam();
			else if(type=="sfu")
				param = new SFUTerminalParam();
			else
				throw new Exception("invalid format");
			param.Import(sec);
			return param;
		}

		private static EncodingType ParseEncoding(string val) {
			return (EncodingType)EnumDescAttribute.For(typeof(EncodingType)).FromDescription(val, EncodingType.UTF8);
		}

		protected static ConnectionMethod ParseMethod(string val) {
			if(val=="SSH1")
				return ConnectionMethod.SSH1;
			if(val=="SSH2")
				return ConnectionMethod.SSH2;
			if(val=="Telnet")
				return ConnectionMethod.Telnet;

			throw new FormatException(String.Format("{0} is unkown method", val));
		}
#endif
	}

	/// <summary>
	/// <ja>TCP�Ɋ�Â����ڑ��̃p�����[�^��\�����܂��B</ja>
	/// <en>Implements the parameters of the connection using TCP. (i.e. Telnet and SSH)</en>
	/// <seealso cref="TelnetTerminalParam"/>
	/// <seealso cref="SSHTerminalParam"/>
	/// </summary>
	[Serializable()]
	public abstract class TCPTerminalParam : TerminalParam {

		internal string _host;
		internal int _port;
		internal ConnectionMethod _method;

		internal TCPTerminalParam() {
			_method = ConnectionMethod.Telnet;
		}

		internal TCPTerminalParam(TCPTerminalParam r) : base(r) {
			_host = r._host;
			_port = r._port;
			_method = r._method;
		}
		internal void Import(TCPTerminalParam r) {
			base.Import(r);
			_host = r._host;
			_port = r._port;
			_method = r._method;
		}

		/// 
		/// 
		/// 
		public override bool Equals(object t_) {
			TCPTerminalParam t = t_ as TCPTerminalParam;
			if(t==null) return false;

			return base.Equals(t) && _host==t.Host && _port==t.Port && _method==t.Method;
		}
		/// 
		/// 
		/// 
		public override int GetHashCode() {
			return base.GetHashCode() + _host.GetHashCode() + _port.GetHashCode()*2 + _method.GetHashCode()*3;
		}

		/// <summary>
		/// <ja>�ڑ���̃z�X�g���ł��B</ja>
		/// <en>Gets or sets the host name.</en>
		/// </summary>
		/// <remarks>
		/// <ja>�܂���"192.168.10.1"�Ȃǂ�IP�A�h���X�̕�����\�����\�ł��B</ja>
		/// <en>The IP address format such as "192.168.10.1" is also allowed.</en>
		/// </remarks>
		public virtual string Host {
			get {
				return _host;
			}
			set {
				_host = value;
			}
		}

		/// <summary>
		/// <ja>�ڑ���̃|�[�g�ԍ��ł��B</ja>
		/// <en>Gets or sets the port number.</en>
		/// </summary>
		public virtual int Port {
			get {
				return _port;
			}
			set {
				_port = value;
			}
		}

		/// <summary>
		/// <ja>�ڑ��̎�ʂł��B</ja>
		/// <en>Gets or sets the connection method.</en>
		/// </summary>
		public virtual ConnectionMethod Method {
			get {
				return _method;
			}
			set {
#if !MACRODOC
				throw new ArgumentException(GEnv.Strings.GetString("Message.TCPTerminalParam.PropCannotBeSet"));
#endif
			}
		}

		/// <summary>
		/// <ja>���̐ڑ���SSH�ł����true�ł��B</ja>
		/// <en>Returns true if the connection method is SSH.</en>
		/// </summary>
		public bool IsSSH {
			get {
				return _method==ConnectionMethod.SSH1 || _method==ConnectionMethod.SSH2;
			}
		}

#if !MACRODOC			
		public override void Export(ConfigNode node) {
			node["type"] = "tcp";
			node["host"] = _host;
			node["port"] = _port.ToString();
			node["method"] = _method.ToString();
			base.Export(node);
		}

		public override void Import(ConfigNode data) {
			_host = data["host"];
			_port = ParsePort(data["port"]);
			_method = ParseMethod(data["method"]);
			base.Import(data);
		}

		//TerminalUtil�ֈړ����ׂ�����
		private static int ParsePort(string val) {
			try {
				return Int32.Parse(val);
			}
			catch(FormatException e) {
				throw e;
			}
		}

		public static TCPTerminalParam Fake {
			get {
				TelnetTerminalParam p = new TelnetTerminalParam();
				p.EncodingProfile = EncodingProfile.Get(EncodingType.EUC_JP);
				return p;
			}
		}
		public override string ShortDescription {
			get {
				return _host;
			}
		}

		public override string MethodName {
			get {
				return _method==ConnectionMethod.SSH1? "SSH1" : _method==ConnectionMethod.SSH2? "SSH2" : "telnet";
			}
		}
#endif

	}

	/// <summary>
	/// <ja>Telnet�ɂ��ڑ��p�����[�^�������N���X</ja>
	/// <en>Implements the parameters of the Telnet connections.</en>
	/// </summary>
	[Serializable()]
	public class TelnetTerminalParam : TCPTerminalParam {

		/// <summary>
		/// <ja>�z�X�g�����w�肵�č쐬���܂��B</ja>
		/// <en>Initializes with the host name.</en>
		/// <seealso cref="Poderosa.Macro.ConnectionList.Open"/>
		/// </summary>
		/// <remarks>
		/// <ja>�|�[�g��23�ɐݒ肳��܂��B</ja>
		/// <en>The port number is set to 23.</en>
		/// <ja>���̃p�����[�^�͎��̂悤�ɏ���������܂��B</ja>
		/// <en>Other parameters are initialized as following:</en>
		/// <list type="table">
		///   <item><term><ja>�G���R�[�f�B���O</ja><en>Encoding</en></term><description><ja>EUC-JP</ja><en>iso-8859-1</en></description></item>�@
		///   <item><term><ja>�^�[�~�i���^�C�v</ja><en>Terminal Type</en></term><description>xterm</description></item>  
		///   <item><term><ja>���O</ja><en>Log</en></term><description><ja>�擾���Ȃ�</ja><en>None</en></description></item>�@�@�@�@�@�@�@
		///   <item><term><ja>���[�J���G�R�[</ja><en>Local echo</en></term><description><ja>���Ȃ�</ja><en>Don't</en></description></item>�@�@
		///   <item><term><ja>���M�����s</ja><en>New line</en></term><description>CR</description></item>�@�@�@�@
		/// </list>
		/// <ja>�ڑ����J���ɂ́A<see cref="Poderosa.Macro.ConnectionList.Open"/>���\�b�h�̈����Ƃ���TelnetTerminalParam�I�u�W�F�N�g��n���܂��B</ja>
		/// <en>To open a new connection, pass the TelnetTerminalParam object to the <see cref="Poderosa.Macro.ConnectionList.Open"/> method.</en>
		/// </remarks>
		/// <param name="host"><ja>�z�X�g��</ja><en>The host name.</en></param>
		public TelnetTerminalParam(string host) {
			_method = ConnectionMethod.Telnet;
			_host = host;
			_port = 23;
		}


		internal TelnetTerminalParam() {
			_method = ConnectionMethod.Telnet;
		}
		internal TelnetTerminalParam(TelnetTerminalParam r) : base(r) {
		}

		/// 
		/// 
		/// 
		public override object Clone() {
			return new TelnetTerminalParam(this);
		}
#if !MACRODOC
		public override void Import(ConfigNode data) {
			base.Import(data);
		}
		public  override string Description {
			get {
				return _host;
			}
		}
#endif
	}


	/// <summary>
	/// <ja>SSH�ɂ��ڑ��p�����[�^�ł��B</ja>
	/// <en>Implements the parameters of SSH connections.</en>
	/// </summary>
	[Serializable()]
	public class SSHTerminalParam : TCPTerminalParam {
		internal string _account;
		internal string _passphrase; //����̓V���A���C�Y�̑ΏۊO�B��������Ɏ����ǂ������I�v�V����
		internal AuthType _auth;
		internal string _identityfile;

		/// <summary>
		/// <ja>�z�X�g���A�A�J�E���g�A�p�X���[�h���w�肵�č쐬���܂��B</ja>
		/// <en>Initializes with the host name, the account, and the password.</en>
		/// <seealso cref="Poderosa.Macro.ConnectionList.Open"/>
		/// </summary>
		/// <remarks>
		/// <ja>�|�[�g��22�ɐݒ肳��܂��B</ja>
		/// <en>The port number is set to 22.</en>
		/// <ja>���̃p�����[�^�͎��̂悤�ɏ���������܂��B</ja>
		/// <en>Other parameters are initialized as following:</en>
		/// <list type="table">
		///   <item><term><ja>�G���R�[�f�B���O</ja><en>Encoding</en></term><description><ja>EUC-JP</ja><en>iso-8859-1</en></description></item>�@
		///   <item><term><ja>�^�[�~�i���^�C�v</ja><en>Terminal Type</en></term><description>xterm</description></item>  
		///   <item><term><ja>���O</ja><en>Log</en></term><description><ja>�擾���Ȃ�</ja><en>None</en></description></item>�@�@�@�@�@�@�@
		///   <item><term><ja>���[�J���G�R�[</ja><en>Local echo</en></term><description><ja>���Ȃ�</ja><en>Don't</en></description></item>�@�@
		///   <item><term><ja>���M�����s</ja><en>New line</en></term><description>CR</description></item>�@�@�@�@
		///   <item><term><ja>�F�ؕ��@</ja><en>Authentication Method</en></term><description><ja>�p�X���[�h</ja><en>Password</en></description></item>�@�@�@�@
		/// </list>
		/// <ja>�ڑ����J���ɂ́AConnectionList�I�u�W�F�N�g��<see cref="Poderosa.Macro.ConnectionList.Open"/>���\�b�h�̈����Ƃ���SSHTerminalParam�I�u�W�F�N�g��n���܂��B</ja>
		/// <en>To open a new connection, pass the SSHTerminalParam object to the <see cref="Poderosa.Macro.ConnectionList.Open"/> method of the ConnectionList object.</en>
		/// </remarks>
		/// <param name="method"><ja>SSH1�܂���SSH2</ja><en>SSH1 or SSH2.</en></param>
		/// <param name="host"><ja>�z�X�g��</ja><en>The host name.</en></param>
		/// <param name="account"><ja>�A�J�E���g��</ja><en>The account</en></param>
		/// <param name="password"><ja>�p�X���[�h�܂��͔閧���̃p�X�t���[�Y</ja><en>The password or the passphrase of the private key.</en></param>
		public SSHTerminalParam(ConnectionMethod method, string host, string account, string password) {
			if(method==ConnectionMethod.Telnet) throw new ArgumentException("Telnet is specified in the constructor of SSHTerminalParam");
			_method = method;
			_host = host;
			_port = 22;
			_account = account;
			_passphrase = password;
			_auth = AuthType.Password;
		}
		internal SSHTerminalParam(SSHTerminalParam r) : base(r) {
			_account = r._account;
			_auth = r._auth;
			_identityfile = r._identityfile;
			_passphrase = r._passphrase;
		}

		internal SSHTerminalParam() {
			_method = ConnectionMethod.SSH2;
			_auth = AuthType.Password;
			_port = 22;
		}

		/// 
		/// 
		/// 
		public override object Clone() {
			return new SSHTerminalParam(this);
		}
		/// 
		/// 
		/// 
		public override bool Equals(object t_) {
			SSHTerminalParam t = t_ as SSHTerminalParam;
			if(t==null) return false;

			return base.Equals(t) && _account==t.Account && _auth==t.AuthType;
		}
		/// 
		/// 
		/// 
		public override int GetHashCode() {
			return base.GetHashCode() + _account.GetHashCode() + _auth.GetHashCode()*2;
		}

		/// <summary>
		/// <ja>�A�J�E���g���ł��B</ja>
		/// <en>Gets or sets the account.</en>
		/// </summary>
		public string Account {
			get {
				return _account;
			}
			set {
				_account = value;
			}
		}

		/// <summary>
		/// <ja>�ڑ��̎�ʂł��B</ja>
		/// <en>Gets or sets the connection method.</en>
		/// </summary>
		public override ConnectionMethod Method {
			set {
#if !MACRODOC
				if(value==ConnectionMethod.Telnet)
					throw new ArgumentException(GEnv.Strings.GetString("Mesage.SSHTerminalParam.MethodSetError"));
				_method = value;
#endif
			}
		}

		/// <summary>
		/// <ja>���[�U�F�؂̕��@�ł��B</ja>
		/// <en>Gets or sets the authentication method.</en>
		/// </summary>
		/// <remarks>
		/// <para><ja>�����PublicKey�ɂ����ꍇ�AIdentityFile�v���p�e�B�͔閧���t�@�C�����w���Ă��Ȃ��Ƃ����܂���B</ja>
		/// <en>If the PublicKey is specified, the IdentityFile property must indicate a correct private key file.</en></para>
		/// <para><ja>Password�ɂ����ꍇ�APassphrase�v���p�e�B�̒l���p�X���[�h�Ƃ��Ďg���܂��B</ja>
		/// <en>If the Password is specified, the value of the Passphrase property is used as the login password.</en></para>
		/// <para><ja>�}�N������́AKeyboardInteractive���Z�b�g���Ȃ��ł��������B</ja>
		/// <en>The macro cannot use KeyboardInteractive method.</en></para>
		/// </remarks>
		public AuthType AuthType {
			get {
				return _auth;
			}
			set {
				_auth = value;
			}
		}

		/// <summary>
		/// <ja>�閧���̃t�@�C���ł��B</ja>
		/// <en>Gets or sets the file name of the private key.</en>
		/// </summary>
		/// <remarks>
		/// <ja>�t���p�X�Ŏw�肵�Ă��������B</ja>
		/// <en>The full path is required.</en>
		/// </remarks>
		public string IdentityFile {
			get {
				return _identityfile;
			}
			set {
				_identityfile = value;
			}
		}

		
		/// <summary>
		/// <ja>�p�X���[�h�܂��̓p�X�t���[�Y�ł��B</ja>
		/// <en>Gets or sets the password or the passphrase.</en>
		/// </summary>
		/// <remarks>
		/// <ja>���J���F�؂̏ꍇ�͂��̃v���p�e�B�̒l���p�X�t���[�Y�Ƃ��Ďg���܂��B</ja>
		/// <en>In case of the public key authentication, the value of this property is used as the passphrase of the private key.</en>
		/// <ja>�p�X���[�h�F�؂̏ꍇ�̓p�X���[�h�ɂȂ�܂��B</ja>
		/// <en>In case of the password authentication, it is used as the login password.</en>
		/// </remarks>
		public string Passphrase {
			get {
				return _passphrase;
			}
			set {
				_passphrase = value;
			}
		}
		private static AuthType ParseAuth(string val) {
			if(val=="Password")
				return AuthType.Password;
			if(val=="PublicKey")
				return AuthType.PublicKey;
			if(val=="KeyboardInteractive")
				return AuthType.KeyboardInteractive;

			throw new FormatException(String.Format("{0} is unkown authentication option", val));

		}
#if !MACRODOC
		public override sealed void Export(ConfigNode node) {
			base.Export(node);
			node["account"] = _account;
			node["auth"] = _auth.ToString();
			if(_auth==AuthType.PublicKey)
				node["keyfile"] = _identityfile;
		}

		public override void Import(ConfigNode data) {
			base.Import(data);
			_method = ParseMethod(data["method"]);
			Debug.Assert(this.IsSSH);
			_account = data["account"];
			_auth = ParseAuth(data["auth"]);
			if(_auth==AuthType.PublicKey)
				_identityfile = data["keyfile"];
			_passphrase = data["passphrase"];
		}
		public override string Description {
			get {
				string t;
				if(_account.Length > 0)
					t = _account + "@" + _host;
				else
					t = _host;
				return t;
			}
		}
#endif

	}

	/// <summary>
	/// <ja>�t���[�R���g���[���̐ݒ�</ja>
	/// <en>Specifies the flow control.</en>
	/// </summary>
	[EnumDesc(typeof(FlowControl))]
	public enum FlowControl {
		/// <summary>
		/// <ja>�Ȃ�</ja>
		/// <en>None</en>
		/// </summary>
		[EnumValue(Description="Enum.FlowControl.None")] None,
		/// <summary>
		/// X ON / X OFf
		/// </summary>
		[EnumValue(Description="Enum.FlowControl.Xon_Xoff")] Xon_Xoff,
		/// <summary>
		/// <ja>�n�[�h�E�F�A</ja>
		/// <en>Hardware</en>
		/// </summary>
		[EnumValue(Description="Enum.FlowControl.Hardware")] Hardware
	}

	/// <summary>
	/// <ja>�p���e�B�̐ݒ�</ja>
	/// <en>Specifies the parity.</en>
	/// </summary>
	[EnumDesc(typeof(Parity))]
	public enum Parity {
		/// <summary>
		/// <ja>�Ȃ�</ja>
		/// <en>None</en>
		/// </summary>
		[EnumValue(Description="Enum.Parity.NOPARITY")] NOPARITY = 0,
		/// <summary>
		/// <ja>�</ja>
		/// <en>Odd</en>
		/// </summary>
		[EnumValue(Description="Enum.Parity.ODDPARITY")] ODDPARITY   =        1,
		/// <summary>
		/// <ja>����</ja>
		/// <en>Even</en>
		/// </summary>
		[EnumValue(Description="Enum.Parity.EVENPARITY")] EVENPARITY  =        2
		//MARKPARITY  =        3,
		//SPACEPARITY =        4
	}

	/// <summary>
	/// <ja>�X�g�b�v�r�b�g�̐ݒ�</ja>
	/// <en>Specifies the stop bits.</en>
	/// </summary>
	[EnumDesc(typeof(StopBits))]
	public enum StopBits {
		/// <summary>
		/// <ja>1�r�b�g</ja>
		/// <en>1 bit</en>
		/// </summary>
		[EnumValue(Description="Enum.StopBits.ONESTOPBIT")] ONESTOPBIT  =        0,
		/// <summary>
		/// <ja>1.5�r�b�g</ja>
		/// <en>1.5 bits</en>
		/// </summary>
		[EnumValue(Description="Enum.StopBits.ONE5STOPBITS")] ONE5STOPBITS=        1,
		/// <summary>
		/// <ja>2�r�b�g</ja>
		/// <en>2 bits</en>
		/// </summary>
		[EnumValue(Description="Enum.StopBits.TWOSTOPBITS")] TWOSTOPBITS =        2
	}


	/// <summary>
	/// <ja>�V���A���ڑ��̃p�����[�^�������܂��B</ja>
	/// <en>Implements the parameters of serial connections.</en>
	/// </summary>
	[Serializable()]
	public class SerialTerminalParam : TerminalParam {
		internal int _port;
		internal int _baudRate;
		internal byte _byteSize;  //7,8�̂ǂ��炩
		internal Parity _parity; //Win32�N���X���̒萔�̂����ꂩ
		internal StopBits _stopBits; //Win32�N���X���̒萔�̂����ꂩ
		internal FlowControl _flowControl;
		internal int _transmitDelayPerChar;
		internal int _transmitDelayPerLine;

		/// <summary>
		/// <ja>�f�t�H���g�ݒ�ŏ��������܂��B</ja>
		/// <en>Initializes with default values.</en>
		/// <seealso cref="Poderosa.Macro.ConnectionList.Open"/>
		/// </summary>
		/// <remarks>
		/// <ja>�p�����[�^�͎��̂悤�ɏ���������܂��B</ja>
		/// <en>The parameters are set as following:</en>
		/// <list type="table">
		///   <item><term><ja>�G���R�[�f�B���O</ja><en>Encoding</en></term><description><ja>EUC-JP</ja><en>iso-8859-1</en></description></item>�@
		///   <item><term><ja>���O</ja><en>Log</en></term><description><ja>�擾���Ȃ�</ja><en>None</en></description></item>�@�@�@�@�@�@�@
		///   <item><term><ja>���[�J���G�R�[</ja><en>Local echo</en></term><description><ja>���Ȃ�</ja><en>Don't</en></description></item>�@�@
		///   <item><term><ja>���M�����s</ja><en>New line</en></term><description>CR</description></item>�@�@�@�@
		///   <item><term><ja>�|�[�g</ja><en>Port</en></term><description>COM1</description></item>
		///   <item><term><ja>�{�[���[�g</ja><en>Baud Rate</en></term><description>9600</description></item>
		///   <item><term><ja>�f�[�^</ja><en>Data Bits</en></term><description><ja>8�r�b�g</ja><en>8 bits</en></description></item>
		///   <item><term><ja>�p���e�B</ja><en>Parity</en></term><description><ja>�Ȃ�</ja><en>None</en></description></item>
		///   <item><term><ja>�X�g�b�v�r�b�g</ja><en>Stop Bits</en></term><description><ja>�P�r�b�g</ja><en>1 bit</en></description></item>
		///   <item><term><ja>�t���[�R���g���[��</ja><en>Flow Control</en></term><description><ja>�Ȃ�</ja><en>None</en></description></item>
		/// </list>
		/// <ja>�ڑ����J���ɂ́A<see cref="Poderosa.Macro.ConnectionList.Open"/>���\�b�h�̈����Ƃ���SerialTerminalParam�I�u�W�F�N�g��n���܂��B</ja>
		/// <en>To open a new connection, pass the SerialTerminalParam object to the <see cref="Poderosa.Macro.ConnectionList.Open"/> method.</en>
		/// </remarks>
		public SerialTerminalParam() {
			_port = 1;
			_baudRate = 9600;
			_byteSize = 8;
			_parity = Parity.NOPARITY;
			_stopBits = StopBits.ONESTOPBIT;
			_flowControl = FlowControl.None;
		}
		internal SerialTerminalParam(SerialTerminalParam p) : base(p) {
			_port = p._port;
			_baudRate = p._baudRate;
			_byteSize = p._byteSize;
			_parity = p._parity;
			_stopBits = p._stopBits;
			_flowControl = p._flowControl;
			_transmitDelayPerChar = p._transmitDelayPerChar;
			_transmitDelayPerLine = p._transmitDelayPerLine;
		}

		/// 
		/// 
		/// 
		public override object Clone() {
			return new SerialTerminalParam(this);
		}
		/// 
		/// 
		/// 
		public override bool Equals(object t_) {
			SerialTerminalParam t = t_ as SerialTerminalParam;
			if(t==null) return false;

			return base.Equals(t) && _port==t.Port && _baudRate==t.BaudRate && _byteSize==t.ByteSize && _parity==t.Parity && _stopBits==t.StopBits && _flowControl==t.FlowControl;
		}
		/// 
		/// 
		/// 
		public override int GetHashCode() {
			return base.GetHashCode() + _port.GetHashCode()*2 + _baudRate.GetHashCode()*3 + _byteSize.GetHashCode()*4 + _parity.GetHashCode()*5 + _stopBits.GetHashCode()*6 + _flowControl.GetHashCode()*7;
		}

#if !MACRODOC
		public override sealed void Export(ConfigNode node) {
			node["type"] = "serial";
			node["port"] = _port.ToString();
			node["baud-rate"] = _baudRate.ToString();
			node["byte-size"] = _byteSize.ToString();
			node["parity"] = EnumDescAttribute.For(typeof(Parity)).GetName(_parity);
			node["stop-bits"] = EnumDescAttribute.For(typeof(StopBits)).GetName(_stopBits);
			node["flow-control"] = EnumDescAttribute.For(typeof(FlowControl)).GetName(_flowControl);
			node["delay-per-char"] =  _transmitDelayPerChar.ToString();
			node["delay-per-line"] = _transmitDelayPerLine.ToString();
			base.Export(node);
		}

		public override sealed void Import(ConfigNode data) {
			_port = GUtil.ParseInt(data["port"], 1);
			_baudRate = GUtil.ParseInt(data["baud-rate"], 9600);
			_byteSize = GUtil.ParseByte(data["byte-size"], 8);
			_parity = (Parity)EnumDescAttribute.For(typeof(Parity)).FromName(data["parity"], Parity.NOPARITY);
			_stopBits = (StopBits)EnumDescAttribute.For(typeof(StopBits)).FromName(data["stop-bits"], StopBits.ONESTOPBIT);
			_flowControl = (FlowControl)EnumDescAttribute.For(typeof(FlowControl)).FromName(data["flow-control"], FlowControl.None);
			_transmitDelayPerChar = GUtil.ParseInt(data["delay-per-char"], 0);
			_transmitDelayPerLine = GUtil.ParseInt(data["delay-per-line"], 0);
			base.Import(data);
		}
		public override string ShortDescription {
			get {
				return "COM"+_port;
			}
		}
		public override string Description {
			get {
				string t = "COM" + _port;
				return t;
			}
		}
		public override string MethodName {
			get {
				return "serial";
			}
		}
#endif
		/// <summary>
		/// <ja>�|�[�g�ł��B</ja>
		/// <en>Gets or sets the port.</en>
		/// </summary>
		/// <remarks>
		/// <ja>�P�Ȃ�COM1�A10�Ȃ�COM10�ɂȂ�܂��B</ja>
		/// <en>1 means COM1, and 10 means COM10.</en>
		/// </remarks>
		public int Port {
			get {
				return _port;
			}
			set {
				_port = value;
			}
		}

		/// <summary>
		/// <ja>�{�[���[�g�ł��B</ja>
		/// <en>Gets or sets the baud rate.</en>
		/// </summary>
		public int BaudRate {
			get {
				return _baudRate;
			}
			set {
				_baudRate = value;
			}
		}
		/// <summary>
		/// <ja>�f�[�^�̃r�b�g���ł��B</ja>
		/// <en>Gets or sets the bit count of the data.</en>
		/// </summary>
		/// <remarks>
		/// <ja>�V���W�łȂ��Ƃ����܂���B</ja>
		/// <en>The value must be 7 or 8.</en>
		/// </remarks>
		public byte ByteSize {
			get {
				return _byteSize;
			}
			set {
				_byteSize = value;
			}
		}
		/// <summary>
		/// <ja>�p���e�B�ł��B</ja>
		/// <en>Gets or sets the parity.</en>
		/// </summary>
		public Parity Parity {
			get {
				return _parity;
			}
			set {
				_parity = value;
			}
		}
		/// <summary>
		/// <ja>�X�g�b�v�r�b�g�ł��B</ja>
		/// <en>Gets or sets the stop bit.</en>
		/// </summary>
		public StopBits StopBits {
			get {
				return _stopBits;
			}
			set {
				_stopBits = value;
			}
		}
		/// <summary>
		/// <ja>�t���[�R���g���[���ł��B</ja>
		/// <en>Gets or sets the flow control.</en>
		/// </summary>
		public FlowControl FlowControl {
			get {
				return _flowControl;
			}
			set {
				_flowControl = value;
			}
		}

		/// <summary>
		/// <ja>����������̃f�B���C(�~���b�P��)�ł��B</ja>
		/// <en>Gets or sets the delay time per a character in milliseconds.</en>
		/// </summary>
		public int TransmitDelayPerChar {
			get {
				return _transmitDelayPerChar;
			}
			set {
#if !MACRODOC
				if(value<0) throw new ArgumentException(GEnv.Strings.GetString("Message.SerialPTerminalParam.TransmitDelayRange"));
				_transmitDelayPerChar = value;
#endif
			}
		}
		/// <summary>
		/// <ja>�s������̃f�B���C(�~���b�P��)�ł��B</ja>
		/// <en>Gets or sets the delay time per a line in milliseconds.</en>
		/// </summary>
		public int TransmitDelayPerLine {
			get {
				return _transmitDelayPerLine;
			}
			set {
#if !MACRODOC
				if(value<0) throw new ArgumentException(GEnv.Strings.GetString("Message.SerialPTerminalParam.TransmitDelayRange"));
				_transmitDelayPerLine = value;
#endif
			}
		}
	}

	/// <summary>
	/// <ja>Cygwin�܂���Services for Unix�ɐڑ����邽�߂�TerminalParam�ł��B</ja>
	/// <en>Implements the parameters to connect a cygwin shell or a Services for Unix shell.</en>
	/// </summary>
	public abstract class LocalShellTerminalParam : TerminalParam {

		protected string _home;
		protected string _shell;

		/// <summary>
		/// <ja>�W���I�Ȓl�ŏ��������܂��B</ja>
		/// <en>Initializes with default values.</en>
		/// </summary>
		public LocalShellTerminalParam() {
#if !MACRODOC
			_terminalType = TerminalType.VT100;
			_transmitnl = NewLine.CR;
			_encoding = LocalShellUtil.DefaultEncoding;
#endif
		}
		internal LocalShellTerminalParam(LocalShellTerminalParam p) : base(p) {
			_home = p._home;
			_shell = p._shell;
		}

		/// <summary>
		/// <ja>Cygwin��̃V�F���ɂȂ����Ƃ���HOME���ϐ��̒l�ł��B�f�t�H���g�l�� <c>/home/(Windows�̃A�J�E���g��)</c> �ł��B</ja>
		/// <en>Gets or sets the initial value of the HOME environment variable. The default value is <c>/home/(account name on Windows)</c>.</en>
		/// </summary>
		public string Home {
			get {
				return _home;
			}
			set {
				_home = value;
			}
		}
		/// <summary>
		/// <ja>�N������Cygwin�̃V�F���ւ̃p�X�ł��B�f�t�H���g�l�� <c>/bin/bash</c> �ł��B</ja>
		/// <en>Gets or sets the path of the shell. The defualt value is <c>/bin/bash</c>.</en>
		/// </summary>
		public string Shell {
			get {
				return _shell;
			}
			set {
				_shell = value;
			}
		}


#if !MACRODOC
		public override void Import(ConfigNode data) {
			_home = data["home"];
			_shell = data["shell"];
			base.Import(data);
		}
		public override string ShortDescription {
			get {
				int n = _shell.IndexOf(' ');
				return n==-1? _shell : _shell.Substring(0, n);
			}
		}

#endif
	}

	/// <summary>
	/// <ja>Cygwin�ɐڑ����邽�߂�TerminalParam�ł��B</ja>
	/// <en>Implements the parameters to connect a cygwin shell.</en>
	/// </summary>
	public class CygwinTerminalParam : LocalShellTerminalParam {
		public CygwinTerminalParam() {
#if !MACRODOC
			_home   = CygwinUtil.DefaultHome;
			_shell  = CygwinUtil.DefaultShell;
#endif
		}

		public override object Clone() {
			CygwinTerminalParam p = new CygwinTerminalParam();
			p.Home = _home;
			p.Shell = _shell;
			return p;
		}
		public override bool Equals(object t_) {
			CygwinTerminalParam t = t_ as CygwinTerminalParam;
			if(t==null) return false;

			return base.Equals(t) && _home==t._home && _shell==t._shell;
		}
		public override int GetHashCode() {
			return base.GetHashCode() + _home.GetHashCode()*3 + _shell.GetHashCode()*7;
		}

#if !MACRODOC
		public override sealed void Export(ConfigNode node) {
			node["type"] = "cygwin";
			node["home"] = _home;
			node["shell"] = _shell;
			base.Export(node);
		}

		public  override string Description {
			get {
				return ShortDescription + "(cygwin)";
			}
		}

		public override string MethodName {
			get {
				return "cygwin";
			}
		}
#endif
	}

	/// <summary>
	/// <ja>Services for Unix�ɐڑ����邽�߂�TerminalParam�ł��B</ja>
	/// <en>Implements the parameters to connect a shell of Servies for Unix.</en>
	/// </summary>
	public class SFUTerminalParam : LocalShellTerminalParam {
		public SFUTerminalParam() {
#if !MACRODOC
			_home   = SFUUtil.DefaultHome;
			_shell  = SFUUtil.DefaultShell;
#endif
		}

		public override object Clone() {
			SFUTerminalParam p = new SFUTerminalParam();
			p.Home = _home;
			p.Shell = _shell;
			return p;
		}
		public override bool Equals(object t_) {
			SFUTerminalParam t = t_ as SFUTerminalParam;
			if(t==null) return false;

			return base.Equals(t) && _home==t._home && _shell==t._shell;
		}
		public override int GetHashCode() {
			return base.GetHashCode() + _home.GetHashCode()*3 + _shell.GetHashCode()*7;
		}

#if !MACRODOC
		public override sealed void Export(ConfigNode node) {
			node["type"] = "sfu";
			node["home"] = _home;
			node["shell"] = _shell;
			base.Export(node);
		}

		public  override string Description {
			get {
				return ShortDescription + "(SFU)";
			}
		}

		public override string MethodName {
			get {
				return "SFU";
			}
		}
#endif
	}
}
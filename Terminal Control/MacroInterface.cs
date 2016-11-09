/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: MacroInterface.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Collections;

using Poderosa.ConnectionParam;
#if !MACRODOC
using Poderosa.Config;
#endif

using Poderosa.Toolkit;

namespace Poderosa.Macro {

	/// <summary>
	/// <ja>�}�N���@�\�̃��[�g�ɂȂ�N���X�ł��B</ja>
	/// <en>This class is the root of the macro functionality.</en>
	/// </summary>
	/// <remarks>
	/// <ja>�}�N�����炱�̃N���X�̃C���X�^���X���쐬���āA�e�v���p�e�B�E���\�b�h�ɃA�N�Z�X���Ă��������B</ja>
	/// <en>Use properties and methods after the macro creates an instance of this class. </en>
	/// </remarks>
	public sealed class Environment : MarshalByRefObject {

		/// <summary>
		/// <ja><see cref="ConnectionList"/>�I�u�W�F�N�g���擾���܂��B</ja>
		/// <en>Gets the <see cref="ConnectionList"/> object.</en>
		/// </summary>
		public ConnectionList Connections {
			get {
				return _connectionList;
			}
		}

		/// <summary>
		/// <ja><see cref="Util"/>�I�u�W�F�N�g���擾���܂��B</ja>
		/// <en>Gets the <see cref="Util"/> object.</en>
		/// </summary>
		public Util Util {
			get {
				return _util;
			}
		}

		/// <summary>
		/// <ja><see cref="Frame"/>�I�u�W�F�N�g���擾���܂��B</ja>
		/// <en>Gets the <see cref="Frame"/> object.</en>
		/// </summary>
		public Frame Frame {
			get {
				return _frame;
			}
		}

		/// <summary>
		/// <ja>�}�N���̃f�o�b�O��⏕���邽�߂�<see cref="DebugService"/>�I�u�W�F�N�g���擾���܂��B</ja>
		/// <en>Gets the <see cref="DebugService"/> object for debugging the macro.</en>
		/// </summary>
		public DebugService Debug {
			get {
				return _debugService;
			}
		}

		/// <summary>
		/// <ja>Poderosa�̃C���X�g�[�����ꂽ�f�B���N�g�������擾���܂��B�����ɂ� \ �����Ă��܂��B</ja>
		/// <en>Gets the directory the Poderosa is installed. The tail of the string is a '\'.</en>
		/// </summary>
		public string InstalledDir {
			get {
				return _guevaraDir;
			}
		}

		/// <summary>
		/// <ja>Poderosa���ϐ����擾���܂��B</ja>
		/// <en>Gets the Poderosa environment variable.</en>
		/// </summary>
		/// <remarks>
		/// <ja>
		/// <para>�@Poderosa���ϐ��́A���[�U�̊��Ɉˑ����镔�����}�N�����番�����邽�߂ɗp�ӂ��ꂽ�@�\�ł��B
		/// ���ϐ��̒�`�́A���j���[����c�[�� - �}�N�� - ���ݒ��I�сA���̒��Ŋ��ϐ��{�^���ɂ���Ċm�F�ƕҏW���ł��܂��B</para>
		/// <para>�@���Ƃ��΁A�e�L�X�g�G�f�B�^�̃p�X�������ɓo�^�����Ă����āA�}�N������N�����邱�Ƃ��ł��܂��B</para>
		/// <para>�@�Ȃ��APoderosa���ϐ���OS�̊��ϐ��Ƃ͊֌W����܂���B</para>
		/// </ja>
		/// <en>
		/// <para> The Poderosa environment variable feature intends to separate configurations which depend on the environment of users from the macro.</para>
		/// <para> The user can edit Poderosa envrionment variables from the Tools - Macro - Configure Environment menu. For example, the user registers the text editor as an environment variable and the macro launchs the editor. </para>
		/// <para> Note that the environment variable has no relation to the environment variables of Windows. </para>
		/// </en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var filename = "C:\temp\...
		/// env.Util.Exec(env.GetVariable("tools.texteditor") + " " + filename);
		/// </code>
		/// </example>
		/// <param name="key"><ja>���ϐ��̖��O</ja><en>the name of varialbe</en></param>
		/// <returns><ja>���ϐ�����`����Ă���΂��̒l�A��`����Ă��Ȃ����null</ja><en>If the variable is defined, returns the value. Otherwise, returns null.</en></returns>
		public string GetVariable(string key) {
#if MACRODOC
			return null;
#else
			return GApp.MacroManager.GetVariable(key, null);
#endif
		}
		/// <summary>
		/// <ja>Poderosa���ϐ����擾���܂��B</ja>
		/// <en>Gets a Poderosa environment variable.</en>
		/// </summary>
		/// <param name="key"><ja>���ϐ��̖��O</ja><en>the name of varialbe</en></param>
		/// <param name="def"><ja>������Ȃ������Ƃ��ɕԂ��f�t�H���g�l</ja><en>the default value in case that the key is not found</en></param>
		/// <returns><ja>���ϐ�����`����Ă���΂��̒l�A��`����Ă��Ȃ����def�̒l</ja><en>If the variable is defined, returns the value. Otherwise, returns the value of def.</en></returns>
		public string GetVariable(string key, string def) {
#if MACRODOC
			return null;
#else
			return GApp.MacroManager.GetVariable(key, def);
#endif
		}

		private static Version _version;
		private static ConnectionList _connectionList;
		private static Util _util;
		private static DebugService _debugService; 
		private static string _guevaraDir;
		private static Frame _frame;

		internal static void Init(ConnectionList cl, Util ui, Frame fr, DebugService ds) {
			_version = new Version(1,0);
			_connectionList = cl;
			_util = ui;
			_frame = fr;
			_debugService = ds;
			_guevaraDir = AppDomain.CurrentDomain.BaseDirectory;
		}
	}

	/// <summary>
	/// <ja><see cref="Connection"/>�I�u�W�F�N�g�̃R���N�V�����ł��B</ja>
	/// <en>A collection of <see cref="Connection"/> objects.</en>
	/// </summary>
	public abstract class ConnectionList : MarshalByRefObject, IEnumerable {
		/// <summary>
		/// <ja>�R�l�N�V�����̐��ł��B</ja>
		/// <en>Gets the number of connections.</en>
		/// </summary>
		public abstract int Count { get; }

		/// <summary>
		/// <ja><see cref="Connection"/>�I�u�W�F�N�g��񋓂��܂��B</ja>
		/// <en>Enumerates each <see cref="Connection"/> objects.</en>
		/// </summary>
		public abstract IEnumerator GetEnumerator();

		/// <summary>
		/// <ja>�A�v���P�[�V�����ŃA�N�e�B�u�ɂȂ��Ă���ڑ���Ԃ��܂��B</ja>
		/// <en>Returns the active connection of Poderosa.</en>
		/// <ja>�A�N�e�B�u�Ȑڑ����Ȃ��Ƃ���null��Ԃ��܂��B</ja>
		/// <en>If there are no active connections, returns null.</en>
		/// </summary>
		public abstract Connection ActiveConnection { get; }

		/// <summary>
		/// <ja>�V�����ڑ����J���܂��B</ja>
		/// <en>Opens a new connection.</en>
		/// </summary>
		/// <remarks>
		/// <ja>���s�����Ƃ��̓��b�Z�[�W�{�b�N�X�Œʒm���������null���Ԃ�܂��B</ja>
		/// <en>If the connection fails, Poderosa shows an error message box and returns null to the macro.</en>
		/// </remarks>
		/// <seealso cref="TerminalParam"/>
		/// <seealso cref="TCPTerminalParam"/>
		/// <seealso cref="TelnetTerminalParam"/>
		/// <seealso cref="SSHTerminalParam"/>
		/// <seealso cref="SerialTerminalParam"/>
		/// <param name="param"><ja>�ڑ��ɕK�v�ȃp�����[�^�����^����<see cref="TerminalParam"/>�I�u�W�F�N�g</ja><en>The <see cref="TerminalParam"/> object that contains parameters for the connection.</en></param>
		/// <returns><ja>�V�����J���ꂽ<see cref="Connection"/>�I�u�W�F�N�g</ja><en>A <see cref="Connection"/> object that describes the new connection.</en></returns>
		public abstract Connection Open(TerminalParam param);

		/// <summary>
		/// <ja>�V���[�g�J�b�g�t�@�C�����J���܂�</ja>
		/// <en>Opens a shortcut file</en>
		/// </summary>
		/// <remarks>
		/// <ja>�ڑ������s������A���[�U���L�����Z�������null���Ԃ�܂��B</ja>
		/// <en>If the connection is failed or the user cancelled, this method returns null.</en>
		/// </remarks>
		/// <param name="filename"><ja>�ڑ��ɕK�v�ȃp�����[�^�����^�����V���[�g�J�b�g�t�@�C����</ja><en>A shortcut file that contains parameters for the connection.</en></param>
		/// <returns><ja>�V�����J���ꂽ<see cref="Connection"/>�I�u�W�F�N�g</ja><en>A <see cref="Connection"/> object that describes the new connection.</en></returns>
		public abstract Connection OpenShortcutFile(string filename);

	}
	

	/// <summary>
	/// <ja>�P�{�̐ڑ���\���܂��B</ja>
	/// <en>Describes a connection.</en>
	/// </summary>
	public abstract class Connection  : MarshalByRefObject {
		/// <summary>
		/// <ja>���̐ڑ��ɐݒ肳�ꂽ��ʂ̕��𕶎��P�ʂŎ擾���܂��B</ja>
		/// <en>Gets the width of the console in characters.</en>
		/// </summary>
		public abstract int TerminalWidth { get; }
		/// <summary>
		/// <ja>���̐ڑ��ɐݒ肳�ꂽ��ʂ̍����𕶎��P�ʂŎ擾���܂��B</ja>
		/// <en>Gets the height of the console in characters.</en>
		/// </summary>
		public abstract int TerminalHeight { get; }

		/// <summary>
		/// <ja>���̐ڑ����A�N�e�B�u�ɂ��A�őO�ʂɎ����Ă����܂��B</ja>
		/// <en>Activates this connection and brings to the front of application.</en>
		/// </summary>
		public abstract void Activate();

#if !MACRODOC
		/// <summary>
		/// ���̐ڑ����A�N�e�B�u�ɂ��A�őO�ʂɎ����Ă����܂��B�����Ńy�C���̈ʒu���w��ł��܂��B
		/// </summary>
		/// <param name="pos">�y�C���̈ʒu���w�肵�܂��B���̈����̓t���[�����㉺�܂��͍��E�ɕ�������Ă���Ƃ��݈̂Ӗ��������܂��B</param>
		public abstract void Activate(PanePosition pos);
#endif
		/// <summary>
		/// <ja>�ڑ�����܂��B</ja>
		/// <en>Closes this connection.</en>
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// <ja>�f�[�^�𑗐M���܂��B</ja>
		/// <en>Transmits data.</en>
		/// <ja>������͂��̐ڑ��ɐݒ肳�ꂽ�G���R�[�f�B���O�ɏ]���ăo�C�g��ɃG���R�[�h����܂��B</ja>
		/// <en>The string is encoded in accord with the encoding of this connection.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var connection = env.Connections.ActiveConnection;
		/// connection.Transmit("ls");
		/// connection.TransmitLn("-la");
		/// </code>
		/// </example>
		/// <param name="data">���M������������</param>
		public abstract void Transmit(string data);

		/// <summary>
		/// <ja>�f�[�^�ɂÂ��ĉ��s�𑗐M���܂��B</ja>
		/// <en>Transmits data followed by new line character.</en>
		/// </summary>
		/// <remarks>
		/// <ja>������͂��̐ڑ��ɐݒ肳�ꂽ�G���R�[�f�B���O�ɏ]���ăo�C�g��ɃG���R�[�h����܂��B</ja>
		/// <en>The string is encoded in accord with the encoding of this connection.</en>
		/// <ja>���̃��\�b�h�͕�����̓��͂ɂÂ���Enter�L�[�������̂Ɠ������ʂ�����܂��B</ja>
		/// <en>This method has the same effect as pressing the Enter key following the input of the string.</en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// 
		/// var connection = Environment.Connections.ActiveConnection;
		/// connection.Transmit("ls");
		/// connection.TransmitLn("-la");
		/// </code>
		/// </example>
		/// <param name="data"><ja>���̓f�[�^</ja><en>The input data</en></param>
		public abstract void TransmitLn(string data);

		/// <summary>
		/// <ja>���̐ڑ��ɑ΂���Break�M���𑗂�܂��B</ja>
		/// <en>Send a break signal to this connection.</en>
		/// </summary>
		/// <remarks>
		/// <ja>SSH1�ł�Break�M���𑗂邱�Ƃ͂ł��܂���B</ja>
		/// <en>SSH1 does not support the break signal.</en>
		/// </remarks>
		public abstract void SendBreak();

		/// <summary>
		/// <ja>�P�s�̃f�[�^����M���܂��B</ja>
		/// <en>Receives a line from the connection.</en>
		/// </summary>
		/// <remarks>
		/// <para><ja>�@�z�X�g����f�[�^����������������s���I����Ă��Ȃ��Ƃ��́A�P�s�̏I�����m�F�ł���܂Ń��\�b�h�̓u���b�N���܂��B</ja>
		/// <en> When no data is available or the new line characters are not sent, the execution of this method is blocked.</en></para>
		/// <para><ja>�@���Ƀv�����v�g������͉��s���܂܂Ȃ��̂ŁA�v�����v�g��҂��߂ɂ��̃��\�b�h���g��Ȃ��悤�ɂ��Ă��������B�v�����v�g�̔��������悤�ȏꍇ�ɂ͂�����<see cref="ReceiveData"/>���\�b�h���g���Ă��������B</ja>
		/// <en> Especially note that this method could not be used to wait a prompt string since it does not contain new line characters. To wait a prompt, use the <see cref="ReceiveData"/> method instead of ReceiveLine method.</en>
		/// </para>
		/// <para><ja>�@�܂��A�z�X�g���痈��f�[�^�̂����ACR��NUL�͖�������܂��B</ja>
		/// <en> Additionally, CR and NUL are ignored in the data from the host.</en></para>
		/// <seealso cref="ReceiveData"/>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// import System.IO;
		/// var env = new Environment();
		/// 
		/// var output = new StreamWriter("...
		/// var connection = env.Connections.ActiveConnection;
		/// var line = connection.ReceiveLine();
		/// while(line!="end") { //wait for "end"
		///   output.WriteLine(line);
		///   line = connection.ReceiveLine();
		/// }
		/// output.Close();
		/// 
		/// </code>
		/// </example>
		/// <returns><ja>��M����������ł��B���s�����͊܂݂܂���B</ja><en>The received line without new line characters.</en></returns>
		public abstract string ReceiveLine();

		/// <summary>
		/// <ja>�f�[�^����M���܂��B</ja>
		/// <en>Receives data from the connection.</en>
		/// </summary>
		/// <remarks>
		/// <para><ja>�@�z�X�g����f�[�^���������̂Ƃ��́A��������܂Ń}�N���̎��s�̓u���b�N���܂��B</ja>
		/// <en>  When no data is available, the execution of this method is blocked.</en>
		/// </para>
		/// <para><ja>�@�f�[�^�������ς݂̂Ƃ��́A�O���ReceiveData�̌Ăяo���ȍ~�ɗ����f�[�^���ꊇ���Ď擾���܂��B�s�ɐ؂蕪�����Ƃ̓}�N�����ōs���K�v������܂����A���s�ŏI����Ă��Ȃ��f�[�^���擾�ł��闘�_������܂��B <see cref="ReceiveLine"/>�Ǝg�������Ă��������B</ja>
		/// <en> This method returns the whole data from the previous call of the ReceiveData method. Though this method can obtain the data even if it does not contain new line characters, the split into lines is responsible for the macro. Please compare to the <see cref="ReceiveLine"/> method.</en>
		/// </para>
		/// <para><ja>�@�܂��A�z�X�g���痈��f�[�^�̂����ACR��NUL�͖�������܂��B���s��LF�ɂ���Ĕ��ʂ��܂��B</ja>
		/// <en> CR and NUL are ignored in the data from the host. The line breaks are determined by LF.</en></para>
		/// <seealso cref="ReceiveLine"/>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// import System.IO;
		/// var env = new Environment();
		/// 
		/// var connection = env.Connections.ActiveConnection;
		/// var data = connection.ReceiveData();
		/// if(data.EndsWith("login: ") {
		///	  ...
		/// </code>
		/// </example>
		/// <returns><ja>��M����������ł��B</ja><en>The received data.</en></returns>
		public abstract string ReceiveData();

		/// <summary>
		/// <ja>���O�ɃR�����g�������܂��B�ڑ������O�����悤�ɐݒ肳��Ă��Ȃ��ꍇ�͉������܂���B</ja>
		/// <en>Writes a comment to the log. If the connection is not set to record the log, this method does nothing.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var connection = env.Connections.ActiveConnection;
		/// connection.WriteComment("starting macro...");
		/// </code>
		/// </example>
		/// <param name="comment"><ja>�R�����g������</ja><en>The comment string</en></param>
		public abstract void WriteComment(string comment);
	}

	/// <summary>
	/// <ja>�A�v���P�[�V�����̊O�ς𐧌䂷�邽�߂̃I�u�W�F�N�g�ł��B</ja>
	/// <en>Implements the appearances of the application.</en>
	/// </summary>
	public abstract class Frame : MarshalByRefObject {
#if !MACRODOC
		/// <summary>
		/// <ja>�����������擾�E�ݒ肵�܂��B</ja>
		/// <en>Gets or sets the division style.</en>
		/// </summary>
		public abstract GFrameStyle FrameStyle { get; set; }
#endif
		/// <summary>
		/// <ja>�t���[�����V���O���X�^�C���ɃZ�b�g����ƂƂ��ɁA�[�������T�C�Y���܂��B</ja>
		/// <en>Changes the frame style to be single and resizes the terminal.</en>
		/// </summary>
		/// <param name="width"><ja>�^�[�~�i���̕��𕶎��P�ʂŎw�肵�܂��B</ja><en>The terminal width in characters</en></param>
		/// <param name="height"><ja>�^�[�~�i���̍����𕶎��P�ʂŎw�肵�܂��B</ja><en>The terminal height in characters</en></param>
		public abstract void SetStyleS(int width, int height);

		/// <summary>
		/// <ja>�t���[�����㉺�Q�����X�^�C���ɃZ�b�g����ƂƂ��ɁA�[�������T�C�Y���܂��B</ja>
		/// <en>Changes the frame style to be divided horizontally and resizes the terminal.</en>
		/// </summary>
		/// <param name="width"><ja>�^�[�~�i���̕��𕶎��P�ʂŎw�肵�܂��B����͏㉺�Q�̃y�C���ŋ��L����܂��B</ja><en>The terminal width in characters. This is common to the two panes.</en></param>
		/// <param name="height1"><ja>��̃y�C���̍����𕶎��P�ʂŎw�肵�܂��B</ja><en>The terminal height of the upper pane in characters.</en></param>
		/// <param name="height2"><ja>���̃y�C���̍����𕶎��P�ʂŎw�肵�܂��B</ja><en>The terminal height of the lower pane in characters.</en></param>
		public abstract void SetStyleH(int width, int height1, int height2);

		/// <summary>
		/// <ja>�t���[�������E�Q�����X�^�C���ɃZ�b�g����ƂƂ��ɁA�t���[�������T�C�Y���܂��B</ja>
		/// <en>Changes the frame style to be divided vertically and resizes the terminal.</en>
		/// </summary>
		/// <param name="width1"><ja>���̃y�C���̕��𕶎��P�ʂŎw�肵�܂��B</ja><en>The terminal width of the left pane in characters.</en></param>
		/// <param name="width2"><ja>�E�̃y�C���̕��𕶎��P�ʂŎw�肵�܂��B</ja><en>The terminal width of the right pane in characters.</en></param>
		/// <param name="height"><ja>�^�[�~�i���̍����𕶎��P�ʂŎw�肵�܂��B����͍��E�Q�̃y�C���ŋ��L����܂��B</ja><en>The terminal height in characters. This is common to the two panes.</en></param>
		public abstract void SetStyleV(int width1, int width2, int height);
	}


	/// <summary>
	/// <ja>�}�N������Ăяo�����߂́A��r�I�悭�g�������ȋ@�\�����^�����I�u�W�F�N�g�ł��B</ja>
	/// <en>Implements several utility functions for macros.</en>
	/// </summary>
	public abstract class Util : MarshalByRefObject {
		/// <summary>
		/// <ja>���b�Z�[�W�{�b�N�X��\�����܂��B</ja>
		/// <en>Shows a message box.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// env.Util.MessageBox(String.Format("This file is {0}", env.MacroFileName));
		/// </code>
		/// </example>
		/// <param name="msg"><ja>�\�����������b�Z�[�W</ja><en>The message to be shown.</en></param>
		public abstract void MessageBox(string msg);

		/// <summary>
		/// <ja>�t�@�C�����J���A�������Ƃ�������������܂��B</ja>
		/// <en>Performs actions to the file such as open or print.</en>
		/// </summary>
		/// <remarks>
		/// <ja>
		/// �����オ��A�v���P�[�V�����̓t�@�C���̊g���q��verb�����ɂ���Č��܂�܂��B
		/// ���Ƃ��Ίg���q��txt�ł���΃e�L�X�g�G�f�B�^���N�����܂��B
		/// �����I�ɂ́A���̃��\�b�h��Win32��ShellExecute API���Ăяo���܂��B
		/// </ja>
		/// <en>
		/// The application is decided by the extension and the verb argument.
		/// For exapmle, a text editor starts if the extension is .txt.
		/// This method calls the ShellExecute API of Win32 internally.
		/// </en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// import System.IO;
		/// var env = new Environment();
		/// 
		/// string filename = Path.GetTempFileName() + ".txt";
		/// ... (write some text to this file)
		/// 
		/// env.Util.ShellExecute("open", filename);
		/// </code>
		/// </example>
		/// <param name="verb"><ja>�t�@�C���ɑ΂��čs������ł��B"open","print"�Ȃǂł��B</ja><en>The action to the file such as "open" or "print".</en></param>
		/// <param name="filename"><ja>�J���t�@�C�����t���p�X�Ŏw�肵�܂��B</ja><en>The full path of the file name.</en></param>
		public abstract void ShellExecute(string verb, string filename);

		/// <summary>
		/// <ja>�C�ӂ̃A�v���P�[�V�������N�����܂��B</ja>
		/// <en>Starts other applications.</en>
		/// </summary>
		/// <remarks>
		/// <ja>�����I�ɂ́A���̃��\�b�h��Win32��WinExec API���Ăяo���܂��B</ja>
		/// <en>This method calls WinExec API of Win32 internally.</en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// env.Util.Exec("notepad.exe");
		/// </code>
		/// </example>
		/// <param name="command"><ja>�N���������A�v���P�[�V�����̖��O�ł��B�K�v�ł���Έ��������邱�Ƃ��ł��܂��B</ja><en>The name of the application to be started. Arguments are allowed if necessary.</en></param>
		public abstract void Exec(string command);
	}

	/// <summary>
	/// <ja>�}�N���̃e�X�g�ƃf�o�b�O�ɕK�v�ȋ@�\��񋟂��܂��B</ja>
	/// <en>Implements features for testing and debugging the macro.</en>
	/// </summary>
	/// <remarks>
	/// <para><ja>�@�}�N���̃v���p�e�B��ʂɂ����āA�u�g���[�X�E�B���h�E��\������v�I�v�V���������Ă����ƁA���̃}�N�����N������Ƃ��Ƀg���[�X�E�B���h�E���g����悤�ɂȂ�܂��B</ja>
	/// <en> The macro trace window is displayed when the "shows trace window" option is checked in the dialog box of the macro property.</en>
	/// </para>
	/// </remarks>
	public abstract class DebugService : MarshalByRefObject {
		/// <summary>
		/// <ja>�g���[�X�E�B���h�E�ɂP�s�̃f�[�^��\�����܂��B</ja>
		/// <en>Outputs a line to the trace window.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var i = 123;
		/// env.Debug.Trace(String.Format("i={0}", i));
		/// </code>
		/// </example>
		/// <param name="msg"><ja>�\���������f�[�^</ja><en>The data to be displayed.</en></param>
		public abstract void Trace(string msg);

		/// <summary>
		/// <ja>�Ăяo�������_�ł̃X�^�b�N�g���[�X��\�����܂��B</ja>
		/// <en>Outputs the stack trace to the trace window.</en>
		/// </summary>
		public abstract void PrintStackTrace();
	}

}

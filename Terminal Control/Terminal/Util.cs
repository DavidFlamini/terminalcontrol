/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: Util.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Threading;

using Poderosa.Config;
using Granados.SSHC;
using Granados.PKI;

namespace Poderosa
{
	public class GUtil {
		public static bool ParseBool(string value, bool defaultvalue) {
			try {
				return Boolean.Parse(value);
			}
			catch(Exception) {
				return defaultvalue;
			}
		}
		public static byte ParseByte(string value, byte defaultvalue) {
			try {
				return Byte.Parse(value);
			}
			catch(Exception) {
				return defaultvalue;
			}
		}
		public static int ParseInt(string value, int defaultvalue) {
			try {
				return Int32.Parse(value);
			}
			catch(Exception) {
				return defaultvalue;
			}
		}
		public static float ParseFloat(string value, float defaultvalue) {
			try {
				return Single.Parse(value);
			}
			catch(Exception) {
				return defaultvalue;
			}
		}
		public static int ParseHexInt(string value, int defaultvalue) {
			try {
				return Int32.Parse(value, System.Globalization.NumberStyles.HexNumber);
			}
			catch(Exception) {
				return defaultvalue;
			}
		}
		public static Color ParseColor(string t, Color defaultvalue) {
			if(t==null || t.Length==0)
				return defaultvalue;
			else {
				if(t.Length==8) { //16�i�ŕۑ�����Ă��邱�Ƃ�����B���]�̍�ł��̂悤��
					try {
						int v = Int32.Parse(t, System.Globalization.NumberStyles.HexNumber);
						return Color.FromArgb(v);
					}
					catch(Exception) {
					}
				}
				return Color.FromName(t);
			}
		}
		public static ValueType ParseEnum(Type enumtype, string t, ValueType defaultvalue) {
			try {
				if(t==null || t.Length==0)
					return (ValueType)Enum.ToObject(enumtype, (int)defaultvalue);
				else
					return (ValueType)Enum.Parse(enumtype, t, false);
			}
			catch(Exception) {
				return (ValueType)Enum.ToObject(enumtype, (int)defaultvalue);
			}
		}
		public static ValueType ParseMultipleEnum(Type enumtype, string t, ValueType defaultvalue) {
			try {
				int r = 0;
				foreach(string a in t.Split(','))
					r |= (int)Enum.Parse(enumtype, a, false);
				return r;
			}
			catch(FormatException) {
				return defaultvalue;
			}
		}

		public static Language CurrentLanguage {
			get {
				return CultureInfo.CurrentUICulture.Name.StartsWith("ja")? Language.Japanese : Language.English;
			}
		}
	
		public static Font CreateFont(string name, float size) {
			bool flag = false;
			do {
				try {
					return new Font(name, size);
				}
				catch(ArithmeticException) {
					//JSPager�̌��őΉ��Bmsvcr71�����[�h�ł��Ȃ��������邩������Ȃ��̂ŗ�O��������Ă͂��߂ČĂԂ悤�ɂ���
					if(!flag) {
						Win32.ClearFPUOverflowFlag();
						flag = true;
					}
					else //���Ɏ��s���Ă��߂Ȃ炠����߂�
						throw;
				}
			} while(true);
		}


		public static string FileToDir(string filename) {
			int n = filename.LastIndexOf('\\');
			if(n==-1) throw new FormatException("filename does not include \\");

			return filename.Substring(0, n);
		}
		public static void Warning(IWin32Window owner, string msg, string caption) {
			MessageBox.Show(owner, msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		public static void Warning(IWin32Window owner, string msg, MessageBoxIcon icon) {
			MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.OK, icon);
		}
		public static void Warning(IWin32Window owner, string msg) {
			MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		public static DialogResult AskUserYesNo(IWin32Window owner, string msg) {
			return MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}
		public static DialogResult AskUserYesNo(IWin32Window owner, string msg, MessageBoxIcon icon) {
			return MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.YesNo, icon);
		}
		public static void ReportCriticalError(Exception ex) {
			InternalReportCriticalError(" [regular] ", ex);
		}
		public static void ReportThreadException(Exception ex) {
			InternalReportCriticalError(" [ThreadException] ", ex);
		}
		private static void InternalReportCriticalError(string remark, Exception ex) {
			Debug.WriteLine(remark);
			Debug.WriteLine(ex.Message);
			Debug.WriteLine(ex.StackTrace);

			//�G���[�t�@�C���ɒǋL
			string dir = null;
			StreamWriter sw = GetDebugLog(ref dir);
			sw.WriteLine(DateTime.Now.ToString() + remark + ex.Message);
			sw.WriteLine(ex.StackTrace);
			//inner exception������
			Exception i = ex.InnerException;
			while(i!=null) {
				sw.WriteLine("[inner] " + i.Message);
				sw.WriteLine(i.StackTrace);
				i = i.InnerException;
			}
			sw.Close();

			//���b�Z�[�W�{�b�N�X�Œʒm�B
			//�������̒��ŗ�O���������邱�Ƃ�SP1�ł͂���炵���B�����������Ȃ�ƃA�v���������I�����B
			//Win32�̃��b�Z�[�W�{�b�N�X���o���Ă������B�X�e�[�^�X�o�[�Ȃ���v�̂悤��
			//...�������A����ł�NullReferenceException���邢��ExecutionEngineException(!)����������ꍇ������BWin32�Ăяo���ł����߂��Ƃ�����o���ł���ȁB������߂ăR�����g�A�E�g
			try {
				string msg = String.Format(GEnv.Strings.GetString("Message.GUtil.InternalError"), dir, ex.Message);
				//MessageBox.Show(GEnv.Frame, String.Format(GEnv.Strings.GetString("Message.GUtil.InternalError"), dir, ex.Message), "Poderosa", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				//Win32.MessageBox(IntPtr.Zero, msg, "Poderosa", 0x00000010/*MB_HAND*/);
				GEnv.Frame.SetStatusBarText(msg);
			}
			catch(Exception ex2) {
				Debug.WriteLine(ex2.Message);
				Debug.WriteLine(ex2.StackTrace);
			}
		}
		private static StreamWriter GetDebugLog(ref string dir) {
			try {
				dir = AppDomain.CurrentDomain.BaseDirectory;
				return new StreamWriter(dir + "\\error.log", true, Encoding.Default);
			}
			catch(Exception) {
				dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Poderosa";
				if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
				return new StreamWriter(dir + "\\error.log", true, Encoding.Default);
			}
		}

		private static StreamWriter _debugLog = null;
		public static void WriteDebugLog(string data) {
			string dir = null;
			if(_debugLog==null) _debugLog = GetDebugLog(ref dir);
			_debugLog.WriteLine(data);
			_debugLog.Flush();
		}



		//���ɐݒ肷�ׂ����O�̃t�@�C���� host��null���Ƃ����͋󔒂ɂȂ�
		public static string CreateLogFileName(string host) {
			DateTime now = DateTime.Now;
			string date = String.Format("{0}{1,2:D2}{2,2:D2}", now.Year, now.Month, now.Day);

			string basefile;
			if(host==null || host.Length==0)
				basefile = String.Format("{0}\\{1}", GEnv.Options.DefaultLogDirectory, date);
			else {
				if(host.StartsWith("rsp://"))
					host = host.Substring(6); //rsp://�̂��Ƃ̕�����
				basefile = String.Format("{0}\\{1}_{2}", GEnv.Options.DefaultLogDirectory, ReplaceBadPathChar(host), date);
			}

			int n = 1;
			do {
				string filename;
				if(n==1)
					filename = String.Format("{0}.log", basefile);
				else
					filename = String.Format("{0}_{1}.log", basefile, n);

				if(!File.Exists(filename))
					return filename;
				else
					n++;
			} while(true);
		}

		public static string ReplaceBadPathChar(string src) {
			char ch = '_';
			return src.Replace('\\', ch).Replace('/', ch).Replace(':', ch).Replace(';', ch).
				Replace(',', ch).Replace('*', ch).Replace('?', ch).Replace('"', ch).
				Replace('<', ch).Replace('>', ch).Replace('|', ch);
		}



		public static void WriteNameValue(TextWriter wr, string name, string value) {
			wr.Write(name);
			wr.Write('=');
			wr.WriteLine(value);
		}

		public static string[] EncodingDescription(Encoding[] src) {
			string[] t = new string[src.Length];
			for(int i=0; i<src.Length; i++)
				t[i] = src[i].WebName;
			return t;
		}

		public static bool IsCursorKey(Keys key) {
			return key==Keys.Left || key==Keys.Right || key==Keys.Up || key==Keys.Down;
		}


		//KeyString�̋t�ϊ��@KeyConverter�̎����͎��ʂقǒx��
		public static Keys ParseKey(string s) {
			if(s.Length==0)
				return Keys.None;
			else if(s.Length==1) {
				char ch = s[0];
				if('0'<=ch && ch<='9')
					return Keys.D0 + (ch - '0');
				else
					return (Keys)Enum.Parse(typeof(Keys), s);
			}
			else
				return (Keys)Enum.Parse(typeof(Keys), s);
		}
		public static Keys ParseKey(string[] value) { //modifier���݂Ńp�[�X
			Keys modifier = Keys.None;
			for(int i=0; i<value.Length-1; i++) { //�Ō�ȊO
				string m = value[i];
				if(m=="Alt") modifier |= Keys.Alt;
				else if(m=="Shift") modifier |= Keys.Shift;
				else if(m=="Ctrl")  modifier |= Keys.Control;
				else throw new Exception(m + " is unknown modifier");
			}
			return modifier | GUtil.ParseKey(value[value.Length-1]);
		}

		//�L�[����Ή�����R���g���[���R�[�h(ASCII 0 ���� 31�܂�)�ɕϊ�����B�Ή�������̂��Ȃ����-1
		public static int KeyToControlCode(Keys key) {
			Keys modifiers = key & Keys.Modifiers;
			Keys body      = key & Keys.KeyCode;
			if(modifiers == Keys.Control) {
				int ib = (int)body;
				if((int)Keys.A <= ib && ib <= (int)Keys.Z)
					return ib - (int)Keys.A + 1;
				else if(body==Keys.Space)
					return 0;
				else
					return -1;
			}
			else
				return -1;
		}

		public static Thread CreateThread(ThreadStart st) {
			Thread t = new Thread(st);
			//t.ApartmentState = ApartmentState.STA;
            t.SetApartmentState(ApartmentState.STA);
			return t;
		}
		public static DialogResult ShowModalDialog(IPoderosaContainer c, Form dialog) {
			Form parent = c.AsForm();
			parent.Enabled = false;
			DialogResult r = dialog.ShowDialog(parent);
			parent.Enabled = true;
			dialog.Dispose();
			return r;
		}

	}



}

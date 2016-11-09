/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: StringResource.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Poderosa.Toolkit {
	/// <summary>
	/// StringResource �̊T�v�̐����ł��B
	/// </summary>
	public class StringResources {
		private string _resourceName;
		private ResourceManager _resMan;

		public StringResources(string name, Assembly asm) {
			_resourceName = name;
			LoadResourceManager(name, asm);
		}

		public string GetString(string id) {
			return _resMan.GetString(id); //�������ꂪ�x���悤�Ȃ炱�̃N���X�ŃL���b�V���ł�����΂������낤
		}

		private void LoadResourceManager(string name, Assembly asm) {
			//���ʂ͉p��E���{�ꂵ�����Ȃ�
			CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentUICulture;
			if(ci.Name.StartsWith("ja"))
				_resMan = new ResourceManager(name+"_ja", asm);
			else
				_resMan = new ResourceManager(name, asm);
		}
	}
}
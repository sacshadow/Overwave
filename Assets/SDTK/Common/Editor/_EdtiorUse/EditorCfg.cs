/*
	编辑器配置
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SDTK{

	public sealed class EditorCfg {
		public static readonly string  dataPath=Application.dataPath+"/SDTK/";
		
		private static void CheckDirectory(){
			DataRW.CheckDirectory(dataPath);
		}
		
		public static bool IsDataExists(string cfgName){
			CheckDirectory();
			return DataRW.IsDataExists(dataPath+cfgName);
		}
		
		public static void SaveData<T>(T data, string cfgName){
			SaveData<T>(data, cfgName, cfgName);
		}
		public static void SaveData<T>(T data, string cfgPath, string cfgName){
			CheckDirectory();
			DataRW.SetClassToXML<T>(data,dataPath+cfgPath+"/Data/"+cfgName+".xml");
		}
		public static T LoadData<T>(string cfgName){
			return LoadData<T>(cfgName, cfgName);
		}
		public static T LoadData<T>(string cfgPath, string cfgName){
			CheckDirectory();
			return DataRW.GetClassFromXML<T>(dataPath+cfgPath+"/Data/"+cfgName+".xml");
		}
	}
}
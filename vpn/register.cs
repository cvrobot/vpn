using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace RegistryTag
{
    class RegistryHelper
    {
        /// <summary>
        /// 读取指定名称的注册表的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetRegistryData(RegistryKey root, string subkey, string name) 
        { 
            string registData = ""; 
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            if (myKey != null)
            {
                registData = myKey.GetValue(name).ToString();
            }

            return registData; 
        } 

        /// <summary>
        /// 向注册表中写数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tovalue"></param> 
        public void SetRegistryData(RegistryKey root, string subkey, string name, string value) 
        { 
            RegistryKey aimdir = root.CreateSubKey(subkey); 
            aimdir.SetValue(name, value); 
        } 

        /// <summary>
        /// 删除注册表中指定的注册表项
        /// </summary>
        /// <param name="name"></param>
        public void DeleteRegist(RegistryKey root, string subkey, string name) 
        { 
            string[] subkeyNames;
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            subkeyNames = myKey.GetSubKeyNames();
            foreach (string aimKey in subkeyNames) 
            { 
                if(aimKey == name)
                    myKey.DeleteSubKeyTree(name); 
            } 
        }

        /// <summary>
        /// 判断指定注册表项的key是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRegistryKeyExist(RegistryKey root, string subkey, string name)
        {
            bool _exit = false;
            string[] subkeyNames;
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            if (myKey == null)
                return _exit;
            subkeyNames = myKey.GetValueNames();
            foreach (string keyName in subkeyNames)
            {
                if (keyName == name)
                {
                    _exit = true;
                    return _exit;
                }
            }

            return _exit;
        }

        /// <summary>
        /// 判断指定注册表项是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRegistryExist(RegistryKey root, string subkey, string name) 
        { 
            bool _exit = false; 
            string[] subkeyNames; 
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            if (myKey == null)
                return _exit;
            subkeyNames = myKey.GetSubKeyNames(); 
            foreach(string keyName in subkeyNames) 
            { 
                if(keyName == name) 
                { 
                    _exit = true; 
                    return _exit; 
                } 
            } 

            return _exit; 
        } 
    }
}

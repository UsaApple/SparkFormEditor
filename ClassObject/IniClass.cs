using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

namespace SparkFormEditor
{
    /// <summary>
    /// Ini文件操作类
    /// </summary>
    internal class IniClass
    {
        public string path;             //INI文件名  

        //声明读写INI文件的API函数  
        public IniClass(string INIPath)
        {
            path = INIPath;
        }

        //类的构造函数，传递INI文件名  
        public void IniWriteValue(string Section, string Key, string Value)
        {
            Win32Api.WritePrivateProfileString(Section, Key, Value, this.path);
        }

        //读INI文件  
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = Win32Api.GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }
    }


    /// <summary>
    /// Ini节点
    /// </summary>
    internal class IniSection
    {
        private Dictionary<string, string> _FDictionary;//节点值

        public Dictionary<string, string> FDictionary { get { return _FDictionary; } }
        private string FSectionName;//节点名称
        public IniSection(string SName)
        {
            FSectionName = SName;
            _FDictionary = new Dictionary<string, string>();
        }

        public string SectionName
        {
            get { return FSectionName; }
        }

        public int Count
        {
            get { return _FDictionary.Count; }
        }

        public void Clear()
        {
            _FDictionary.Clear();
        }

        //增加键值对
        public void AddKeyValue(string key, string value)
        {
            if (_FDictionary.ContainsKey(key))
                _FDictionary[key] = value;
            else
                _FDictionary.Add(key, value);
        }

        public void WriteValue(string key, string value)
        {
            AddKeyValue(key, value);
        }

        public void WriteValue(string key, bool value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public void WriteValue(string key, int value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public void WriteValue(string key, float value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public void WriteValue(string key, DateTime value)
        {
            AddKeyValue(key, Convert.ToString(value));
        }

        public string ReadValue(string key, string defaultv)
        {
            if (_FDictionary.ContainsKey(key))
                return _FDictionary[key];
            else
                return defaultv;
        }

        public bool ReadValue(string key, bool defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToBoolean(rt);
        }

        public int ReadValue(string key, int defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToInt32(rt);
        }

        public float ReadValue(string key, float defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToSingle(rt);
        }

        public DateTime ReadValue(string key, DateTime defaultv)
        {
            string rt = ReadValue(key, Convert.ToString(defaultv));
            return Convert.ToDateTime(rt);
        }

        public void SaveToStream(Stream stream)
        {
            StreamWriter SW = new StreamWriter(stream);
            SaveToStream(SW);
            SW.Dispose();
        }

        public void SaveToStream(StreamWriter SW)
        {
            SW.WriteLine("[" + FSectionName + "]");
            foreach (KeyValuePair<string, string> item in _FDictionary)
            {
                SW.WriteLine(item.Key + "=" + item.Value);
            }

        }
    }

    internal class MemIniFile
    {
        private ArrayList List;//所有节点信息

        private bool SectionExists(string SectionName)
        {
            foreach (IniSection ISec in List)
            {
                if (ISec.SectionName.ToLower() == SectionName.ToLower())
                    return true;
            }
            return false;
        }

        public IniSection FindSection(string SectionName)
        {
            foreach (IniSection ISec in List)
            {
                if (ISec.SectionName.ToLower() == SectionName.ToLower())
                    return ISec;
            }
            return null;
        }

        public MemIniFile()
        {
            List = new ArrayList();
        }

        /// <summary>
        /// 载入流 初始化类。用的时候直接读取
        /// IniSection ISec = Mini.FindSection("系统配置");
        /// ISec.ReadValue("Port", 345);
        /// </summary>
        /// <param name="stream"></param>
        public void LoadFromStream(Stream stream)
        {

            StreamReader SR = new StreamReader(stream, Encoding.GetEncoding("gb2312"));//需要指定编码，否则中文无法识别
            List.Clear();
            string st = null;
            IniSection Section = null;//节点
            int equalSignPos;
            string key, value;
            while (true)
            {
                st = SR.ReadLine();
                if (st == null)
                    break;
                st = st.Trim();
                if (st == "")
                    continue;
                if (st != "" && st[0] == '[' && st[st.Length - 1] == ']')
                {
                    st = st.Remove(0, 1);
                    st = st.Remove(st.Length - 1, 1);
                    Section = FindSection(st);
                    if (Section == null)
                    {
                        Section = new IniSection(st);
                        List.Add(Section);
                    }
                }
                else
                {
                    if (Section == null)
                    {
                        Section = FindSection("UnDefSection");
                        if (Section == null)
                        {
                            Section = new IniSection("UnDefSection");
                            List.Add(Section);
                        }
                    }
                    //开始解析         
                    equalSignPos = st.IndexOf('=');
                    if (equalSignPos != 0)
                    {
                        key = st.Substring(0, equalSignPos);
                        value = st.Substring(equalSignPos + 1, st.Length - equalSignPos - 1);

                        //jsp增加去掉左右双引号 开始
                        if (value[0] == '"') value = value.Substring(1, value.Length - 1);

                        if (value[value.Length - 1] == '"') value = value.TrimEnd(new char[] { '"' });

                        //jsp增加去掉左右双引号 结束


                        Section.AddKeyValue(key, value);//增加到节点
                    }
                    else
                        Section.AddKeyValue(st, "");
                }
            }
            SR.Dispose();
        }

        public void SaveToStream(Stream stream)
        {
            StreamWriter SW = new StreamWriter(stream);
            foreach (IniSection ISec in List)
            {
                ISec.SaveToStream(SW);
            }
            SW.Dispose();
        }

        public string ReadValue(string SectionName, string key, string defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public bool ReadValue(string SectionName, string key, bool defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public int ReadValue(string SectionName, string key, int defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public float ReadValue(string SectionName, string key, float defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public DateTime ReadValue(string SectionName, string key, DateTime defaultv)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec != null)
            {
                return ISec.ReadValue(key, defaultv);
            }
            else return defaultv;
        }

        public IniSection WriteValue(string SectionName, string key, string value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, bool value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, int value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, float value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public IniSection WriteValue(string SectionName, string key, DateTime value)
        {
            IniSection ISec = FindSection(SectionName);
            if (ISec == null)
            {
                ISec = new IniSection(SectionName);
                List.Add(ISec);
            }
            ISec.WriteValue(key, value);
            return ISec;
        }

        public void LoadFromFile(string FileName)
        {
            FileStream FS = new FileStream(System.IO.Path.GetFullPath(FileName), FileMode.Open);
            LoadFromStream(FS);
            FS.Close();
            FS.Dispose();
        }

        public void SaveToFile(string FileName)
        {
            FileStream FS = new FileStream(System.IO.Path.GetFullPath(FileName), FileMode.Create);
            SaveToStream(FS);
            FS.Close();
            FS.Dispose();
        }
    }

}

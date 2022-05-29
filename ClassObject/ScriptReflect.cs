using System;
using System.Data;
using System.Configuration;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using SparkFormEditor.Model;

namespace SparkFormEditor
{
    /// <summary> 
    /// 解析执行字符串，解析表达式
    /// 从别处复制，勿随意更改！ 
    /// </summary> 
    internal class Evaluator
    {
        public static EventHandler ReturnEvent = null;

        public static EventHandler ParentEvent = null;

        #region 构造函数
        /// <summary> 
        /// 可执行串的构造函数 
        /// </summary> 
        /// <param name="items"> 
        /// 可执行字符串数组 
        /// </param> 
        public Evaluator(EvaluatorItem[] items)
        {
            ConstructEvaluator(items); //调用解析字符串构造函数进行解析 
        }

        /// <summary> 
        /// 可执行串的构造函数 
        /// </summary> 
        /// <param name="returnType">返回值类型</param> 
        /// <param name="expression">执行表达式</param> 
        /// <param name="name">执行字符串名称</param> 
        public Evaluator(Type returnType, string expression, string name)
        {
            //创建可执行字符串数组 
            EvaluatorItem[] items = { new EvaluatorItem(returnType, expression, name) };
            ConstructEvaluator(items); //调用解析字符串构造函数进行解析 
        }

        /// <summary> 
        /// 可执行串的构造函数 Void方法，没有 返回结果的。只是动态执行代码，不需要返回结果，但是编译器不支持，必须有类型。
        /// 那么用 类型来强制代表void的方法。
        /// </summary> 
        /// <param name="returnType">返回值类型</param> 
        /// <param name="expression">执行表达式</param> 
        /// <param name="name">方法名</param> 
        public Evaluator(string expression, string name)
        {
            //创建可执行字符串数组 
            EvaluatorItem[] items = { new EvaluatorItem(typeof(System.DBNull), expression, name) };   //System.DBNull代替void
            ConstructEvaluator(items); //调用解析字符串构造函数进行解析 
        }

        /// <summary> 
        /// 可执行串的构造函数 
        /// </summary> 
        /// <param name="item">可执行字符串项</param> 
        public Evaluator(EvaluatorItem item)
        {
            EvaluatorItem[] items = { item };//将可执行字符串项转为可执行字符串项数组 
            ConstructEvaluator(items); //调用解析字符串构造函数进行解析 
        }

        /// <summary> 
        /// 解析字符串构造函数 
        /// </summary> 
        /// <param name="items">待解析字符串数组</param> 
        private void ConstructEvaluator(EvaluatorItem[] items)
        {
            //创建C#编译器实例 
            ICodeCompiler comp = (new CSharpCodeProvider().CreateCompiler());
            //编译器的传入参数 
            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("system.dll"); //添加程序集 system.dll 的引用 
            cp.ReferencedAssemblies.Add("system.data.dll"); //添加程序集 system.data.dll 的引用 
            cp.ReferencedAssemblies.Add("system.xml.dll"); //添加程序集 system.xml.dll 的引用 
            cp.ReferencedAssemblies.Add("system.windows.forms.dll");
            cp.GenerateExecutable = false; //不生成可执行文件 
            cp.GenerateInMemory = true; //在内存中运行 

            StringBuilder code = new StringBuilder(); //创建代码串 
            /* 
            * 添加常见且必须的引用字符串 
            */
            code.Append("using System; \n");
            code.Append("using System.Data; \n");
            code.Append("using System.Data.SqlClient; \n");
            code.Append("using System.Data.OleDb; \n");
            code.Append("using System.Xml; \n");
            code.Append("using System.Windows.Forms; \n");//System.Windows.Forms
            code.Append("namespace EvalGuy { \n"); //生成代码的命名空间为EvalGuy，和本代码一样 

            code.Append(" internal class _Evaluator { \n"); //产生 _Evaluator 类，所有可执行代码均在此类中运行 
            foreach (EvaluatorItem item in items) //遍历每一个可执行字符串项 
            {
                code.AppendFormat(" public {0} {1}() ", //添加定义公共函数代码 
                item.ReturnType.Name, //函数返回值为可执行字符串项中定义的返回值类型 ：System.DBNulll来代替void方法
                item.Name); //函数名称为可执行字符串项中定义的执行字符串名称 
                code.Append("{ "); //添加函数开始括号 

                //void 不需要返回的，不然报错
                if (item.ReturnType.Name.ToLower() != "dbnull")  // 在 C# 中无法使用 System.Void -- 使用 typeof(void) 获取 void 类型对象。 不支持void方法
                {
                    code.AppendFormat("return ({0});", item.Expression);//添加函数体，返回可执行字符串项中定义的表达式的值 
                }
                else
                {
                    code.Append(item.Expression);
                    code.Append("return null;");   //void方法，因为必须有返回类型，那么就返回System.DBNull
                }

                code.Append("}\n"); //添加函数结束括号 
            }
            code.Append("} }"); //添加类结束和命名空间结束括号 

            //得到编译器实例的返回结果 
            CompilerResults cr = comp.CompileAssemblyFromSource(cp, code.ToString());

            if (cr.Errors.HasErrors) //如果有错误 
            {
                StringBuilder error = new StringBuilder(); //创建错误信息字符串 
                error.Append("编译有错误的表达式: "); //添加错误文本 
                foreach (CompilerError err in cr.Errors) //遍历每一个出现的编译错误 
                {
                    error.AppendFormat("{0}\n", err.ErrorText); //添加进错误文本，每个错误后换行 
                }
                throw new Exception("编译错误: " + error.ToString());//抛出异常 
            }

            Assembly a = cr.CompiledAssembly; //获取编译器实例的程序集 
            _Compiled = a.CreateInstance("EvalGuy._Evaluator"); //通过程序集查找并声明 EvalGuy._Evaluator 的实例 
        }
        #endregion

        #region 公有成员
        /// <summary> 
        /// 执行字符串并返回整型值 
        /// </summary> 
        /// <param name="name">执行字符串名称</param> 
        /// <returns>执行结果</returns> 
        public int EvaluateInt(string name)
        {
            return (int)Evaluate(name);
        }
        /// <summary> 
        /// 执行字符串并返回字符串型值 
        /// </summary> 
        /// <param name="name">执行字符串名称</param> 
        /// <returns>执行结果</returns> 
        public string EvaluateString(string name)
        {
            return (string)Evaluate(name);
        }
        /// <summary> 
        /// 执行字符串并返回布尔型值 
        /// </summary> 
        /// <param name="name">执行字符串名称</param> 
        /// <returns>执行结果</returns> 
        public bool EvaluateBool(string name)
        {
            return (bool)Evaluate(name);
        }

        public void EvaluateVoid(string name)
        {
            //Evaluate(name);
            MethodInfo mi = _Compiled.GetType().GetMethod(name);//获取 _Compiled 所属类型中名称为 name 的方法的引用 
            mi.Invoke(_Compiled, null); //执行 mi 所引用的方法 
        }

        /// <summary> 
        /// 执行字符串并返 object 型值 
        /// </summary> 
        /// <param name="name">执行字符串名称</param> 
        /// <returns>执行结果</returns> 
        public object Evaluate(string name)
        {
            MethodInfo mi = _Compiled.GetType().GetMethod(name);//获取 _Compiled 所属类型中名称为 name 的方法的引用 
            return mi.Invoke(_Compiled, null); //执行 mi 所引用的方法 
        }
        #endregion

        #region 静态成员


        /// <summary> 
        /// 执行表达式:无窗体主题
        /// </summary> 
        /// <param name="code">要执行的表达式</param> 
        /// <returns>运算结果</returns> 
        static public void EvaluateRunString(string code)
        {
            Evaluator eval = new Evaluator(typeof(bool), code, staticMethodName);//生成 Evaluator 类的对像 
            eval.Evaluate(staticMethodName); //执行
        }


        /// <summary> 
        /// 执行表达式:根据窗体值进行判断，灵活执行
        ///             Encoding thisEncoding = Encoding.GetEncoding("gb2312");
        //string[] stringlines = File.ReadAllLines(TiWenDan.relativeRootPath + @"\BabyTCXML\AwokeMessage.cfg", thisEncoding); //放在数组里就好办多了
        /// </summary> 
        /// <param name="code">要执行的表达式</param> 
        /// <returns>运算结果</returns> 
        static public bool EvaluateRunString(Object ClassInstance, string code)
        {
            bool isOKPara = true;
            return EvaluateRunString(ClassInstance, code, ref isOKPara);
        }

        public static string messageList = ""; //一张表单多个人填写，有40个必填项，一个一个提示太麻烦。一起提示消息

        //同上构造方法：返回值是调用处是否中断返回，引用参数是返回表达式是否满足条件：比如满足条件后，主程序中还要业务处理
        static public bool EvaluateRunString(Object ClassInstance, string code, ref bool isOKPara)
        {
            try
            {
                messageList = "";

                bool ret = true;  //如果脚本执行弹出错误消息，返回False，调用的主程序方法中断，跳出。

                if (!code.Contains(".") && !code.Contains("Then"))
                {
                    //表达式计算的
                    //但是，如果是：主治医师 != 主治医师 Then Error(没有签名权限)，就会报错了。不能解析了
                    Evaluator eval = new Evaluator(typeof(bool), code, staticMethodName);//生成 Evaluator 类的对像 
                    eval.Evaluate(staticMethodName); //执行
                }
                else if (code.Contains("Then"))
                {

                    # region 条件只支持一个表达式
                    //bool isOK = false;
                    //bool isNotStartNot = true;  //不是以!开始取反

                    //code = code.Trim();
                    //if (code.StartsWith("!"))
                    //{
                    //    isNotStartNot = false;
                    //    code = code.TrimStart('!');
                    //}
                    //else
                    //{
                    //    isNotStartNot = true;
                    //}

                    //string[] arrRoot = code.Trim().Split(new string[] { "Then" }, StringSplitOptions.RemoveEmptyEntries);
                    //string[] arr = arrRoot[0].Trim().Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);

                    ////可能没有用==来判定，那么就是用!=来判定的
                    //if (arr.Length == 1)
                    //{
                    //    //不等于号
                    //    arr = arrRoot[0].Trim().Split(new string[] { "!=" }, StringSplitOptions.RemoveEmptyEntries);
                    //}


                    //if (arr.Length == 1)
                    //{
                    //    //勾选框，直接判断是选中还是不选中；即使取反也是如此
                    //    isOK = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == "True";
                    //}
                    //else
                    //{
                    //    //两个单引号表示空
                    //    arr[1] = arr[1].Trim().Replace("''","");

                    //    if (arrRoot[0].Contains("=="))
                    //    {
                    //        //等于号
                    //        isOK = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == arr[1].Trim();
                    //    }
                    //    else if (arrRoot[0].Contains("!="))
                    //    {
                    //        //不等于号
                    //        isOK = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() != arr[1].Trim();

                    //    }
                    //}

                    ////根据是否取反，最终判断条件是否成立
                    //if (isOK && isNotStartNot)
                    //{
                    //    isOK = true;
                    //}
                    //else if (!isOK && !isNotStartNot)
                    //{
                    //    isOK = true; // 假取反，负负得正
                    //}
                    //else
                    //{
                    //    isOK = false;
                    //}

                    # endregion 条件只支持一个表达式

                    # region //条件判定支持多个表达式，更加灵活的扩展 
                    bool isOK = false;
                    bool isNotStartNot = true;  //不是以!开始取反

                    string[] arrRoot = code.Trim().Split(new string[] { "Then" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] arrWhen = arrRoot[0].Trim().Split(new string[] { "And" }, StringSplitOptions.RemoveEmptyEntries); ; //所有的条件
                    string[] arr;     //每个条件的分项
                    bool isOKEach = false;

                    isOK = true;

                    object retObj = null;

                    for (int index = 0; index < arrWhen.Length; index++)
                    {
                        arrWhen[index] = arrWhen[index].Trim();

                        if (arrWhen[index].StartsWith("!"))
                        {
                            isNotStartNot = false;
                            arrWhen[index] = arrWhen[index].TrimStart('!');
                        }
                        else
                        {
                            isNotStartNot = true;
                        }

                        arr = arrWhen[index].Trim().Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);

                        //可能没有用==来判定，那么就是用!=来判定的
                        if (arr.Length == 1)
                        {
                            //不等于号
                            arr = arrWhen[index].Trim().Split(new string[] { "!=" }, StringSplitOptions.RemoveEmptyEntries);
                        }


                        if (arr.Length == 1)
                        {
                            //a.Text < 12 或者b.Checked 或者 a.Text <= b.Text 
                            //如果长度为1，那么：是含有<或>或者就是勾选框是否被选中的情况
                            string thisCondition = arr[0].Trim();
                            string[] arrExtend = thisCondition.Split(new string[] { "<=", "<", ">=", ">" }, StringSplitOptions.RemoveEmptyEntries); //字符串和分割符号，都是按照从左往右的次序分割，不会重复分割
                            string[] arrExtendChild;
                            string getValue = "";
                            double valueFirstNo = 0;

                            //isOKEach = !isNotStartNot;
                            bool getedValueNotException = false;

                            for (int aa = 0; aa < arrExtend.Length; aa++)
                            {
                                arrExtendChild = arrExtend[aa].Trim().Split('.');
                                if (arrExtendChild.Length > 1)
                                {
                                    getValue = GetValueControlProperty1(ClassInstance, arrExtendChild[0], arrExtendChild[1]).ToString();

                                    if (arrExtendChild[1].Trim() == "Checked")
                                    {
                                        thisCondition = thisCondition.Replace(arrExtend[aa].Trim(), getValue.ToLower()); //True False首字母大写报错，全部小写是ok的
                                    }
                                    else
                                    {
                                        valueFirstNo = StringNumber.getFirstNum(getValue);
                                        if (valueFirstNo != -1)
                                        {
                                            thisCondition = thisCondition.Replace(arrExtend[aa].Trim(), valueFirstNo.ToString());
                                        }
                                        else
                                        {
                                            //thisCondition = thisCondition.Replace(arrExtend[aa].Trim(), getValue); //a>12 会报错
                                            isOKEach = false; //大于小于之只能用于数字比较，如果没有数据内容，就认为是false，不管是大于还是小于比较
                                            getedValueNotException = true;
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    //不用处理，说明是个常量。 GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == "True";
                                }
                            }

                            //"aa<12"会异常
                            if (!getedValueNotException)
                            {
                                if (!bool.TryParse(thisCondition, out isOKEach))
                                {
                                    //这一行，效率很低很低。30几个脚本，要十几秒。
                                    //thisCondition = "45 > 45"   <Action Name="T" Event="Save">T.Text > 45 Then WarnList(体温不能超过45度)</Action>
                                    isOKEach = EvaluateToObject(thisCondition).ToString().ToLower() == "true"; //EvaluateToBool 一样很慢
                                }
                            }

                            ////在这里执行扩展 大于等于和小于号：<Action Name="评分" Event="SavedChanged">评分.Text &lt; 12 Then Creat(神经内科护理记录单)</Action>
                            //PropertyName = PropertyName.Replace(" 小于 ", " < "); //xml格式中节点值，部支持<号，转移后的&lt也不行，也报错。xml属性值是支持转移符号的。后来又支持了。
                            //if (PropertyName.StartsWith(nameof(EntXmlModel.Text)) && (PropertyName.Contains("<") || PropertyName.Contains(">")))
                            //{
                            //    double valueFirstNo = StringNumber.getFirstNum(cl.Text);
                            //    if (valueFirstNo != -1)
                            //    {
                            //        string executeStr = PropertyName.Replace(nameof(EntXmlModel.Text), StringNumber.getFirstNum(cl.Text).ToString());
                            //        Result = EvaluateToObject(executeStr);
                            //    }
                            //    else
                            //    {
                            //        Result = cl.Text;
                            //    }
                            //}
                            //else
                            //{
                            //    Result = cl.Text;
                            //}

                            //-----------------下面处理勾选框是否被选中的条件表达式----------------------------------
                            ////如果还是长度为1，那么肯定含有【.】 用来判断勾选框，直接判断是选中还是不选中；即使取反也是如此 ,如： ckb.Checked。否则就会超出索引报错
                            ////如果传入的参数值为空，没有用''来代替，上面的分割会忽略空的。那么如：【4 == 2  And  == 12:45 And 16:00 == 16:00 Then Warn(只有指定的第2页可以执行该脚本)】的时候出现异常
                            //isOKEach = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == "True";
                        }
                        else
                        {
                            //两个单引号表示空
                            arr[1] = arr[1].Trim().Replace("''", "");

                            //if (arrRoot[0].Contains("==")) //如果多个判断条件：有== 也有!=那么就错了
                            if (arrWhen[index].Contains("=="))
                            {
                                //等于号
                                if (arr[0].Trim().Split('.').Length == 1)
                                {
                                    isOKEach = (arr[0].Trim() == arr[1].Trim());
                                }
                                else
                                {
                                    //isOKEach = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == arr[1].Trim();

                                    retObj = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]);

                                    if (retObj != null)
                                    {
                                        isOKEach = retObj.ToString() == arr[1].Trim();
                                    }
                                    else
                                    {
                                        isOKEach = null == arr[1].Trim();  //false; //不成立
                                    }
                                }
                            }
                            else if (arrRoot[0].Contains("!="))
                            {
                                //不等于号
                                if (arr[0].Trim().Split('.').Length == 1)
                                {
                                    isOKEach = (arr[0].Trim() != arr[1].Trim());
                                }
                                else
                                {
                                    //isOKEach = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() != arr[1].Trim();

                                    //如果脚本配置不合理，会导致找不到控件GetValueControlProperty1方法返回null，那么就不用处理了。
                                    retObj = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]);

                                    if (retObj != null)
                                    {
                                        isOKEach = retObj.ToString() != arr[1].Trim();
                                    }
                                    else
                                    {
                                        isOKEach = null != arr[1].Trim();  //false; //不成立
                                    }
                                }

                            }

                        }

                        //根据是否取反，最终判断条件是否成立
                        if (isOKEach && isNotStartNot)
                        {
                            isOKEach = true;
                        }
                        else if (!isOKEach && !isNotStartNot)
                        {
                            isOKEach = true; // 假取反，负负得正
                        }
                        else
                        {
                            isOKEach = false;
                        }

                        isOK = isOK && isOKEach; //有一个条件不满足，那么就都不满足
                    }

                    # endregion //条件判定支持多个表达式，更加灵活的扩展

                    //isOK = isOK | isNotStartNot;

                    if (isOK)
                    {
                        //checkBox1.Checked Then textBox6.Text = 6666 & checkBox2.Checked = True
                        string[] arrCommit;// = arrRoot[1].Trim().Split('&');
                        if (arrRoot[1].Trim().Contains("&"))
                        {
                            arrCommit = arrRoot[1].Trim().Split('&');
                        }
                        else if (arrRoot[1].Trim().Contains(" And "))
                        {
                            arrCommit = arrRoot[1].Trim().Split(new string[] { "And" }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else
                        {
                            arrCommit = arrRoot[1].Trim().Split('&');
                        }

                        //string propertyName = "";//防止多级属性
                        for (int i = 0; i < arrCommit.Length; i++)
                        {
                            arr = arrCommit[i].Trim().Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);   // "时间.Text ="去除空的，就长度就为1了

                            if (arr.Length > 1)
                            {
                                if (arr[1].Contains("."))
                                {
                                    //含有多个.属性的：赋值为其他控件的内容
                                    SetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1],
                                                                GetValueControlProperty1(ClassInstance, arr[1].Trim().Split('.')[0], arr[1].Trim().Split('.')[1]));
                                }
                                else
                                {
                                    //两个单引号表示空
                                    arr[1] = arr[1].Trim().Replace("''", "");

                                    //直接赋值
                                    SetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1], arr[1].Trim().TrimStart('\"').TrimEnd('\"'));
                                }
                            }
                            else
                            {
                                //消息提示：Error("XXX")  Warn("XXX")
                                string commitStr = arrCommit[i].Trim();
                                if (commitStr.Contains("Error(") || commitStr.Contains("Warn(") || commitStr.Contains("WarnList("))
                                {
                                    int a = 0;
                                    int b = 0;

                                    //获取消息内容
                                    a = commitStr.IndexOf("(");
                                    b = commitStr.IndexOf(")");

                                    if (a < b && b - a - 1 > 0)
                                    {
                                        string msg = commitStr.Substring(a + 1, b - a - 1);

                                        //if(messageList.ToString()

                                        if (commitStr.Contains("Error("))
                                        {
                                            ret = false;  //如果弹出错误消息，返回给主程序false，跳出执行方法。
                                            MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        else if (commitStr.Contains("Warn("))
                                        {
                                            MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        }
                                        else if (commitStr.Contains("WarnList("))
                                        {
                                            //MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            messageList = msg;
                                        }
                                    }
                                }
                                else
                                {
                                    //创建表单……   如果表达式判断就不成立，不会执行到这里了
                                    if (ClassInstance is SparkFormEditor)
                                    {
                                        (ClassInstance as SparkFormEditor).ScriptOwnMethod(commitStr);
                                    }
                                    else
                                    {
                                        ////_ReturnValue = commitStr; //静态变量返回值设定
                                        //string key = ClassInstance.GetType().Name; 
                                        //if (_ReturnValue.ContainsKey(key))
                                        //{
                                        //    _ReturnValue[key] = commitStr;
                                        //}
                                        //else
                                        //{
                                        //    _ReturnValue.Add(key, commitStr);
                                        //}

                                        if (ReturnEvent != null && commitStr.Contains("ReprotType("))
                                        {
                                            ReturnEvent(commitStr, null);  //commitStr:ReprotType(XXX)
                                        }
                                        else if (ParentEvent != null && commitStr.Contains("ShowGroup("))
                                        {
                                            ParentEvent(commitStr, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //如果是 error和warn的时候，判断表达式条件不满足，是返回True。因为消息在本类中弹出的。且只有弹出error错误消息的时候返回False，主程序方法中中断跳出
                    }

                    isOKPara = isOK;

                    //if (isOK)
                    //{
                    //    //ScriptOwnMethod
                    //}
                }

                return ret;
            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr("在执行脚本：【" + code + "】的时候出现异常");
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 表格的时候，执行脚本解析
        /// </summary>
        /// <param name="tableInfor"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        static public bool EvaluateRunString(TableInfor tableInfor, string code)
        {
            bool ret = true;

            try
            {
                //签名.Text != '' Then Error(已经签名了哦)
                //日期.Text != '' Then 签名.Text = '' 
                //if (!code.Contains("."))
                if (!code.Contains(".") && !code.Contains("Then"))
                {
                    //一般用不到，执行一个字符串，比如加法运算等
                    Evaluator eval = new Evaluator(typeof(bool), code, staticMethodName);//生成 Evaluator 类的对像 
                    eval.Evaluate(staticMethodName); //执行
                }
                else if (code.Contains("Then"))
                {
                    # region 条件只支持一个表达式
                    //bool isOK = false;
                    //bool isNotStartNot = true;      //不是以!开始取反

                    //code = code.Trim();
                    //if (code.StartsWith("!"))
                    //{
                    //    isNotStartNot = false;      //如果是取反，那个“不是取反”的标记变量设置为True
                    //    code = code.TrimStart('!');
                    //}
                    //else
                    //{
                    //    isNotStartNot = true;
                    //}

                    //string[] arrRoot = code.Trim().Split(new string[] { "Then" }, StringSplitOptions.RemoveEmptyEntries);

                    //string[] arr = arrRoot[0].Trim().Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);

                    //if (arr.Length == 1)
                    //{
                    //    //不等于号
                    //    arr = arrRoot[0].Trim().Split(new string[] { "!=" }, StringSplitOptions.RemoveEmptyEntries);
                    //}

                    ////string[] arr = arrRoot[0].Trim().Split(new string[] { "==", "!=", "<=", "<", ">=", ">" }, StringSplitOptions.RemoveEmptyEntries); //字符串和分割符号，都是按照从左往右的次序分割，不会重复分割


                    //if (arr.Length == 1)
                    //{
                    //    //勾选框，直接判断是选中还是不选中；即使取反也是如此
                    //    isOK = tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).Text.Trim() == "True";
                    //    //isOK = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == "True";
                    //}
                    //else
                    //{
                    //    //两个单引号表示空
                    //    arr[1] = arr[1].Trim().Replace("''", "");

                    //    if (arrRoot[0].Contains("=="))
                    //    {
                    //        //等于号
                    //        isOK = tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).Text.Trim() == arr[1].Trim();
                    //    }
                    //    else if (arrRoot[0].Contains("!="))
                    //    {
                    //        //不等于号
                    //        isOK = tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).Text.Trim() != arr[1].Trim();

                    //    }
                    //    //else if (arrRoot[0].Contains("<"))
                    //    //{
                    //    //    //小于 < 无法适用于string类型操作，所以要转成double来处理
                    //    //    isOK = int.Parse(tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).Text.Trim()) < int.Parse(arr[1].Trim());
                    //    //}
                    //}

                    //if (isOK && isNotStartNot)
                    //{
                    //    isOK = true;
                    //}
                    //else if (!isOK && !isNotStartNot)
                    //{
                    //    isOK = true; // 假取反，负负得正
                    //}
                    //else
                    //{
                    //    isOK = false;
                    //}

                    ////isOK = isOK | isNotStartNot;
                    # endregion 条件只支持一个表达式

                    # region //条件判定支持多个表达式，更加灵活的扩展
                    bool isOK = false;
                    bool isNotStartNot = true;  //不是以!开始取反

                    string[] arrRoot = code.Trim().Split(new string[] { "Then" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] arrWhen = arrRoot[0].Trim().Split(new string[] { "And" }, StringSplitOptions.RemoveEmptyEntries); ; //所有的条件
                    string[] arr;     //每个条件的分项
                    bool isOKEach = false;

                    isOK = true;

                    for (int index = 0; index < arrWhen.Length; index++)
                    {
                        arrWhen[index] = arrWhen[index].Trim();

                        if (arrWhen[index].StartsWith("!"))
                        {
                            isNotStartNot = false;
                            arrWhen[index] = arrWhen[index].TrimStart('!');
                        }
                        else
                        {
                            isNotStartNot = true;
                        }

                        arr = arrWhen[index].Trim().Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);

                        //可能没有用==来判定，那么就是用!=来判定的
                        if (arr.Length == 1)
                        {
                            //不等于号
                            arr = arrWhen[index].Trim().Split(new string[] { "!=" }, StringSplitOptions.RemoveEmptyEntries);
                        }

                        //arr = new string[1];
                        //arr[0] = arrWhen[index].Trim();//.Replace("''", ""); //两个单引号表示空

                        if (arr.Length == 1)
                        {
                            //这里肯定要有数值了
                            //a.Text < 12 或者b.Checked 或者 a.Text <= b.Text 
                            //如果长度为1，那么：是含有<或>或者就是勾选框是否被选中的情况
                            string thisCondition = arr[0].Trim();
                            ////两个单引号表示空
                            //thisCondition = thisCondition.Trim().Replace("''", ""); //上面一开始就处理 \"\"  。这里和前面都不能去掉，否则长度就有问题了

                            string[] arrExtend = thisCondition.Split(new string[] { "==", "!=", "<=", "<", ">=", ">" }, StringSplitOptions.RemoveEmptyEntries); //字符串和分割符号，都是按照从左往右的次序分割，不会重复分割
                            string[] arrExtendChild;
                            string getValue = "";
                            double valueFirstNo = 0;

                            //isOKEach = !isNotStartNot;
                            bool getedValueNotException = false;

                            for (int aa = 0; aa < arrExtend.Length; aa++)
                            {
                                arrExtendChild = arrExtend[aa].Trim().Split('.');
                                if (arrExtendChild.Length > 1)
                                {
                                    //getValue = GetValueControlProperty1(ClassInstance, arrExtendChild[0], arrExtendChild[1]).ToString();
                                    getValue = tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arrExtendChild[0]).Text.Trim();

                                    if (arrExtendChild[1].Trim() == "Checked")
                                    {
                                        thisCondition = thisCondition.Replace(arrExtend[aa].Trim(), getValue.ToLower()); //True False首字母大写报错，全部小写是ok的
                                    }
                                    else
                                    {
                                        valueFirstNo = StringNumber.getFirstNum(getValue);
                                        if (valueFirstNo != -1) //两个单引号表示空
                                        {
                                            thisCondition = thisCondition.Replace(arrExtend[aa].Trim(), valueFirstNo.ToString());
                                            //thisCondition.Replace("''", "");
                                        }
                                        //else if (arrExtend.Length > 1 && arrExtend[1].Trim() == "''") 
                                        //{
                                        //    // == 和 != 不要取到数值，还是两个文本进行比较。还是走上一层外面的分支
                                        //    thisCondition = thisCondition.Replace(arrExtend[aa].Trim(), getValue);
                                        //    thisCondition = thisCondition.Replace("''", "");
                                        //}
                                        else
                                        {
                                            //thisCondition = thisCondition.Replace(arrExtend[aa].Trim(), getValue); //a>12 会报错
                                            isOKEach = false; //大于小于之只能用于数字比较，如果没有数据内容，就认为是false，不管是大于还是小于比较
                                            getedValueNotException = true;
                                            continue;

                                            ////判断空来这里做了
                                            //if (true)
                                            //{

                                            //}
                                            //else
                                            //{
                                            //    isOKEach = false; //大于小于之只能用于数字比较，如果没有数据内容，就认为是false，不管是大于还是小于比较
                                            //    getedValueNotException = true;
                                            //    continue;
                                            //}

                                        }
                                    }
                                }
                                else
                                {
                                    //不用处理，说明是个常量。 GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == "True";
                                }
                            }

                            //"aa<12"会异常
                            if (!getedValueNotException)
                            {
                                //isOKEach = EvaluateToObject(thisCondition).ToString().ToLower() == "true";
                                if (!bool.TryParse(thisCondition, out isOKEach))
                                {
                                    //这一行，效率很低很低。30几个脚本，要十几秒。
                                    isOKEach = EvaluateToObject(thisCondition).ToString().ToLower() == "true"; //EvaluateToBool 一样很慢
                                }
                            }

                            ////在这里执行扩展 大于等于和小于号：<Action Name="评分" Event="SavedChanged">评分.Text &lt; 12 Then Creat(神经内科护理记录单)</Action>
                            //PropertyName = PropertyName.Replace(" 小于 ", " < "); //xml格式中节点值，部支持<号，转移后的&lt也不行，也报错。xml属性值是支持转移符号的。后来又支持了。
                            //if (PropertyName.StartsWith(nameof(EntXmlModel.Text)) && (PropertyName.Contains("<") || PropertyName.Contains(">")))
                            //{
                            //    double valueFirstNo = StringNumber.getFirstNum(cl.Text);
                            //    if (valueFirstNo != -1)
                            //    {
                            //        string executeStr = PropertyName.Replace(nameof(EntXmlModel.Text), StringNumber.getFirstNum(cl.Text).ToString());
                            //        Result = EvaluateToObject(executeStr);
                            //    }
                            //    else
                            //    {
                            //        Result = cl.Text;
                            //    }
                            //}
                            //else
                            //{
                            //    Result = cl.Text;
                            //}

                            //-----------------下面处理勾选框是否被选中的条件表达式----------------------------------
                            ////如果还是长度为1，那么肯定含有【.】 用来判断勾选框，直接判断是选中还是不选中；即使取反也是如此 ,如： ckb.Checked。否则就会超出索引报错
                            ////如果传入的参数值为空，没有用''来代替，上面的分割会忽略空的。那么如：【4 == 2  And  == 12:45 And 16:00 == 16:00 Then Warn(只有指定的第2页可以执行该脚本)】的时候出现异常
                            //isOKEach = GetValueControlProperty1(ClassInstance, arr[0].Trim().Split('.')[0], arr[0].Trim().Split('.')[1]).ToString() == "True";
                        }
                        else
                        {
                            //两个单引号表示空
                            arr[1] = arr[1].Trim().Replace("''", "");

                            arr[0] = arr[0].Trim().Replace("''", "");

                            //if (arrRoot[0].Contains("==")) //如果多个判断条件：有== 也有!=那么就错了
                            if (arrWhen[index].Contains("=="))
                            {
                                //等于号
                                if (arr[0].Trim().Split('.').Length == 1)
                                {
                                    isOKEach = (arr[0].Trim() == arr[1].Trim());
                                }
                                else
                                {
                                    //UserID.Text %%包含的是常量。
                                    if (arr[0].Trim().Split('.')[0] == nameof(EntXmlModel.UserID))
                                    {
                                        isOKEach = tableInfor.Rows[tableInfor.CurrentCell.RowIndex].UserID == arr[1].Trim();
                                    }
                                    else
                                    {
                                        isOKEach = tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).Text.Trim() == arr[1].Trim();
                                    }
                                }
                            }
                            else if (arrRoot[0].Contains("!="))
                            {
                                //不等于号
                                if (arr[0].Trim().Split('.').Length == 1)
                                {
                                    isOKEach = (arr[0].Trim() != arr[1].Trim());
                                }
                                else
                                {
                                    if (arr[0].Trim().Split('.')[0] == nameof(EntXmlModel.UserID))
                                    {
                                        isOKEach = tableInfor.Rows[tableInfor.CurrentCell.RowIndex].UserID != arr[1].Trim();
                                    }
                                    else
                                    {
                                        isOKEach = tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).Text.Trim() != arr[1].Trim();
                                    }
                                }

                            }
                        }

                        //根据是否取反，最终判断条件是否成立
                        if (isOKEach && isNotStartNot)
                        {
                            isOKEach = true;
                        }
                        else if (!isOKEach && !isNotStartNot)
                        {
                            isOKEach = true; // 假取反，负负得正
                        }
                        else
                        {
                            isOKEach = false;
                        }

                        isOK = isOK && isOKEach; //有一个条件不满足，那么就都不满足
                    }

                    # endregion //条件判定支持多个表达式，更加灵活的扩展 


                    if (isOK)
                    {
                        //原来已经支持条件部分是 and ，但是还不支持大于小于
                        //checkBox1.Checked Then textBox6.Text = 6666 & checkBox2.Checked = True
                        string[] arrCommit;// = arrRoot[1].Trim().Split('&');
                        if (arrRoot[1].Trim().Contains("&"))
                        {
                            arrCommit = arrRoot[1].Trim().Split('&');
                        }
                        else if (arrRoot[1].Trim().Contains(" And "))
                        {
                            arrCommit = arrRoot[1].Trim().Split(new string[] { "And" }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else
                        {
                            arrCommit = arrRoot[1].Trim().Split('&');
                        }

                        //string propertyName = "";//防止多级属性
                        for (int i = 0; i < arrCommit.Length; i++)
                        {
                            arr = arrCommit[i].Trim().Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                            if (arr.Length > 1)
                            {
                                //表格暂时不支持赋值，因为关联表格单元格的重绘等很复杂
                                if (arr[0].Contains(".Visible")) //if (arr[0].Contains(".Visible")) //ReadOnly
                                {
                                    //控制表格某行不能编辑
                                    bool isVisible = true;

                                    if (!bool.TryParse(arr[1].Trim().Split('.')[0], out isVisible))
                                    {
                                        isVisible = false;
                                    }

                                    //如果录入一条数据后，日期时间等数据为一行，病情观察措施为多行，在多行最后一行签名后，本段落目前可以随意修改，会造成护理记录单不合格。
                                    tableInfor.Columns[tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).ColIndex].RTBE.Visible = isVisible;

                                    if (!isVisible)
                                    {
                                        ret = false;
                                    }
                                }
                                else if (arr[0].Contains(".ReadOnly")) //if (arr[0].Contains(".Visible")) //ReadOnly
                                {
                                    //控制表格某行不能编辑
                                    bool isVisible = true;

                                    if (!bool.TryParse(arr[1].Trim().Split('.')[0], out isVisible))
                                    {
                                        isVisible = false;
                                    }

                                    //如果录入一条数据后，日期时间等数据为一行，病情观察措施为多行，在多行最后一行签名后，本段落目前可以随意修改，会造成护理记录单不合格。
                                    tableInfor.Columns[tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).ColIndex].RTBE.ReadOnly = isVisible;

                                    //if (isVisible)
                                    //{
                                    //    ret = false;
                                    //}

                                    ret = true;
                                }
                                else
                                {
                                    if (arr[1].Contains("."))
                                    {
                                        //含有多个.属性的：赋值为其他控件的内容                                  
                                        tableInfor.Columns[tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).ColIndex].RTBE.Text =
                                            tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[1].Trim().Split('.')[0]).Text;
                                    }
                                    else
                                    {
                                        //直接赋值
                                        tableInfor.Columns[tableInfor.CellByRowNo_ColName(tableInfor.CurrentCell.RowIndex, arr[0].Trim().Split('.')[0]).ColIndex].RTBE.Text = arr[1].Trim().TrimStart('\"').TrimEnd('\"');
                                    }
                                }
                            }
                            else
                            {
                                //消息提示：Error("XXX")  Warn("XXX")
                                string commitStr = arrCommit[i].Trim();
                                if (commitStr.Contains("Error(") || commitStr.Contains("Warn("))
                                {
                                    int a = 0;
                                    int b = 0;

                                    //获取消息内容
                                    a = commitStr.IndexOf("(");
                                    b = commitStr.IndexOf(")");

                                    if (a < b && b - a - 1 > 0)
                                    {
                                        string msg = commitStr.Substring(a + 1, b - a - 1);

                                        if (commitStr.Contains("Error("))
                                        {
                                            ret = false;
                                            MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        else if (commitStr.Contains("Warn("))
                                        {
                                            MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr("请先确认脚本中的列名在表格中是否存在：\r\n" + ex.StackTrace);
            }

            return ret;
        }

        /// <summary> 
        /// 执行表达式并返回整型值 
        /// </summary> 
        /// <param name="code">要执行的表达式</param> 
        /// <returns>运算结果</returns> 
        static public int EvaluateToInteger(string code)
        {
            Evaluator eval = new Evaluator(typeof(int), code, staticMethodName);//生成 Evaluator 类的对像 
            return (int)eval.Evaluate(staticMethodName); //执行并返回整型数据 
        }
        /// <summary> 
        /// 执行表达式并返回字符串型值 
        /// </summary> 
        /// <param name="code">要执行的表达式</param> 
        /// <returns>运算结果</returns> 
        static public string EvaluateToString(string code)
        {
            Evaluator eval = new Evaluator(typeof(string), code, staticMethodName);//生成 Evaluator 类的对像 
            return (string)eval.Evaluate(staticMethodName); //执行并返回字符串型数据 
        }
        /// <summary> 
        /// 执行表达式并返回布尔型值 
        /// </summary> 
        /// <param name="code">要执行的表达式</param> 
        /// <returns>运算结果</returns> 
        static public bool EvaluateToBool(string code)
        {
            Evaluator eval = new Evaluator(typeof(bool), code, staticMethodName);//生成 Evaluator 类的对像 
            return (bool)eval.Evaluate(staticMethodName); //执行并返回布尔型数据 
        }
        /// <summary> 
        /// 执行表达式并返回 object 型值 
        /// </summary> 
        /// <param name="code">要执行的表达式</param> 
        /// <returns>运算结果</returns> 
        static public object EvaluateToObject(string code)
        {
            Evaluator eval = new Evaluator(typeof(object), code, staticMethodName);//生成 Evaluator 类的对像 
            return eval.Evaluate(staticMethodName); //执行并返回 object 型数据 
        }

        /// <summary>
        /// 只是动态执行语句，无返回值
        /// </summary>
        /// <param name="code"></param>
        static public void EvaluateToVoid(string code)
        {
            Evaluator eval = new Evaluator(code, staticMethodName);//生成 Evaluator 类的对像  //typeof(void):在 C# 中无法使用 System.Void -- 使用 typeof(void) 获取 void 类型对象。
            eval.EvaluateVoid(staticMethodName); //执行并返回 object 型数据 

        }
        #endregion

        #region 私有成员
        /// <summary> 
        /// 静态方法的执行字符串名称 
        /// </summary> 
        private const string staticMethodName = "__foo";
        /// <summary> 
        /// 用于动态引用生成的类，执行其内部包含的可执行字符串 
        /// </summary> 
        object _Compiled = null;
        #endregion

        /// <summary>
        /// 根据控件名和属性名取值 反射
        /// </summary>
        /// <param name="ClassInstance">控件所在实例</param>
        /// <param name="ControlName">控件名</param>
        /// <param name="PropertyName">属性名</param>
        /// <returns>属性值</returns>
        public static Object GetValueControlProperty(Object ClassInstance, string ControlName, string PropertyName)
        {
            Object Result = null;

            Type myType = ClassInstance.GetType();

            FieldInfo myFieldInfo = myType.GetField(ControlName, BindingFlags.NonPublic | //"|"为或运算，除非两个位均为0，运算结果为0，其他运算结果均为1
             BindingFlags.Instance);

            if (myFieldInfo != null)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(myFieldInfo.FieldType);

                PropertyDescriptor myProperty = properties.Find(PropertyName, false);

                if (myProperty != null)
                {
                    Object ctr;

                    ctr = myFieldInfo.GetValue(ClassInstance);

                    try
                    {
                        Result = myProperty.GetValue(ctr);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

            return Result;
        }

        /// <summary>
        /// 根据控件名和属性名赋值 反射
        /// </summary>
        /// <param name="ClassInstance">控件所在实例</param>
        /// <param name="ControlName">控件名</param>
        /// <param name="PropertyName">属性名</param>
        /// <param name=nameof(EntXmlModel.Value)>属性值</param>
        /// <returns>属性值</returns>
        public static Object SetValueControlProperty(Object ClassInstance, string ControlName, string PropertyName, Object Value)
        {
            Object Result = null;

            Type myType = ClassInstance.GetType();

            FieldInfo myFieldInfo = myType.GetField(ControlName, BindingFlags.NonPublic | BindingFlags.Instance); //动态加载的空间，找不到
            //FieldInfo myFieldInfo = myType.GetField(ControlName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField); 

            if (myFieldInfo != null)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(myFieldInfo.FieldType);

                PropertyDescriptor myProperty = properties.Find(PropertyName, false);  //这里设为True就不用区分大小写了

                if (myProperty != null)
                {
                    Object ctr;

                    ctr = myFieldInfo.GetValue(ClassInstance); //取得控件实例

                    try
                    {
                        if (ctr is CheckBox)
                        {
                            myProperty.SetValue(ctr, bool.Parse(Value.ToString()));
                        }
                        else if (ctr is ComboBox)
                        {
                            //下拉框
                            if (PropertyName == nameof(EntXmlModel.Items))
                            {
                                ((ComboBox)ctr).Items.Clear();
                                ((ComboBox)ctr).Items.AddRange(Value.ToString().Split(','));
                            }
                            if (PropertyName == "SelectedIndex")
                            {
                                ((ComboBox)ctr).SelectedIndex = int.Parse(Value.ToString());

                            }
                            else
                            {
                                ((ComboBox)ctr).Text = Value.ToString();
                            }
                        }
                        else
                        {
                            if (ctr is RichTextBoxExtended)
                            {
                                if ("%DateTime%".Equals(Value))
                                {
                                    if (!string.IsNullOrEmpty((ctr as RichTextBoxExtended)._YMD))
                                    {
                                        Value = string.Format("{0:" + (ctr as RichTextBoxExtended)._YMD + "}", DateTime.Now);
                                    }
                                    else
                                    {
                                        Value = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
                                    }
                                }
                                else if ("%Date%".Equals(Value)) //Value == "%Date%"
                                {
                                    if (!string.IsNullOrEmpty((ctr as RichTextBoxExtended)._YMD))
                                    {
                                        Value = string.Format("{0:" + (ctr as RichTextBoxExtended)._YMD + "}", DateTime.Now);
                                    }
                                    else
                                    {
                                        Value = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                                    }
                                }
                                else if ("%Time%".Equals(Value))
                                {
                                    if (!string.IsNullOrEmpty((ctr as RichTextBoxExtended)._YMD))
                                    {
                                        Value = string.Format("{0:" + (ctr as RichTextBoxExtended)._YMD + "}", DateTime.Now);
                                    }
                                    else
                                    {
                                        Value = string.Format("{0:HH:mm}", DateTime.Now);
                                    }
                                }
                            }

                            myProperty.SetValue(ctr, Value);
                        }

                        Result = ctr;
                    }
                    catch (Exception ex)
                    {
                        //if (ctr is CheckBox)
                        //{
                        //    myProperty.SetValue(ctr, bool.Parse(Value.ToString()));
                        //}
                        ////else if (ctr is ComboBox)
                        ////{
                        ////    //下拉框
                        ////    if (PropertyName == "Item")
                        ////    {
                        ////        ((ComboBox)ctr).Items.Clear();
                        ////        ((ComboBox)ctr).Items.AddRange(Value.ToString().Split(','));
                        ////    }
                        ////    else
                        ////    {
                        ////        ((ComboBox)ctr).Text = Value.ToString();
                        ////    }
                        ////}
                        //else
                        //{
                        throw ex;
                        //MessageBox.Show(ex.Message);                      
                        //}                                          
                    }
                }
            }
            return Result;
        }

        /// <summary>
        /// 根据控件名和属性名取值 非反射
        /// </summary>
        /// <param name="ClassInstance">控件所在实例</param>
        /// <param name="ControlName">控件名</param>
        /// <param name="PropertyName">属性名</param>
        /// <returns>属性值</returns>
        public static Object GetValueControlProperty1(Object ClassInstance, string ControlName, string PropertyName)
        {
            Object Result = null;

            Control cl = getControlFromName(ClassInstance, ControlName);

            if (cl != null)
            {
                Result = cl;

                switch (PropertyName)  //Checked、Text等,如果支持含有大于号等就是Text > 50
                {
                    case nameof(EntXmlModel.Text):
                        Result = cl.Text;
                        break;
                    case "Checked":
                        Result = ((CheckBox)cl).Checked;
                        break;
                    case "CheckState":
                        Result = ((CheckBox)cl).CheckState.ToString();
                        break;
                    case "SelectedIndex":
                        Result = ((ComboBox)cl).SelectedIndex;
                        break;
                    case "Tag":
                        Result = cl.Tag;
                        break;
                    case "Visible":
                        Result = cl.Visible;
                        break;
                    //case nameof(EntXmlModel.ReadOnly):
                    //    Result = cl.Visible;
                    //    break;
                    default:
                        //EvaluateToObject("Application.ExecutablePath"); //
                        //EvaluateToVoid("Application.Exit();");
                        //EvaluateToVoid("MessageBox.Show(\"a\");");
                        //如果条件表达式不是用==和!=来判断的，比如：<,<=,>,>=那么就这里再来扩展处理。
                        //string[] arr1 = "a <= b < c".Split(new string[] { "<=", "<", ">=", ">" }, StringSplitOptions.RemoveEmptyEntries); //字符串和分割符号，都是按照从左往右的次序分割，不会重复分割

                        ////在这里执行扩展 大于等于和小于号：<Action Name="评分" Event="SavedChanged">评分.Text &lt; 12 Then Creat(神经内科护理记录单)</Action>
                        //PropertyName = PropertyName.Replace(" 小于 ", " < "); //xml格式中节点值，部支持<号，转移后的&lt也不行，也报错。xml属性值是支持转移符号的。后来又支持了。
                        //if(PropertyName.StartsWith(nameof(EntXmlModel.Text)) && (PropertyName.Contains("<") || PropertyName.Contains(">")))
                        //{
                        //    double valueFirstNo = StringNumber.getFirstNum(cl.Text);
                        //    if (valueFirstNo != -1)
                        //    {
                        //        string executeStr = PropertyName.Replace(nameof(EntXmlModel.Text), StringNumber.getFirstNum(cl.Text).ToString());
                        //        Result = EvaluateToObject(executeStr);
                        //    }
                        //    else
                        //    {
                        //        Result = cl.Text;
                        //    }
                        //}
                        //else
                        //{
                        //    Result = cl.Text;
                        //}

                        Result = cl.Text;

                        break;
                }
            }

            return Result;
        }

        /// <summary>
        /// 根据控件名和属性名赋值 非反射
        /// </summary>
        /// <param name="ClassInstance">控件所在实例</param>
        /// <param name="ControlName">控件名</param>
        /// <param name="PropertyName">属性名</param>
        /// <param name=nameof(EntXmlModel.Value)>属性值</param>
        /// <returns>属性值</returns>
        public static Object SetValueControlProperty1(Object ClassInstance, string ControlName, string PropertyName, Object Value)
        {
            Object Result = null;

            #region 老的实现代码，存在很多缺陷，不支持动态加载的控件等
            //Type myType = ClassInstance.GetType();

            //FieldInfo myFieldInfo = myType.GetField(ControlName, BindingFlags.NonPublic | BindingFlags.Instance); //动态加载的空间，找不到
            ////FieldInfo myFieldInfo = myType.GetField(ControlName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField); 

            //if (myFieldInfo != null)
            //{
            //    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(myFieldInfo.FieldType);

            //    PropertyDescriptor myProperty = properties.Find(PropertyName, false);  //这里设为True就不用区分大小写了

            //    if (myProperty != null)
            //    {
            //        Object ctr;

            //        ctr = myFieldInfo.GetValue(ClassInstance); //取得控件实例

            //        try
            //        {
            //            if (ctr is CheckBox)
            //            {
            //                myProperty.SetValue(ctr, bool.Parse(Value.ToString()));
            //            }
            //            else if (ctr is ComboBox)
            //            {
            //                //下拉框
            //                if (PropertyName == nameof(EntXmlModel.Items))
            //                {
            //                    ((ComboBox)ctr).Items.Clear();
            //                    ((ComboBox)ctr).Items.AddRange(Value.ToString().Split(','));
            //                } if (PropertyName == "SelectedIndex")
            //                {
            //                    ((ComboBox)ctr).SelectedIndex = int.Parse(Value.ToString());

            //                }
            //                else
            //                {
            //                    ((ComboBox)ctr).Text = Value.ToString();
            //                }
            //            }
            //            else
            //            {
            //                myProperty.SetValue(ctr, Value);
            //            }

            //            Result = ctr;
            //        }
            //        catch (Exception ex)
            //        {
            //            //if (ctr is CheckBox)
            //            //{
            //            //    myProperty.SetValue(ctr, bool.Parse(Value.ToString()));
            //            //}
            //            ////else if (ctr is ComboBox)
            //            ////{
            //            ////    //下拉框
            //            ////    if (PropertyName == "Item")
            //            ////    {
            //            ////        ((ComboBox)ctr).Items.Clear();
            //            ////        ((ComboBox)ctr).Items.AddRange(Value.ToString().Split(','));
            //            ////    }
            //            ////    else
            //            ////    {
            //            ////        ((ComboBox)ctr).Text = Value.ToString();
            //            ////    }
            //            ////}
            //            //else
            //            //{
            //                throw ex;
            //                //MessageBox.Show(ex.Message);                      
            //            //}                                          
            //        }
            //    }
            //}
            #endregion 老的实现代码，存在很多缺陷，不支持动态加载的控件等

            Control cl = getControlFromName(ClassInstance, ControlName);

            if (cl != null)
            {
                Result = cl;

                switch (PropertyName)
                {
                    case nameof(EntXmlModel.Text):
                        if (cl is RichTextBoxExtended)
                        {
                            //打开表单的脚本，如果值一样也会提示保存
                            //((RichTextBoxExtended)cl).Focus();
                            ((RichTextBoxExtended)cl).OldText = ((RichTextBoxExtended)cl).Text; //打开表单的脚本，如果值一样也会提示保存
                        }
                        
                        if (cl is RichTextBoxExtended)
                        {
                            if ("%DateTime%".Equals(Value))
                            {
                                if (!string.IsNullOrEmpty((cl as RichTextBoxExtended)._YMD))
                                {
                                    Value = string.Format("{0:" + (cl as RichTextBoxExtended)._YMD + "}", DateTime.Now);
                                }
                                else
                                {
                                    Value = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
                                }
                            }
                            else if ("%Date%".Equals(Value)) //Value == "%Date%"
                            {
                                if (!string.IsNullOrEmpty((cl as RichTextBoxExtended)._YMD))
                                {
                                    Value = string.Format("{0:" + (cl as RichTextBoxExtended)._YMD + "}", DateTime.Now);
                                }
                                else
                                {
                                    Value = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                                }
                            }
                            else if ("%Time%".Equals(Value))
                            {
                                if (!string.IsNullOrEmpty((cl as RichTextBoxExtended)._YMD))
                                {
                                    Value = string.Format("{0:" + (cl as RichTextBoxExtended)._YMD + "}", DateTime.Now);
                                }
                                else
                                {
                                    Value = string.Format("{0:HH:mm}", DateTime.Now);
                                }
                            }
                        }

                        cl.Text = Value.ToString();
                        break;
                    case "Checked":
                        ((CheckBox)cl).Checked = bool.Parse(Value.ToString());
                        break;
                    case "CheckState":
                        //((CheckBox)cl).CheckState = bool.Parse(Value.ToString());
                        if (Value.ToString() == "Checked")
                        {
                            ((CheckBox)cl).CheckState = CheckState.Checked;
                        }
                        else if (Value.ToString() == "Indeterminate")
                        {
                            ((CheckBox)cl).CheckState = CheckState.Indeterminate;
                        }
                        else if (Value.ToString() == "Unchecked")
                        {
                            ((CheckBox)cl).CheckState = CheckState.Unchecked;
                        }
                        break;
                    case "SelectedIndex":
                        ((ComboBox)cl).SelectedIndex = int.Parse(Value.ToString());
                        break;
                    case nameof(EntXmlModel.Items):
                        ((ComboBox)cl).Items.Clear();
                        ((ComboBox)cl).Items.AddRange(Value.ToString().Split(','));
                        break;
                    case "Visible":
                        cl.Visible = bool.Parse(Value.ToString());
                        //cl.Refresh();
                        break;
                    case "Enabled":
                        cl.Enabled = bool.Parse(Value.ToString());
                        break;
                    default:
                        cl.Text = Value.ToString();
                        break;
                }
            }

            return Result;
        }

        //提高性能
        //public static Dictionary<string, Control> controlsDic = new Dictionary<string, Control>();
        public static Control getControlFromName(Object me, string name)
        {
            //Control[] ct = null;// new Control[1]; // = ((Control)me).Controls.Find(name, true);

            //if (controlsDic != null && controlsDic.Count > 0)
            //{
            //    if (controlsDic.ContainsKey(name))
            //    {
            //        ct = new Control[1];
            //        ct[0] = controlsDic[name];
            //    }
            //    else
            //    {
            //        ct = null;
            //    }
            //}
            //else
            //{
            //    ct = ((Control)me).Controls.Find(name, true);
            //}

            Control[] ct = ((Control)me).Controls.Find(name, true);

            if (ct != null && ct.Length > 0)
            {
                return ct[0];
            }
            else
            {
                return null;
            }
        }

    }

    /// <summary> 
    /// 可执行字符串项（即一条可执行字符串） 
    /// </summary> 
    internal class EvaluatorItem
    {
        /// <summary> 
        /// 返回值类型 
        /// </summary> 
        public Type ReturnType;
        /// <summary> 
        /// 执行表达式 
        /// </summary> 
        public string Expression;
        /// <summary> 
        /// 执行字符串名称 
        /// </summary> 
        public string Name;
        /// <summary> 
        /// 可执行字符串项构造函数 
        /// </summary> 
        /// <param name="returnType">返回值类型</param> 
        /// <param name="expression">执行表达式</param> 
        /// <param name="name">执行字符串名称</param> 
        public EvaluatorItem(Type returnType, string expression, string name)
        {
            ReturnType = returnType;
            Expression = expression;
            Name = name;
        }
    }
}
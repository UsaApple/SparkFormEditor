using System;
using System.Diagnostics;

namespace SparkFormEditor
{
    /// <summary>
    /// 日志处理类
    /// </summary>
    public class LogManager
    {
        private static log4net.ILog myLog;
        private string _logNode = "TRT";

        /// <summary>
        /// 接口配置，根据节点生成多个log文件，默认为TRT
        /// </summary>
        public string LogNode
        {
            get { return _logNode; }
            set { _logNode = value; }
        }

        /// <summary>
        /// 隐藏构造函数
        /// </summary>
        public LogManager()
        {
        }

        /// <summary>
        /// 记录信息到文件日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="eventMessage">日志信息</param>
        public void WriteLog(EventLogEntryType logType, string eventMessage)
        {
            if (myLog == null)
            {
                myLog = log4net.LogManager.GetLogger(LogNode);
            }
            switch (logType)
            {
                case EventLogEntryType.Error:
                    myLog.Error(eventMessage);
                    break;
                case EventLogEntryType.Information:
                    myLog.Info(eventMessage);
                    break;
                //case EventLogEntryType.Warning:
                //    myLog.Debug(eventMessage);
                //    break;
                default:
                    myLog.Debug(eventMessage);
                    break;
            }
        }

        /// <summary>
        /// 记录信息到文件日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="eventMessage">日志信息</param>
        /// <param name="args">参数</param>
        public void WriteLog(EventLogEntryType logType, string eventMessage, params object[] args)
        {
            if (myLog == null)
            {
                myLog = log4net.LogManager.GetLogger(LogNode);
            }
            switch (logType)
            {
                case EventLogEntryType.Error:
                    myLog.ErrorFormat(eventMessage, args);
                    break;
                case EventLogEntryType.Information:
                    myLog.InfoFormat(eventMessage, args);
                    break;
                //case EventLogEntryType.Warning:
                //    myLog.DebugFormat(eventMessage, args);
                //    break;
                default:
                    myLog.DebugFormat(eventMessage, args);
                    break;
            }
        }

        /// <summary>
        /// 记录错误信息到系统日志
        /// </summary>
        /// <param name="ex">错误对象</param>
        public void WriteErr(Exception ex)
        {
            WriteLog(EventLogEntryType.Error, ex.StackTrace + "\r\n  " + ex.Message);
        }

        /// <summary>
        /// 记录错误信息到系统日志
        /// </summary>
        /// <param name="eventMessage">日志信息</param>
        public void WriteErr(string eventMessage)
        {
            WriteLog(EventLogEntryType.Error, eventMessage);
        }

        /// <summary>
        /// 记录错误信息到系统日志
        /// </summary>
        /// <param name="eventMessage">日志信息</param>
        /// <param name="args">参数</param>
        public void WriteErr(string eventMessage, params object[] args)
        {
            WriteLog(EventLogEntryType.Error, eventMessage, args);
        }

        /// <summary>
        /// 记录警告信息到系统日志
        /// </summary>
        /// <param name="eventMessage">日志信息</param>
        public void WriteWarning(string eventMessage)
        {
            WriteLog(EventLogEntryType.Warning, eventMessage);
        }

        /// <summary>
        /// 记录警告信息到系统日志
        /// </summary>
        /// <param name="eventMessage">日志信息</param>
        /// <param name="args">参数</param>
        public void WriteWarning(string eventMessage, params object[] args)
        {
            WriteLog(EventLogEntryType.Warning, eventMessage, args);
        }

        /// <summary>
        /// 记录一般信息到系统日志
        /// </summary>
        /// <param name="eventMessage">日志信息</param>
        public void WriteInformation(string eventMessage)
        {
            WriteLog(EventLogEntryType.Information, eventMessage);
        }

        /// <summary>
        /// 记录一般信息到系统日志
        /// </summary>
        /// <param name="eventMessage">日志信息</param>
        /// <param name="args">参数</param>
        public void WriteInformation(string eventMessage, params object[] args)
        {
            WriteLog(EventLogEntryType.Information, eventMessage, args);
        }

        /// <summary>
        /// 记录一般信息到系统日志
        /// </summary>
        /// <param name="eventMessage">日志信息</param>
        /// <param name="args">参数</param>
        public void WriteDebug(string eventMessage, params object[] args)
        {
            WriteLog(EventLogEntryType.Warning, eventMessage, args);
        }
    }
}

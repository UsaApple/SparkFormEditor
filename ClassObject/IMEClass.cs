using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SparkFormEditor
{
    ///<summary>
    ///输入法操作控制类
    ///</summary>
    internal class IMEClass
    {
        /// <summary>
        /// 将输入法设置为半角
        /// </summary>
        /// <param name="AHandle"></param>
        public static void SetHalfShape(IntPtr AHandle)
        {
            ////解决Tab键切换，还是全角的问题 （都是在Enter中处理）
            //if (this.ImeMode != ImeMode.Hangul)
            //{
            //    this.ImeMode = ImeMode.Hangul;
            //}

            try
            {
                IntPtr vIme = Win32Api.ImmGetContext(AHandle);
                if (Win32Api.ImmGetOpenStatus(vIme)) // 输入法是打开的 
                {
                    int vMode = 0, vSentence = 0;
                    if (Win32Api.ImmGetConversionStatus(vIme, ref vMode, ref vSentence)) // 获取输入法状态
                    {
                        if ((vMode & Consts.IME_CMODE_FULLSHAPE) > 0) // 是全角 
                        {
                            vMode &= (~Consts.IME_CMODE_FULLSHAPE);
                            Win32Api.ImmSetConversionStatus(vIme, vMode, vSentence);
                        }
                    }
                }
                Win32Api.ImmReleaseContext(AHandle, vIme);
            }
            catch
            {

            }
        }
    }
}

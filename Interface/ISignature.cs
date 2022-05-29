using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SparkFormEditor
{
    public interface ISignature
    {
        /// <summary>
        /// 启动手写板签名
        /// </summary>
        /// <returns>签章数据信息</returns>
        string RaisePenTabletSignature();

        /// <summary>
        /// 启动手写板签名(包含指纹签名)
        /// </summary>
        /// <returns>签章数据信息</returns>
        bool RaiseFingerprintAndPenTabletSig();

        /// <summary>
        /// 获取签名图片的字节数组
        /// </summary>
        /// <returns></returns>
        byte[] GetSignatureImgBytes();

        /// <summary>
        /// 获取签名图片的字符串
        /// </summary>
        /// <returns></returns>
        string GetSignatureImgStr();

        /// <summary>
        /// 直接返回图片 手写
        /// </summary>
        /// <returns></returns>
        Image GetSignatureImg();

        /// <summary>
        /// 直接返回图片 指纹
        /// </summary>
        /// <returns></returns>
        Image GetSignatureImgEx();

        /// <summary>
        /// 验证方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool PatientSignVerify(object[] obj);
    }
}

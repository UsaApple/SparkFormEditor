using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using SparkFormEditor.Model;

namespace SparkFormEditor
{
    ///<summary>
    ///文件操作类
    ///</summary>
    internal class FileHelp
    {
        /// <summary>
        /// 根据指定目录，指定的文件名，返回对应的文件完整路径
        /// 如果为空，表示没有找到指定文件
        /// </summary>
        public static string GetFilePathFromRootDirAndFileName(string rootPath,string fileName)
        {
            string retFilePath = "";

            //遍历文件夹
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            DirectoryInfo dirChild;

            if (!Directory.Exists(rootPath))
            {
                MessageBox.Show("不存在指定目录：" + rootPath);
            }
            else
            {

                //遍历主目录下的文件
                foreach (FileInfo f in dir.GetFiles("*.xml")) //查找.xml的文件
                {
                    if (f.Name.Replace(".xml", "").Trim() == fileName.Trim())
                    {
                        retFilePath = f.FullName;
                        return retFilePath;
                    }
                }

                //本方法内只支持两层，内部方法  SetTemplateChild在继续遍历
                foreach (DirectoryInfo dChild in dir.GetDirectories("*"))
                {
                    //如果用GetDirectories("ab*"),那么全部以ab开头的目录会被显示
                    dirChild = new DirectoryInfo(dChild.FullName);

                    //遍历该子目录下的文件:
                    foreach (FileInfo f in dirChild.GetFiles("*.xml")) //查找.xml的文件
                    {
                        if (f.Name.Replace(".xml", "").Trim() == fileName.Trim())
                        {
                            retFilePath = f.FullName;
                            return retFilePath;
                        }
                    }

                    //遍历子文件夹 递层遍历N所有层
                    retFilePath = GetTemplateChild(dChild.FullName, fileName);

                    if (retFilePath != "")
                    {
                        return retFilePath;
                    }
                }
            }

            return retFilePath;
        }

        /// <summary>
        /// 根据子目录，继续遍历
        /// </summary>
        /// <param name="dChildPath"></param>
        /// <param name="fileName"></param>
        private static string GetTemplateChild(string dChildPath, string fileName)
        {
            string retFilePath = "";

            DirectoryInfo dir = new DirectoryInfo(dChildPath);
            DirectoryInfo dirChild;

            //逐个遍历子目录下面的文件，再遍历子子目录
            foreach (DirectoryInfo dChild in dir.GetDirectories("*"))
            {
                //遍历该子目录下的文件:
                foreach (FileInfo f in dChild.GetFiles("*.xml")) //查找.xml的文件
                {
                    if (f.Name.Replace(".xml", "").Trim() == fileName.Trim())
                    {
                        retFilePath = f.FullName;
                        return retFilePath;
                    }
                }

                //如果用GetDirectories("ab*"),那么全部以ab开头的目录会被显示
                dirChild = new DirectoryInfo(dChild.FullName);

                //继续遍历
                retFilePath = GetTemplateChild(dChild.FullName, fileName);

                if (retFilePath != "")
                {
                    return retFilePath;
                }
            }

            return retFilePath;
        }

        /// <summary>
        /// 二进制字符串字符串转换为图片
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static System.Drawing.Image ConvertStringToImage(string str)
        {
            try
            {
                byte[] ImageBytes = Convert.FromBase64String(str);
                System.IO.MemoryStream ImageMem = new System.IO.MemoryStream(ImageBytes);
                return System.Drawing.Image.FromStream(ImageMem);
            }
            catch
            { }

            return null;
        }

        //public static System.Drawing.Bitmap ConvertStringToBitmap(string str)
        //{
        //    try
        //    {
        //        byte[] ImageBytes = Convert.FromBase64String(str);
        //        Stream ImageMem = new Stream(ImageBytes);
        //        return System.Drawing.Bitmap.FromStream(ImageMem);
        //    }
        //    catch
        //    { }

        //    return null;
        //}

        /// <summary>
        /// 图片转换为二进制字符串
        /// </summary>
        /// <param name="IMG"></param>
        /// <returns></returns>
        public static string ConvertImageToString(System.Drawing.Image IMG)
        {
            if (IMG == null) return "";

            try
            {
                //System.IO.MemoryStream ImageMem = new System.IO.MemoryStream();
                //IMG.Save(ImageMem, IMG.RawFormat);
                //byte[] ImageBytes = ImageMem.GetBuffer();
                //return Convert.ToBase64String(ImageBytes);

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                IMG.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] buffer = ms.ToArray();
                return Convert.ToBase64String(buffer);

            }
            catch (Exception ex)
            {
                Comm.LogHelp.WriteErr(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 清空指定的文件夹，但不删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            try
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {

                    if (File.Exists(d))
                    {
                        if (!d.EndsWith(".png") && !d.EndsWith(".bmp"))
                        {
                            continue;
                        }

                        FileInfo fi = new FileInfo(d);
                      
                        if (fi.Attributes.ToString().IndexOf(nameof(EntXmlModel.ReadOnly)) != -1)
                            fi.Attributes = FileAttributes.Normal;

                        try
                        {
                            File.Delete(d);//直接删除其中的文件  
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    else
                    {
                        DirectoryInfo d1 = new DirectoryInfo(d);
                        if (d1.GetFiles().Length != 0)
                        {
                            DeleteFolder(d1.FullName);////递归删除子文件夹
                        }

                        try
                        {
                            Directory.Delete(d);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
                //正由另一进程使用,因此该进程无法访问该文件。
            }
        }
        /// <summary>
        /// 删除文件夹及其内容
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder1(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    if (!d.EndsWith(".png"))
                    {
                        continue;
                    }

                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf(nameof(EntXmlModel.ReadOnly)) != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
                else
                    DeleteFolder(d);////递归删除子文件夹
                Directory.Delete(d);
            }
        }

    }

    //文件夹和文件名排序
    internal enum SortOption
    {
        FileName,
        FolderName,
        Extension,
        CreationTime
    }

    /// <summary>
    /// 文件和文件夹排序比较
    /// </summary>
    internal class SortFile : IComparer
    {
        SortOption mso;
        public SortFile(SortOption so)
        {
            mso = so;
        }

        int IComparer.Compare(object a, object b)
        {
            FileInfo fa = (FileInfo)a;
            FileInfo fb = (FileInfo)b;

            switch (mso)
            {
                case SortOption.FileName:
                    return String.Compare(fa.Name, fb.Name, true);
                    break;
                case SortOption.Extension:
                    return String.Compare(Path.GetExtension(fa.Name), Path.GetExtension(fb.Name), true);
                    break;
                case SortOption.CreationTime:
                    return DateTime.Compare(fa.CreationTime, fb.CreationTime);
                    break;
                default:
                    break; ;
            }
            return 0;
        }
    }

    /// <summary>
    /// 文件名排序
    /// </summary>
    internal class SortFolder : IComparer
    {
        SortOption mso;
        public SortFolder(SortOption so)
        {
            mso = so;
        }

        int IComparer.Compare(object a, object b)
        {
            DirectoryInfo fa = (DirectoryInfo)a;
            DirectoryInfo fb = (DirectoryInfo)b;

            switch (mso)
            {
                case SortOption.FolderName:
                    return String.Compare(fa.Name, fb.Name, true);
                    break;
                case SortOption.Extension:
                    return String.Compare(Path.GetExtension(fa.Name), Path.GetExtension(fb.Name), true);
                    break;
                case SortOption.CreationTime:
                    return DateTime.Compare(fa.CreationTime, fb.CreationTime);
                    break;
                default:
                    break; ;
            }
            return 0;
        }
    }
}

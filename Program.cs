using System;
using System.Collections.Generic;
using System.Text;
using PDFLibNet;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
namespace pdf2png
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始生成图片");
            //GetImgFromPDFLibNet();
            Thread cbThread = new Thread(new ThreadStart(GetImgFromAcrobat));
            cbThread.TrySetApartmentState(ApartmentState.STA);
            cbThread.Start();
            cbThread.Join();
            Console.WriteLine("生成图片完成");
            Console.Read();

        }

        /// <summary>
        /// 调用PDFLibNet生成图片(如果pdf文件有特殊字体，无法转化)
        /// </summary>
        private static void GetImgFromPDFLibNet()
        {
            string pdfInputPath = AppDomain.CurrentDomain.BaseDirectory + "test.pdf";
            string imageOutputPath = AppDomain.CurrentDomain.BaseDirectory +@"\Pdf2Img\";
            ImageFormat imageFormat = ImageFormat.Png;
            int DPI = 200; int definition = 30;
            try
            {
                List<int> list = new List<int>();
                PDFWrapper wrapper = new PDFWrapper();
                wrapper.LoadPDF(pdfInputPath);
                if (!Directory.Exists(imageOutputPath))
                {
                    Directory.CreateDirectory(imageOutputPath);
                }
                if (list.Count == 0)
                {
                    for (int i = 1; i <= wrapper.PageCount; i++)
                    {
                        list.Add(i);
                    }
                }
                foreach (int num in list)
                {
                    string filename = imageOutputPath + num.ToString() + "." + imageFormat.ToString();

                    wrapper.ExportJpg(filename, num, num, (double)DPI, definition);
                    Thread.Sleep(1000);
                    Console.WriteLine("生成图片" + filename);

                }
                wrapper.Dispose();
            }
            catch (Exception exception)
            {
                Console.WriteLine("异常" + exception.ToString());
            }
        }



        /// <summary>
        /// 调用Acrobat COM 接口 将PDF文档转换为图片（服务器需要安装Adobe Acrobat DC 软件）
        /// 因为大多数的参数都有默认值，startPageNum默认值为1，endPageNum默认值为总页数，
        /// imageFormat默认值为ImageFormat.Jpeg，resolution默认值为1
        /// </summary>
        /// <param name="pdfInputPath">PDF文件路径</param>
        /// <param name="imageOutputPath">图片输出路径</param>
        /// <param name="imageName">图片的名字，不需要带扩展名</param>
        /// <param name="startPageNum">从PDF文档的第几页开始转换，默认值为1</param>
        /// <param name="endPageNum">从PDF文档的第几页开始停止转换，默认值为PDF总页数</param>
        /// <param name="imageFormat">设置所需图片格式</param>
        /// <param name="resolution">设置图片的分辨率，数字越大越清晰，默认值为1</param>
        public static void ConvertPDF2Image(string pdfInputPath, string imageOutputPath,
        string imageName, int startPageNum, int endPageNum, ImageFormat imageFormat, double resolution)
        {
            Acrobat.CAcroPDDoc pdfDoc = null;
            Acrobat.CAcroPDPage pdfPage = null;
            Acrobat.CAcroRect pdfRect = null;
            Acrobat.CAcroPoint pdfPoint = null;

            // Create the document (Can only create the AcroExch.PDDoc object using late-binding)
            // Note using VisualBasic helper functions, have to add reference to DLL
            pdfDoc = (Acrobat.CAcroPDDoc)Microsoft.VisualBasic.Interaction.CreateObject("AcroExch.PDDoc", "");

            // validate parameter
            if (!pdfDoc.Open(pdfInputPath)) { throw new FileNotFoundException(); }
            if (!Directory.Exists(imageOutputPath)) { Directory.CreateDirectory(imageOutputPath); }
            if (startPageNum <= 0) { startPageNum = 1; } if (endPageNum > pdfDoc.GetNumPages() || endPageNum <= 0) { endPageNum = pdfDoc.GetNumPages(); } if (startPageNum > endPageNum) { int tempPageNum = startPageNum; startPageNum = endPageNum; endPageNum = startPageNum; }
            if (imageFormat == null) { imageFormat = ImageFormat.Jpeg; }
            if (resolution <= 0) { resolution = 1; }

            // start to convert each page
            for (int i = startPageNum; i <= endPageNum; i++)
            {
                pdfPage = (Acrobat.CAcroPDPage)pdfDoc.AcquirePage(i - 1);
                pdfPoint = (Acrobat.CAcroPoint)pdfPage.GetSize();
                pdfRect = (Acrobat.CAcroRect)Microsoft.VisualBasic.Interaction.CreateObject("AcroExch.Rect", "");

                int imgWidth = (int)((double)pdfPoint.x * resolution);
                int imgHeight = (int)((double)pdfPoint.y * resolution);

                pdfRect.Left = 0;
                pdfRect.right = (short)imgWidth;
                pdfRect.Top = 0;
                pdfRect.bottom = (short)imgHeight;

                // Render to clipboard, scaled by 100 percent (ie. original size)
                // Even though we want a smaller image, better for us to scale in .NET
                // than Acrobat as it would greek out small text
                bool b = pdfPage.CopyToClipboard(pdfRect, 0, 0, (short)(100 * resolution));

                IDataObject clipboardData = Clipboard.GetDataObject();

                if (clipboardData.GetDataPresent(DataFormats.Bitmap))
                {
                    Bitmap pdfBitmap = (Bitmap)clipboardData.GetData(DataFormats.Bitmap);
                    pdfBitmap.Save(Path.Combine(imageOutputPath, imageName) + "_" + i.ToString(), imageFormat);
                    pdfBitmap.Dispose();
                }
            }

            pdfDoc.Close();
            Marshal.ReleaseComObject(pdfPage);
            Marshal.ReleaseComObject(pdfRect);
            Marshal.ReleaseComObject(pdfDoc);
            Marshal.ReleaseComObject(pdfPoint);
        }

        /// <summary>
        /// 调用Acrobat COM 接口 将PDF文档转换为图片（服务器需要安装Adobe Acrobat DC 软件） 
        /// </summary>
        [STAThread]
        private static void GetImgFromAcrobat()
        {
            string pdfInputPath = AppDomain.CurrentDomain.BaseDirectory + "test.pdf";
            string imageOutputPath = AppDomain.CurrentDomain.BaseDirectory + @"\Pdf2Img\";
            int startPageNum = 1;
            ImageFormat imageFormat = ImageFormat.Jpeg;
            int resolution = 1;
            Acrobat.CAcroPDDoc pdfDoc = null;
            Acrobat.CAcroPDPage pdfPage = null;
            Acrobat.CAcroRect pdfRect = null;
            Acrobat.CAcroPoint pdfPoint = null;

            // Create the document (Can only create the AcroExch.PDDoc object using late-binding)
            // Note using VisualBasic helper functions, have to add reference to DLL
            pdfDoc = (Acrobat.CAcroPDDoc)Microsoft.VisualBasic.Interaction.CreateObject("AcroExch.PDDoc", "");

            if (!pdfDoc.Open(pdfInputPath)) { throw new FileNotFoundException(); }
            if (!Directory.Exists(imageOutputPath)) { Directory.CreateDirectory(imageOutputPath); }
            int endPageNum = pdfDoc.GetNumPages();
            if (startPageNum <= 0) { startPageNum = 1; } if (endPageNum > pdfDoc.GetNumPages() || endPageNum <= 0) { endPageNum = pdfDoc.GetNumPages(); } if (startPageNum > endPageNum) { int tempPageNum = startPageNum; startPageNum = endPageNum; endPageNum = startPageNum; }
            if (imageFormat == null) { imageFormat = ImageFormat.Jpeg; }
            if (resolution <= 0) { resolution = 1; }

            // start to convert each page
            for (int i = startPageNum; i <= endPageNum; i++)
            {
                pdfPage = (Acrobat.CAcroPDPage)pdfDoc.AcquirePage(i - 1);
                pdfPoint = (Acrobat.CAcroPoint)pdfPage.GetSize();
                pdfRect = (Acrobat.CAcroRect)Microsoft.VisualBasic.Interaction.CreateObject("AcroExch.Rect", "");

                int imgWidth = (int)((double)pdfPoint.x * resolution);
                int imgHeight = (int)((double)pdfPoint.y * resolution);

                pdfRect.Left = 0;
                pdfRect.right = (short)imgWidth;
                pdfRect.Top = 0;
                pdfRect.bottom = (short)imgHeight;

                // Render to clipboard, scaled by 100 percent (ie. original size)
                // Even though we want a smaller image, better for us to scale in .NET
                // than Acrobat as it would greek out small text
                bool b = pdfPage.CopyToClipboard(pdfRect, 0, 0, (short)(100 * resolution));//复制到剪切板

                IDataObject clipboardData = Clipboard.GetDataObject();

                if (clipboardData.GetDataPresent(DataFormats.Bitmap))
                {
                    Bitmap pdfBitmap = (Bitmap)clipboardData.GetData(DataFormats.Bitmap);
                    string imtName = i.ToString() + ".jpg";
                    pdfBitmap.Save(imageOutputPath + imtName, imageFormat);
                    pdfBitmap.Dispose();
                    Console.WriteLine("生成图片" + imtName);

                }
            }

            pdfDoc.Close();
            Marshal.ReleaseComObject(pdfPage);
            Marshal.ReleaseComObject(pdfRect);
            Marshal.ReleaseComObject(pdfDoc);
            Marshal.ReleaseComObject(pdfPoint);
        }
    }
}

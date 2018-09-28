using System;
using System.Collections.Generic;
using System.Text;
using PDFLibNet;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
namespace pdf2png
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始生成图片");
            string pdfInputPath = @"F:\Pdf2Img\test.pdf";
            string imageOutputPath = @"F:\Pdf2Img\";
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
            Console.WriteLine("生成图片完成");
            Console.Read();

        }
    }
}

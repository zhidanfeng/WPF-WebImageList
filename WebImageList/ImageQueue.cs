using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading;
using System.IO;

namespace WebImageList
{
    /// <summary>
    /// 图片下载队列
    /// </summary>
    public static class ImageQueue
    {
        #region 辅助类别
        private class ImageQueueInfo
        {
            public Image image { get; set; }
            public String url { get; set; }
        }
        #endregion
        public delegate void ComplateDelegate(Image i, string u, BitmapImage b);
        public static event ComplateDelegate OnComplate;
        private static AutoResetEvent autoEvent;
        private static Queue<ImageQueueInfo> Stacks;
        static ImageQueue()
        {
            ImageQueue.Stacks = new Queue<ImageQueueInfo>();
            autoEvent = new AutoResetEvent(true);
            Thread t = new Thread(new ThreadStart(ImageQueue.DownloadImage));
            t.Name = "下载图片";
            t.IsBackground = true;
            t.Start();
        }
        private static void DownloadImage()
        {
            while (true)
            {
                ImageQueueInfo t = null;
                lock (ImageQueue.Stacks)
                {
                    if (ImageQueue.Stacks.Count > 0)
                    {
                        t = ImageQueue.Stacks.Dequeue();
                    }
                }
                if (t != null)
                {
                    Uri uri = new Uri(t.url);
                    BitmapImage image = null;
                    try
                    {
                        if ("http".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
                        {
                            //如果是HTTP下载文件
                            WebClient wc = new WebClient();
                            using (var ms = new MemoryStream(wc.DownloadData(uri)))
                            {
                                image = new BitmapImage();
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.StreamSource = ms;
                                image.EndInit();
                            }
                        }
                        else if ("file".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
                        {
                            using (var fs = new FileStream(t.url, FileMode.Open))
                            {
                                image = new BitmapImage();
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.StreamSource = fs;
                                image.EndInit();
                            }
                        }
                        if (image != null)
                        {
                            if (image.CanFreeze) image.Freeze();
                            t.image.Dispatcher.BeginInvoke(new Action<ImageQueueInfo, BitmapImage>((i, bmp) => 
                            {
                                if (ImageQueue.OnComplate != null)
                                {
                                    ImageQueue.OnComplate(i.image, i.url, bmp);
                                }
                            }),new Object[] { t, image });
                        }
                    }
                    catch(Exception e)
                    {
                        System.Windows.MessageBox.Show(e.Message);
                        continue;
                    }
                }
                if (ImageQueue.Stacks.Count > 0) continue;
                autoEvent.WaitOne();
            }
        }
        public static void Queue(Image img, String url)
        {
            if (String.IsNullOrEmpty(url)) return;
            lock (ImageQueue.Stacks)
            {
                ImageQueue.Stacks.Enqueue(new ImageQueueInfo { url = url, image = img });
                ImageQueue.autoEvent.Set();
            }
        }
    }
}

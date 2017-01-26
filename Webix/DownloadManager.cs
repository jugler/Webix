using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Webix
{
    class DownloadManager
    {
        private Cache cache;
        private  Downloader downloader;
        private CacheElement element;
        private static DownloadManager singleInstance;
        public static DownloadManager getInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new DownloadManager();
                
            }
            return singleInstance;
        }
        private DownloadManager()
        {
            this.cache = new Cache(1000);
            this.downloader = new Downloader();
        }
        public async Task getString(string urlToGet)
        {
            CacheElement elementInCache = cache.get(urlToGet);
            if (elementInCache == null)
            {
                await downloader.getString(urlToGet);

                CacheElement newElement = new CacheElement(urlToGet, downloader.DownloadedString);
                cache.addToCache(newElement);
                elementInCache = cache.get(urlToGet);
            }
            element = elementInCache;
        }


        public async Task getStream(string urlToGet)
        {
            CacheElement elementInCache = cache.get(urlToGet);
            if (elementInCache == null)
            {
                //IRandomAccessStreamWithContentType contentStream = downloader.getStream(urlToGet);
                await downloader.getStream(urlToGet);
                CacheElement newElement = new CacheElement(urlToGet, downloader.DownloadedStream);
                cache.addToCache(newElement);
                elementInCache = cache.get(urlToGet);
            }
            element = elementInCache;
        }


        public string getString()
        {
            return element.ContentString;
        }

        public BitmapImage getStream()
        {
            return element.ContentStream;
        }
    }
}
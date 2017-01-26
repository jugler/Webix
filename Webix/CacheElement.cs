using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Webix
{
    class CacheElement
    {
        private DateTime lastUsed;

        public DateTime LastUsed
        {
            get { return lastUsed; }
            set { lastUsed = value; }
        }

        private string urlOfElement;

        public string UrlOfElement
        {
            get { return urlOfElement; }
            set { urlOfElement = value; }
        }
        private CacheElementType typeOfElement;

        private CacheElementType TypeOfElement
        {
            get { return typeOfElement; }
            set { typeOfElement = value; }
        }

        private string contentString;

        public string ContentString
        {
            get
            {
                if (this.typeOfElement == CacheElementType.ElementTypeStream)
                {
                    throw new UnauthorizedAccessException("This cache element is of type stream not string");
                }
                return contentString;
            }
            set { contentString = value; }
        }
        private BitmapImage contentStream;

        public BitmapImage ContentStream
        {
            get
            {
                if (this.typeOfElement == CacheElementType.ElementTypeString)
                {
                    throw new UnauthorizedAccessException("This cache element is of type string not stream");
                }
                return contentStream;
            }
            set { contentStream = value; }
        }

        public CacheElement(string url, BitmapImage content)
        {
            this.typeOfElement = CacheElementType.ElementTypeStream;
            this.urlOfElement = url;
            this.contentStream = content;
            this.lastUsed = DateTime.Now;
        }

        public CacheElement(string url, string content)
        {
            this.typeOfElement = CacheElementType.ElementTypeString;
            this.urlOfElement = url;
            this.contentString = content;
        }

        enum CacheElementType
        {
            ElementTypeString,
            ElementTypeStream
        }

        public void Touch()
        {
            LastUsed = DateTime.Now;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Webix
{
    class Downloader
    {
        String downloadedString;

        public String DownloadedString
        {
            get { return downloadedString; }
            set { downloadedString = value; }
        }
        BitmapImage downloadedStream;

        public BitmapImage DownloadedStream
        {
            get { return downloadedStream; }
            set { downloadedStream = value; }
        }
        public async Task getString(string urlToGet) {
            Uri uriToGet = new Uri(urlToGet);
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            headers.UserAgent.ParseAdd("Mozilla/5.0");

            var res = await httpClient.GetAsync(uriToGet);

            string result = await res.Content.ReadAsStringAsync();

            downloadedString = result;
        }

        public async Task getStream(string urlToGet)
        {
            try
            {
                Uri downloadUri = new Uri(urlToGet);
                Uri realUri = new Uri("http://" + downloadUri.Host + downloadUri.PathAndQuery);
                RandomAccessStreamReference stream = RandomAccessStreamReference.CreateFromUri(realUri);
                IRandomAccessStream st = await stream.OpenReadAsync();
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(st);
                downloadedStream = bitmapImage;
            }
            catch
            {

                downloadedStream = null;
            }
        
        }
    }

    
}

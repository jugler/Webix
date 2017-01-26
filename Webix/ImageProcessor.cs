using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;


using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Net;
using System.IO;
using System.Net.Http;


namespace Webix
{
    class ImageProcessor
    {
        private List<string> _images = new List<string>();
        private BitmapImage _currentImage;
        private String _currentImageUrl;
        private String baseHostname;
        private DownloadManager downloadMngr;
        public BitmapImage CurrentImage
        {
            get { return _currentImage; }
            set { _currentImage = value; }
        }
        public List<string> Images
        {
            get { return _images; }
            set { _images = value; }
        }  
        public ImageProcessor()
        {
            downloadMngr = DownloadManager.getInstance();
        }

        public async Task setImage(string url, Image displayImage)
        {
            
            RandomAccessStreamReference stream = RandomAccessStreamReference.CreateFromUri(new Uri(url));
            IRandomAccessStreamWithContentType st = await stream.OpenReadAsync();
           
           /* await downloadMngr.getStream(url);
            IRandomAccessStreamWithContentType st = downloadMngr.getStream();
            * */
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(st);
            displayImage.Source = bitmapImage;
             
            displayImage.Width = bitmapImage.PixelWidth;
            displayImage.Height = bitmapImage.PixelHeight;

            displayImage.MaxHeight = bitmapImage.PixelHeight;
            displayImage.MaxWidth = bitmapImage.PixelWidth;
        }
        public async Task getImageFromUrl(string urlToGet)
        {
            try
            {

                await downloadMngr.getStream(urlToGet);
                BitmapImage bitmapImage = downloadMngr.getStream();
                _currentImage = bitmapImage;
            }
            catch 
            {
                //wasnt able to download get an image, no problem continue
                _currentImage = new BitmapImage();
            }
        }

        public async Task getLargestImage(string urlToGet,string extraTricksforSearch)
        {
            BitmapImage largest=new BitmapImage();
            string largestUrl="";
            await getImages(urlToGet, extraTricksforSearch);
            foreach (string img in Images)
            {
                _currentImage = null;
                _currentImageUrl = img;

                await getImageFromUrl(img);

                if (_currentImage != null)
                {
                    if (largest.PixelWidth * largest.PixelHeight < _currentImage.PixelHeight * _currentImage.PixelHeight)
                    {
                        //maybe largest image is not the comic?
                        //if larger but comes from a different host (picture add, except Dilbert)
                        //if larger but does not contain the word "comic" on the path (PhD comics fuck you)
                        if (isBetterOptionBigger(_currentImageUrl, largestUrl))
                        {
                            largestUrl = img;
                            largest = _currentImage;
                        }
                    }
                    else if (_currentImage.PixelHeight * _currentImage.PixelHeight > 0)//maybe largest image is not the comic
                    {
                        //if short one comes from the same host as the comic then short better
                        //if short one comes from the path "comic" PhD comics fuck you then short better
                        if (isBetterOption(_currentImageUrl, largestUrl))
                        {
                            largestUrl = img;
                            largest = _currentImage;
                        }
                    }
                }
                _currentImage = largest;
            }
        }

        //this is so hacky its gross, couldnt find other way to make it work, maybe with second algorithm
        private bool isBetterOptionBigger(string newUrl, string oldUrl)
        {
            if (oldUrl.Contains(baseHostname) && !newUrl.Contains(baseHostname))
            {
                return false;
            }
            if (!String.IsNullOrEmpty(oldUrl))
            {
                Uri uriTmp = new Uri(oldUrl);
                Uri uriTmp2 = new Uri(newUrl);
                if (uriTmp.PathAndQuery.Contains("comic") && !uriTmp2.PathAndQuery.Contains("comic"))
                {
                    return false;
                }
            }
            return true;
        }
        private bool isBetterOption(string newUrl, string oldUrl)
        {
            if (!oldUrl.Contains(baseHostname) && newUrl.Contains(baseHostname))
            {
                return true;
            }
            Uri uriTmp = new Uri(oldUrl);
            Uri uriTmp2 = new Uri(newUrl);
            if (!uriTmp.PathAndQuery.Contains("comic") && uriTmp2.PathAndQuery.Contains("comic"))
            {
                return true;
            }
            return false;
        }

        private string getCustomHostName(string HostName)
        {
            String customHost = HostName.Replace("www.", "");
            customHost = customHost.Replace(".com", "");
            return customHost;
        }

        public async Task getImages(string urlToGet,string extraTricksforSearch)
        {
            Uri uriToGet = new Uri(urlToGet);
            this.baseHostname = getCustomHostName(uriToGet.DnsSafeHost);
            
            await downloadMngr.getString(urlToGet);
            string result = downloadMngr.getString();
            
            TextBlock htmlCode = new TextBlock();
            htmlCode.MaxHeight = 600;
            
            string expForImg = "src=";
            int indexImage = result.IndexOf(expForImg,0);
            string imageWorker = "";
            int endofTag = -1;
            List<string> imageUrls = new List<string>();
            while (indexImage != -1)   //keep looking for images
            {
                string simpleQuote = result.Substring(indexImage+expForImg.Length, 1);
                if (simpleQuote == "\"")
                {
                    endofTag = result.IndexOf("\"", indexImage + expForImg.Length+1);
                    if (endofTag != -1)
                    {
                        imageWorker = result.Substring(indexImage + expForImg.Length + 1, endofTag - indexImage - expForImg.Length-1);
                        //is it an image?
                        //.png, .jpeg, .jpg, .gif, 

                        if (isAcceptedImage(imageWorker) || imageWorker.Contains(extraTricksforSearch))
                        {
                            imageUrls.Add(imageWorker);
                        }
                        if (imageWorker.Contains("barra"))
                        {
                            imageUrls.Add(imageWorker);
                        }
                    }
                }
                if (simpleQuote == "'")
                {
                    endofTag = result.IndexOf("\'", indexImage + expForImg.Length+1);
                    if (endofTag != -1)
                    {
                        imageWorker = result.Substring(indexImage + expForImg.Length + 1, endofTag - indexImage - expForImg.Length - 1);
                        //is it an image?
                        //.png, .jpeg, .jpg, .gif, 
                        if (isAcceptedImage(imageWorker))
                        {
                            imageUrls.Add(imageWorker);
                        }
                    }
                }
                else //because sometimes they dont quote the url...
                {
                    //look for the first thing that happens, close tag or a space
                    endofTag = result.IndexOf(" ", indexImage + expForImg.Length + 1); 
                    int endofTag2 = result.IndexOf(">", indexImage + expForImg.Length + 1);
                    if (endofTag < 0)
                    {
                        endofTag = endofTag2;
                    }
                    else if (endofTag2 > 0)
                    {
                        endofTag = (endofTag < endofTag2) ? endofTag : endofTag2;
                    }

                    if(endofTag != -1)//if an actual src tag was found
                    {
                        imageWorker = result.Substring(indexImage + expForImg.Length , endofTag - indexImage - expForImg.Length );
                        
                        if (isAcceptedImage(imageWorker))
                        {
                            imageUrls.Add(imageWorker);
                        }       
                    }
                }        
                indexImage = result.IndexOf(expForImg, indexImage + 1);
            }
           
            List<string> realImages = new List<string>();
            foreach (string urlI in imageUrls)
            {
                string realUrl = urlI;
                if (!realUrl.Contains("//"))
                {
                    //new Uri(urlToGet).AbsolutePath
                    Uri uriT = new Uri(urlToGet);
                    
                    string realPath = realUrl;
                    if (!String.IsNullOrEmpty(uriT.Query))
                    {
                        realPath = removeQueryCustom(uriT.OriginalString);//cause fuck the URI Scheme, need everything but the query, and file
                    }
                    realUrl = realPath + urlI;
                }
                if (!realImages.Contains(realUrl))
                {
                    realImages.Add(realUrl);
                }
            }
            _images = realImages;           
        }

        private bool isAcceptedImage(string imageName)
        {
            if (imageName.Contains(".png") || imageName.Contains(".jpg") || imageName.Contains(".jpeg") || imageName.Contains(".gif"))
            {
                return true;
            }
            return false;
        }

        //Because fuck the URI Scheme, need something to grab everything but the file+query of a url
        private string removeQueryCustom(string originalQuery)
        {
            int i = originalQuery.Length-1;
            for (; i > 0; i--)
            {
                if (originalQuery.ElementAt(i) == '/')
                {
                    break;
                }
            }
            return originalQuery.Substring(0, i+1);
        }

    }
}

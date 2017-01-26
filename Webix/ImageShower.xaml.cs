using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Webix
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ImageShower : Webix.Common.LayoutAwarePage
    {
        ImageProcessor imgP;
        Boolean imageLoaded = false;
        WebComic webComic;
        private const int MAX_RETRIES = 10;

       
        public ImageShower()
        {
            this.InitializeComponent();
            imgP = new ImageProcessor();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            webComic = e.Parameter as WebComic;
            pageTitle.Text = webComic.Name;
            loadComic();
        }

        private void autoZoomImage()
        {
            float zoomF = 0.0f;
            if (displayImage.Width > displayImage.Height)
            {
                zoomF = (float)scrollViewer.ViewportHeight / (float)displayImage.Height;              
            }
            else
            {
                zoomF = (float)scrollViewer.ViewportWidth / (float)displayImage.Width;               
            }
            zoomF = zoomF * 0.7f; //this hardcoded number looks good, might add a preference option for it....
            scrollViewer.ChangeView(0.0f, 0.0f, zoomF);
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void sizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (imageLoaded)
                autoZoomImage();
        }

        private void PreviousComic_click(object sender, RoutedEventArgs e)
        {         
            webComic.goBack=-1;
            loadComic();
        }

        public async void loadComic()
        {
            this.progressComicLoaded.IsActive = true;
            this.progressComicLoaded.Visibility = Visibility.Visible;
            DateTime dateToUse = webComic.comicDate;
            if (webComic.comicDate.AddDays(webComic.goBack).Date == (new DateTime()).Date)
            {
                //this.backButton.Visibility = Visibility.Collapsed;
                this.Next_Comic.IsEnabled = false;
                dateToUse = DateTime.Today;
                webComic.goBack = 0;
            }
            else
            {
                this.Next_Comic.IsEnabled = true;
            }

            await webComic.getComicfromUrl(webComic.calculateUrl(dateToUse.AddDays(webComic.goBack)), webComic.tricksForSearch);
            int retries = 0;
            while (webComic.Comic == null && retries++ < MAX_RETRIES)
            {
                webComic.goBack--;
                await webComic.getComicfromUrl(webComic.calculateUrl(dateToUse.AddDays(webComic.goBack)), webComic.tricksForSearch);
            }
            BitmapImage comicImage = webComic.Comic;
            displayImage.Source = comicImage;

            displayImage.Width = comicImage.PixelWidth;
            displayImage.Height = comicImage.PixelHeight;

            displayImage.MaxHeight = comicImage.PixelHeight;
            displayImage.MaxWidth = comicImage.PixelWidth;
            imageLoaded = true;
           
            mainSectionGrid.UpdateLayout();
            
            if (webComic.comicDate == webComic.latestComic)
            {
                this.Next_Comic.IsEnabled = false;
            }
            
            autoZoomImage();
            this.progressComicLoaded.IsActive = false;
            this.progressComicLoaded.Visibility = Visibility.Collapsed;
        }
        private void NextComic_click(object sender, RoutedEventArgs e)
        {
            webComic.goBack=+1;
            loadComic();
        }

    }
}

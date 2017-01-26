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
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Webix
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ComicCreatorNameLogo : Webix.Common.LayoutAwarePage
    {
        public ComicCreatorNameLogo()
        {
            this.InitializeComponent();
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            WebComic partialWebComic = e.Parameter as WebComic;
            ImageProcessor imgP = new ImageProcessor();
            await imgP.setImage(partialWebComic.WebComicUrl, webComicImageSelected);
            imageSelectedScroll.Width = webComicImageSelected.Width;
            imageSelectedScroll.Height = webComicImageSelected.Height;
            imageSelectedScroll.MaxHeight = webComicImageSelected.MaxHeight;
            imageSelectedScroll.MaxWidth = webComicImageSelected.MaxWidth;

            imageSelectedScroll.UpdateLayout();
            urlToGet.Text = partialWebComic.BasicUrl;
            imagePointerPressed(this, null);
        }

        private void imagePointerPressed(object sender, PointerRoutedEventArgs e)
        {
            WebComic wbSelected = new WebComic();
            wbSelected.WebComicUrl = ((Image)sender).Name;
            wbSelected.UrlProvided = urlToGet.Text;
            if (this.Frame != null)
            {
                //this.Frame.Navigate(typeof(ComicCreatorNameLogo), wbSelected);
            }
            //imageSelected.Text = ((Image)sender).Name;
        }

        private async void getImages_Click(object sender, RoutedEventArgs e)
        {
            imageGrid.Children.Clear();
            string url = urlToGet.Text;
            ImageProcessor imgP = new ImageProcessor();
            List<string> images = new List<string>();
            await imgP.getImages(url,null);
            int i = 0;
            images = imgP.Images;

            foreach (string img in images)
            {
                Image tmpImage = new Image();
                tmpImage.Name = img;
                tmpImage.PointerPressed += imagePointerPressed;

                try
                {
                    await imgP.setImage(img, tmpImage);
                }
                catch 
                {
                    continue;
                }
                RowDefinition rowD = new RowDefinition();

                imageGrid.RowDefinitions.Add(rowD);

                Grid.SetRow(tmpImage, i);
                imageGrid.Children.Add(tmpImage);
                i++;
            }
            
            
        }
    }
}

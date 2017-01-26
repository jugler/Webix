using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Runtime.Serialization.Json;

using System.Xml;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.Core;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Webix
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Webix.Common.LayoutAwarePage
    {
        List<WebComic> comicsLoaded = new List<WebComic>();
        
        public async Task readDefaultComics()
        {
            string defaultComicsFile = @"Assets\defaultWebComics.txt";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(defaultComicsFile);

            Stream comicsFileStream = await file.OpenStreamForReadAsync();
            StreamReader comicsFileReader = new StreamReader(comicsFileStream);
            string line;
            while ((line = comicsFileReader.ReadLine()) != null)
            {
                WebComic tmp = WebComic.deSerialize(line);
                comicsLoaded.Add(tmp);
                
            }
            //read Roaming comics not supported anymore but might be usefull later.
            try { 
            Windows.Storage.StorageFolder roamingFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;

            StorageFile comicsFile = await roamingFolder.GetFileAsync("customComics.txt");
            IList<String> jsonContents = await FileIO.ReadLinesAsync(comicsFile);
            foreach (String roamedComic in jsonContents)
            {
                if (roamedComic.Length > 0)
                {
                    WebComic tmp = WebComic.deSerialize(roamedComic);
                    comicsLoaded.Add(tmp);
                }
            }

            }
            catch (FileNotFoundException)
            {
                //No roaming file, no problemo
            }
            
        }
        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += Page_OrientationChanged;          
        }

        private void Page_OrientationChanged(object sender, WindowSizeChangedEventArgs e)
        {
            buttonsGridView.Items.Clear();
            loadComicPanels();
            buttonsGridView.UpdateLayout();
        }


        private async void loadComicPanels()
        {
            //There has to be a better way to refactor all this shit

            this.progressLogosLoaded.IsActive = true;
            this.progressLogosLoaded.Visibility = Visibility.Visible;
            ImageProcessor logoProcessor = new ImageProcessor();
            Thickness margin = new Thickness();
            margin.Left = 0;
            margin.Right = 0;
            margin.Top = 0;
            margin.Bottom = 0;
            buttonsGridView.Margin = margin;


            float scaleWidth = (float)(Window.Current.Bounds.Width - 160) / 3.0f;
            float scaleHeight = (float)Window.Current.Bounds.Height / 4.0f;
            float scaleText = (float)scaleHeight * .1f;
            foreach (WebComic comic in comicsLoaded)
            {
                float scale = 1.0f;
                if (comic.isReleasedToday())
                {
                    scale = 0.84f;
                }
                StackPanel comicPanel = new StackPanel();
                comicPanel.Orientation = Orientation.Vertical;

                comicPanel.Margin = margin;
                comicPanel.Width = scaleWidth;
                comicPanel.Height = scaleHeight;

                Border borderImage = new Border();
                SolidColorBrush brushSolid = new SolidColorBrush(Windows.UI.Colors.DarkGray);
                borderImage.Background = brushSolid;
                Image comicLogo = new Image();
                comicLogo.Width = scaleWidth;
                comicLogo.Height = scaleHeight;

                comicLogo.VerticalAlignment = VerticalAlignment.Stretch;
                comicLogo.HorizontalAlignment = HorizontalAlignment.Stretch;

                StackPanel textPanel = new StackPanel();
                textPanel.Orientation = Orientation.Horizontal;
                textPanel.Margin = margin;
                textPanel.Width = scaleWidth;
                textPanel.Height = scaleHeight * .15;

                Border borderText = new Border();
                borderText.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
                borderText.Opacity = 0.5;

                TextBlock comicName = new TextBlock();
                comicPanel.Tag = comic;
                comicName.Text = comic.Name;
                comicName.FontWeight = Windows.UI.Text.FontWeights.Bold;
                comicName.Width = scaleWidth * scale;
                comicName.Height = scaleHeight * .15;

                Border borderTextNew = new Border();
                borderTextNew.Background = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 25, 153, 0));
                borderTextNew.Opacity = 1;

                TextBlock comicNew = new TextBlock();
                comicNew.Text = "New!";
                comicNew.Width = scaleWidth * (1 - scale);
                comicNew.Height = scaleHeight * .15;
                comicNew.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                comicNew.FontWeight = Windows.UI.Text.FontWeights.Bold;
                comicNew.FontSize = scaleText;
                comicNew.TextAlignment = TextAlignment.Right;

                comicPanel.PointerReleased += LoadComic_click;
                comicPanel.PointerPressed += pointerPressed_click;

                borderImage.Child = comicLogo;
                borderText.Child = comicName;
                borderTextNew.Child = comicNew;

                textPanel.Children.Add(borderText);
                textPanel.Children.Add(borderTextNew);

                comicPanel.Children.Add(textPanel);
                comicPanel.Children.Add(borderImage);

                comicPanel.PointerEntered += new PointerEventHandler(bEntered_PointerEntered);
                comicPanel.PointerExited += new PointerEventHandler(bExited_PointerExited);
                buttonsGridView.Items.Add(comicPanel);

            }
            foreach (StackPanel comicPanel in buttonsGridView.Items)
            {
                WebComic comic = (WebComic)comicPanel.Tag;
                Border borderImg = (Border)comicPanel.Children[1];
                Image comicLogo = (Image)(borderImg.Child);
                await logoProcessor.getImageFromUrl(comic.LogoUrl);
                comicLogo.Source = logoProcessor.CurrentImage;
            }
            this.progressLogosLoaded.IsActive = false;
            this.progressLogosLoaded.Visibility = Visibility.Collapsed;
           
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            await readDefaultComics();
            loadComicPanels();  
           // listOfComicsScroll.Content = comicsButtons;
            
        }
        public void bEntered_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            StackPanel panel = (StackPanel)sender;
            panel.Opacity = panel.Opacity - .50;
           

        }
        public void bExited_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            StackPanel panel = (StackPanel)sender;
            panel.Opacity = panel.Opacity+ .50;

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
        bool pressed = false;
        private void pointerPressed_click(object sender, PointerRoutedEventArgs e)
        {
            ((StackPanel)sender).CapturePointer(e.Pointer);
            pressed = true;
        }
        private void LoadComic_click(object sender, RoutedEventArgs e)
        {
            if (pressed)
            {
                WebComic selected = (WebComic)(sender as StackPanel).Tag;
                if (this.Frame != null)
                {
                    this.Frame.Navigate(typeof(ImageShower), selected);
                }
            }
            pressed = false;
        }



        private void findComic_click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CustomComic));
                //this.Frame.Navigate(typeof(ItemsPage1));
            }
        }


        private void jsonFile_click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(CustomComicsJSON),comicsLoaded);
                //this.Frame.Navigate(typeof(ItemsPage1));
            }
        }

        private async void deleteRoaming_click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                Windows.Storage.StorageFolder roamingFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
                string jsonContents = "";
                StorageFile customComics = await roamingFolder.CreateFileAsync("customComics.txt", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(customComics, jsonContents);
            }
        }

        void InitHandlers()
        {
            ApplicationData.Current.DataChanged += new TypedEventHandler<ApplicationData, object>(DataChangeHandler);
        }

        void DataChangeHandler(Windows.Storage.ApplicationData appData, object o)
        {
            // TODO: Refresh your data
        }


      
    }
}

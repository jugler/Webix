using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class CustomComic : Webix.Common.LayoutAwarePage
    {
        WebComic newWebComic = new WebComic();
        

        public CustomComic()
        {
            this.InitializeComponent();
            newWebComic.BasedOnPattern = true;
        
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

        private void comicNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            this.comicNameResultText.Text = textBox.Text;
            this.newWebComic.Name = textBox.Text;
        }

        private void comicBaseUrlTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            this.newWebComic.BasicUrl = textBox.Text;
            if (newWebComic.ReleaseDay != null)
            {
                loadComic();
            }
        }

        private async void loadLogo()
        {
            ImageProcessor logoProcessor = new ImageProcessor();
            await logoProcessor.getImageFromUrl(this.newWebComic.LogoUrl);
            this.comicLogoImageResult.Source = logoProcessor.CurrentImage;
        }
        private async void loadComic()
        {
             this.statusLog.Visibility = Visibility.Collapsed;

             this.newWebComic.DateSeed = this.comicDateSeedDate.Date.Date;
            WebComic webComicTemp = new WebComic();
            webComicTemp.BasicUrl = newWebComic.BasicUrl;
            webComicTemp.BasedOnPattern = newWebComic.BasedOnPattern;
            webComicTemp.ReleaseDay = newWebComic.ReleaseDay;
            webComicTemp.SeedCounter = newWebComic.SeedCounter;
            webComicTemp.DateSeed = newWebComic.DateSeed;

            if (!newWebComic.BasedOnPattern)
            {
                
                if (newWebComic.SeedCounter == 0)
                {
                    this.statusLog.Text = "Webcomic based on counter need SeedCounter info";
                    this.statusLog.Visibility = Visibility.Visible;
                    return;
                }
                webComicTemp.SeedCounter = newWebComic.SeedCounter;
                webComicTemp.DateSeed = newWebComic.DateSeed;
            }
            this.progressBar.IsEnabled = true;
            this.progressBar.Visibility= Visibility.Visible;
            try
            {
                DateTime dateToUse = DateTime.Today;
                await webComicTemp.getComicfromUrl(webComicTemp.calculateUrl(dateToUse), webComicTemp.tricksForSearch);
                BitmapImage comicImage = webComicTemp.Comic;
                comicImageResult.Source = comicImage;

                comicImageResult.Width = comicImage.PixelWidth;
                comicImageResult.Height = comicImage.PixelHeight;

                this.progressBar.IsEnabled = false;
                this.progressBar.Visibility = Visibility.Collapsed;
                comicImageResult.MaxHeight = comicImage.PixelHeight;
                comicImageResult.MaxWidth = comicImage.PixelWidth;


                this.UpdateLayout();
            }
            catch (Exception)
            {
                this.progressBar.IsEnabled = false;
                this.progressBar.Visibility = Visibility.Collapsed;
                this.statusLog.Text = "Could not process the URL provided";
                this.statusLog.Visibility = Visibility.Visible;
            }
        }

        private void comicReleasedDaysCombo_LostFocus(object sender, RoutedEventArgs e)
        {

            List<DayOfWeek> releasedDays = new List<DayOfWeek>();
            for (int indexSelection = 0; indexSelection < comicReleasedDaysCombo.SelectedItems.Count; indexSelection++)
            {
                ListBoxItem selectedItem = (ListBoxItem)comicReleasedDaysCombo.SelectedItems[indexSelection];
                switch ((String)selectedItem.Content)
                {
                    case "Monday":
                        releasedDays.Add(DayOfWeek.Monday);
                        break;
                    case "Tuesday":
                        releasedDays.Add(DayOfWeek.Tuesday);
                        break;
                    case "Wednesday":
                        releasedDays.Add(DayOfWeek.Wednesday);
                        break;
                    case "Thursday":
                        releasedDays.Add(DayOfWeek.Thursday);
                        break;
                    case "Friday":
                        releasedDays.Add(DayOfWeek.Friday);
                        break;
                    case "Saturday":
                        releasedDays.Add(DayOfWeek.Saturday);
                        break;
                    case "Sunday":
                        releasedDays.Add(DayOfWeek.Sunday);
                        break;
                }
                
            }
            this.newWebComic.ReleaseDay = releasedDays;
            if (this.newWebComic.BasicUrl != null)
            {
                loadComic();
            }
        }

        private void comicReleasedDaysCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comicReleasedDaysCombo_LostFocus(sender, null);
        }

        private void comicReleasedDaysCombo_Event(object sender, DependencyPropertyChangedEventArgs e)
        {
            comicReleasedDaysCombo_LostFocus(sender, null);
        }

        private void logoUrlTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            this.newWebComic.LogoUrl = textBox.Text;
            loadLogo();
        }

        private void comicDateSeedDate_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            loadComic();
        }

        private void comicBasedOnPatternCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.newWebComic.BasedOnPattern = false;
        }

        private void comicBasedOnPatternCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.newWebComic.BasedOnPattern = true;
        }




        private void comicSeedCounterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;
                this.newWebComic.SeedCounter = Int32.Parse(textBox.Text);
                if (this.newWebComic.BasicUrl != null & this.newWebComic.ReleaseDay != null)
                {
                    loadComic();
                }
            }
            catch (Exception)
            {
                this.statusLog.Text = "Invalid Seed Counter, needs to be a number";
            }
        }

        private async void saveDetails(object sender, RoutedEventArgs e)
        {
           string newWebComicJson = newWebComic.serialize();
           await savetoRoamingComics(newWebComicJson);
           this.GoBack(sender, e);
        }


        public async Task savetoRoamingComics(string comicJson)
        {
            
            Windows.Storage.StorageFolder roamingFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
            string jsonContents = "";
            try
            {
                StorageFile sampleFile = await roamingFolder.GetFileAsync("customComics.txt");
                jsonContents = await FileIO.ReadTextAsync(sampleFile);
            }
            catch (FileNotFoundException) { }
            
            jsonContents = jsonContents + "\n" + comicJson;
            StorageFile customComics = await roamingFolder.CreateFileAsync("customComics.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(customComics, jsonContents);
           
            
        }


        
    }


}
namespace VisibilityTest
{
    public class BooleanToVisibilityConverter : IValueConverter
    {

        public Object Convert(Object value, Type targetType, Object parameter, string culture)
        {
            if (targetType == typeof(Visibility))
            {
                var visible = (Boolean)value;
                if (parameter != null) { 
                    var negate = (String)parameter;
                    if (negate=="true")
                        visible = !visible;
                }
                if (InvertVisibility)
                    visible = !visible;
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, string culture)
        {
            throw new InvalidOperationException("Converter cannot convert back.");
        }

        public Boolean InvertVisibility { get; set; }

    }

}

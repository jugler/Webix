using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;



namespace Webix
{
    //Yeah dont touch this class or you will fuck up all the JSON serialized objects...
    [DataContract]
    public sealed class WebComic
    {
       
        public WebComic()
        {
            this.tricksForSearch = this.ToString();
        }
        [DataMember]
        private string _webComicUrl;

        [DataMember]
        public string WebComicUrl
        {
            get { return _webComicUrl; }
            set { _webComicUrl = value; }
        }
        [DataMember]
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        [DataMember]
        private string _urlProvided;

        public string UrlProvided
        {
            get {  return _urlProvided; }
            set { _urlProvided = value; this.getSimpleUrl(); }
        }
        [DataMember]
        private string _logoUrl;

        public string LogoUrl
        {
            get { return _logoUrl; }
            set { _logoUrl = value; }
        }

        [DataMember]
        private string _basicUrl;

        public string BasicUrl
        {
            get { return _basicUrl; }
            set { _basicUrl = value; }
        }

        [DataMember]
        public DateTime comicDate
        {
            get;
            set;
        }

        [DataMember]
        public int goBack
        {
            get;
            set;
        }
        [DataMember]
        public string tricksForSearch { get; set; }

        [DataMember]
        public DateTime latestComic { get; set; }

        [DataMember]
        public string First { get; set; }

        [DataMember]
        public bool BasedOnPattern { get; set; }

        [DataMember]
        public DateTime DateSeed { get; set; }

        [DataMember]
        public int SeedCounter { get; set; }


        [DataMember]
        private string _comicPattern;

      
        public string ComicPattern 
        {
            get { return _comicPattern; }
            set { _comicPattern = value; }
        }

        [DataMember]
        private List<DayOfWeek> releasedDays;
        
        [DataMember]
        public List<DayOfWeek> ReleaseDay { get { return releasedDays; } set { releasedDays = value; } }
        public int getSizeReleased()
        {
            return releasedDays.Count;
        }

        [DataMember]
        public int Position
        {
            get;
            set;
        }
        
        public void setReleasedDays(params DayOfWeek[] days)
        {
            releasedDays = new List<DayOfWeek>();
            foreach (DayOfWeek day in days)
                releasedDays.Add(day);
        }

        [Obsolete("comics dont follow their own pattern")]
        public string calculateComicUrl(DateTime dayToCalculate)
        {
            if (this.releasedDays.Contains(dayToCalculate.DayOfWeek))
            {
                return String.Format(this.ComicPattern, dayToCalculate);
            }
            return calculateComicUrl(dayToCalculate.AddDays(-1.0));

        }

        private void getSimpleUrl()
        {
            string worker = _urlProvided;
            int indexOfDotCom = worker.IndexOf("/", ("http://").Length);
            if (indexOfDotCom != -1)
            {
                worker = worker.Substring(0, indexOfDotCom);
            }
            this.BasicUrl = worker;
        }

        public string calculateUrl(DateTime dateRequested) 
        {
            while (!this.releasedDays.Contains(dateRequested.DayOfWeek))
            {
                if (goBack==0)
                    dateRequested = dateRequested.AddDays(-1);
                else
                    dateRequested = dateRequested.AddDays(goBack);
            }
            if (goBack == 0)
            {
                this.latestComic = dateRequested;
            }
            if (comicDate.Date != dateRequested.Date)
            {
                if (this.BasedOnPattern)
                {
                    string finalUrl = this.BasicUrl;
                    if (dateRequested.Date > DateTime.Today.Date) //requesting comics in the future
                    {
                        finalUrl = String.Format(this.BasicUrl, comicDate);
                        return finalUrl;
                    }

                    if (this.releasedDays.Contains(dateRequested.DayOfWeek))
                    {
                        finalUrl = String.Format(this.BasicUrl, dateRequested);
                        if (comicDate.Date != dateRequested.Date)
                        {
                            this.comicDate = dateRequested;
                            return finalUrl;
                        }
                    }
                    return calculateUrl(dateRequested.AddDays(goBack));
                }
                else
                {
                    return calculateUrlCounterBased(dateRequested);
                }
            }
            else
            {
                return calculateUrl(dateRequested.AddDays(goBack));
            }
            

        }


        public string calculateUrlCounterBased(DateTime dateRequested)
        {
            int SeedCounter1 = this.SeedCounter;
            for (DateTime iterator = DateSeed.AddDays(1); iterator <= dateRequested; iterator=iterator.AddDays(1)) {
                if (this.releasedDays.Contains(iterator.DayOfWeek))
                {
                    SeedCounter1 = SeedCounter1 + 1;
                    comicDate = iterator;
                }
            }
            if (dateRequested < DateSeed)
            {
                
                for (DateTime iterator = DateSeed.AddDays(-1); iterator >= dateRequested; iterator = iterator.AddDays(-1))
                {
                    if (this.releasedDays.Contains(iterator.DayOfWeek))
                    {
                        SeedCounter1 = SeedCounter1 - 1;
                        comicDate = iterator;
                    }
                }

            }
            if (dateRequested.Date == DateSeed.Date)
            {
                comicDate = DateSeed;
                SeedCounter1 = this.SeedCounter;
            }
            string finalUrl = this.BasicUrl.Replace("%COUNTER%", SeedCounter1.ToString());
            return finalUrl;
        }

        public async Task getComicfromUrl(string url, string extraTricksforSearch)
        {
            ImageProcessor imgP = new ImageProcessor();
            await imgP.getLargestImage(url, extraTricksforSearch);
            this.Comic = imgP.CurrentImage;
            //return imgP.CurrentImage;
            
        }





        public Windows.UI.Xaml.Media.Imaging.BitmapImage Comic { get; set; }


        public static WebComic deSerialize(string jsonString)
        {
                       
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(WebComic));
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            MemoryStream stream = new MemoryStream(byteArray);
            Object webComicObject = jsonSerializer.ReadObject(stream);
            return (WebComic)webComicObject;
            
        }

        public string serialize()
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(WebComic));
            MemoryStream ms = new MemoryStream();
            jsonSerializer.WriteObject(ms, this);
            byte[] streamArray = ms.ToArray();
            string _jsonString = Encoding.UTF8.GetString(streamArray, 0, streamArray.Count());
            return _jsonString;
        }

        public bool isReleasedToday()
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            return releasedDays.Contains(today);
        }
    }


    

}

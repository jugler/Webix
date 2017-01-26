using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Webix
{
    class Cache
    {
        private Dictionary<String, int> cacheUrls;
        private CacheElement[] cacheElements;
        
        public Cache(int size)
        {
            this.cacheUrls = new Dictionary<String,int>();
            this.cacheElements = new CacheElement[size];
        }


        public CacheElement get(string urlToGet)
        {
            if (cacheUrls.ContainsKey(urlToGet))
            {
                int cacheElementIndex = cacheUrls[urlToGet];
                cacheElements[cacheElementIndex].Touch();
                return cacheElements[cacheElementIndex];
            }
            else
            {
                return null;
            }
        }

        public void addToCache(CacheElement elementToAdd)
        {
            if (cacheUrls.ContainsKey(elementToAdd.UrlOfElement))
            {
                return;
            }
            //is there space in the cache?
            DateTime lastAccessed = DateTime.Now;
            int lruElement = -1;
            for (int cacheIndex = 0; cacheIndex < cacheElements.Length; cacheIndex++)
            {
                if (cacheElements[cacheIndex] == null)
                {
                    //add element to cache
                    cacheElements[cacheIndex] = elementToAdd;
                    cacheUrls.Add(elementToAdd.UrlOfElement,cacheIndex);
                    return;
                }
                else
                {
                    if (cacheElements[cacheIndex].LastUsed < lastAccessed)
                    {
                        lastAccessed = cacheElements[cacheIndex].LastUsed;
                        lruElement = cacheIndex;
                    }
                   
                }
            }

            //no space in cache? eliminate LRU element, add new one
            CacheElement toEliminate = cacheElements[lruElement];
            cacheUrls.Remove(toEliminate.UrlOfElement);
            cacheElements[lruElement] = null;

            cacheElements[lruElement] = elementToAdd;
            cacheUrls.Add(elementToAdd.UrlOfElement, lruElement);

            return;
        }
    }
}

using System;

namespace MCRatings
{
    // status tracker for several ProgressBar tasks
    public class ProgressInfo
    {
        public string title;
        public string subtitle;
        public int currentItem;
        public int totalItems;
        public DateTime startTime;
        public bool cancelled;
        public bool paused;
        public bool noCache;
        public int success;
        public int fail;
        public int skip;
        public object args;
        public bool result;
        public bool canCancel = true;
        public bool canOverwrite = false;
        public bool useAltTitle = false;

        public Action RefreshHandler;

        private DateTime lastUpdate;


        public ProgressInfo(string Title, int items = 0)
        {
            startTime = DateTime.Now;
            title = Title;
            totalItems = items;
        }

        public void Update(bool immediate = true)
        {
            if (immediate || lastUpdate.AddMilliseconds(100) < DateTime.Now)
            {
                RefreshHandler?.Invoke();
                if (!immediate) lastUpdate = DateTime.Now;
            }
        }
    }
}

using UnityEngine;
using InfoWatch.Main;
using DevGorillaLib.Objects;
using System;

namespace InfoWatch.Scripts
{
    public class PageManager
    {
        // Is this a bad way to do a page system?

        public event EventHandler PageChange;
        readonly int Pages = 2;
        public int CurrentPage = 0;

        /// <summary>
        /// Increments the page. Does nothing if attempting to go to a nonexistant page or if the page was changed too recently.
        /// </summary>
        public void PageUp()
        {
            if (CurrentPage + 1 > Pages) return;
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.Hat);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.Badge);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.Face);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.LeftHand);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.RightHand);
            Plugin.instance.watch.SetColourSwatch(Color.clear);
            CurrentPage++;
            PageChange?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Decrements the page. Does nothing if attempting to go to a nonexistant page or if the page was changed too recently.
        /// </summary>
        public void PageDown()
        {
            if (CurrentPage - 1 < 0) return;
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.Hat);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.Badge);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.Face);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.LeftHand);
            Plugin.instance.watch.SetImage(null, DummyWatch.ImageType.RightHand);
            Plugin.instance.watch.SetColourSwatch(Color.clear);
            CurrentPage--;
            PageChange?.Invoke(this, EventArgs.Empty);
        }
    }
}

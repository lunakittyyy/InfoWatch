using InfoWatch.Main;
using System;

namespace InfoWatch.Scripts
{
    internal class WatchButton : GorillaPressableButton
    {
        public Action onPressed;

        public override void ButtonActivation()
        {
            base.ButtonActivation();
            onPressed();
        }
    }
}

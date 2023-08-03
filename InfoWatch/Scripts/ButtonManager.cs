using InfoWatch.Main;
using System;

namespace InfoWatch.Scripts
{
    internal class WatchButton : GorillaPressableButton
    {
        public Action onPressed;

        public override void ButtonActivationWithHand(bool isLeftHand)
        {
            base.ButtonActivationWithHand(isLeftHand);
            if (isLeftHand) return;
            onPressed();
        }
    }
}

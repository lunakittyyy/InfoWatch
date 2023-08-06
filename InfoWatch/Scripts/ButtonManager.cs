using InfoWatch.Main;
using System;

namespace InfoWatch.Scripts
{
    // TODO: Use our own implementation of GorillaPressableButton.
    // As of right now, the click sounds are networked
    // and play regardless of which hand you press with
    // and therefore sometimes plays even when the page doesn't switch
    // To fix this we would have to remake GorillaPressableButton ourselves
    // Maybe we could override some of the behavior with a Harmony prefix to fix the hand thing
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

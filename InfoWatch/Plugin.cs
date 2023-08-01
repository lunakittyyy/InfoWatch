using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using Utilla;
using DevGorillaLib.Objects;
using InfoWatch.Scripts;
using Photon.Pun;
using Photon.Voice.Unity;
using GorillaNetworking;
using DevGorillaLib.Utils;

namespace InfoWatch.Main
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        // core
        public static Plugin instance;
        bool WatchActive;
        public DummyWatch watch;
        string TempText;
        ConfigEntry<bool> TwentyFourHr;
        PageManager pageManager;
        TimeSpan playTime;
        bool InitDone = false;

        // network stuff
        Recorder VoiceRecorder;
        
        // icons
        Sprite SpeakerSprite;
        Sprite OneBarSprite;
        Sprite TwoBarSprite;
        Sprite ThreeBarSprite;
        Sprite FourBarSprite;
        Sprite usSprite;
        Sprite uswSprite;
        Sprite euSprite;
        
        async void Start()
        {

            // streams
            Stream speakerstr = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.speaker.png");
            Stream pingbar1 = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.pingbar1.png");
            Stream pingbar2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.pingbar2.png");
            Stream pingbar3 = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.pingbar3.png");
            Stream pingbar4 = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.pingbar4.png");
            Stream us = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.us.png");
            Stream usw = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.usw.png");
            Stream eu = Assembly.GetExecutingAssembly().GetManifestResourceStream("InfoWatch.Resources.eu.png");
            // byte arrays
            byte[] speakerBytes = new byte[speakerstr.Length];
            byte[] OneBarBytes = new byte[pingbar1.Length];
            byte[] TwoBarBytes = new byte[pingbar2.Length];
            byte[] ThreeBarBytes = new byte[pingbar3.Length];
            byte[] FourBarBytes = new byte[pingbar4.Length];
            byte[] usBytes = new byte[us.Length];
            byte[] uswBytes = new byte[usw.Length];
            byte[] euBytes = new byte[eu.Length];
            // reading
            await speakerstr.ReadAsync(speakerBytes, 0, speakerBytes.Length);
            await pingbar1.ReadAsync(OneBarBytes, 0, OneBarBytes.Length);
            await pingbar2.ReadAsync(TwoBarBytes, 0, TwoBarBytes.Length);
            await pingbar3.ReadAsync(ThreeBarBytes, 0, ThreeBarBytes.Length);
            await pingbar4.ReadAsync(FourBarBytes, 0, FourBarBytes.Length);
            await us.ReadAsync(usBytes, 0, usBytes.Length);
            await usw.ReadAsync(uswBytes, 0, uswBytes.Length);
            await eu.ReadAsync(euBytes, 0, euBytes.Length);
            // sprite creation
            SpeakerSprite = SpriteTools.CreateSpriteFromByteArray(speakerBytes, "speaker", 512, 512);
            OneBarSprite = SpriteTools.CreateSpriteFromByteArray(OneBarBytes, "onebar", 8, 8);
            TwoBarSprite = SpriteTools.CreateSpriteFromByteArray(TwoBarBytes, "twobar", 8, 8);
            ThreeBarSprite = SpriteTools.CreateSpriteFromByteArray(ThreeBarBytes, "threebar", 8, 8);
            FourBarSprite = SpriteTools.CreateSpriteFromByteArray(FourBarBytes, "fourbar", 8, 8);
            usSprite = SpriteTools.CreateSpriteFromByteArray(usBytes, "us", 9, 9);
            uswSprite = SpriteTools.CreateSpriteFromByteArray(uswBytes, "usw", 13, 13);
            euSprite = SpriteTools.CreateSpriteFromByteArray(euBytes, "eu", 9, 9);
            // config
            ConfigFile customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "InfoWatch.cfg"), true);
            TwentyFourHr = customFile.Bind("Time", "24-Hour Time", false, "Use 24-hour time instead of 12.");

            pageManager = new PageManager();
            if (instance == null) instance = this;

            Utilla.Events.RoomJoined += RoomJoined;
            Utilla.Events.RoomLeft += RoomLeft;
            Utilla.Events.GameInitialized += Init;
            pageManager.PageChange += PageChange;
        }

        void RoomJoined(object sender, Events.RoomJoinedArgs e)
        {
            if (e.Gamemode.Contains("HUNT") && WatchActive) { WatchDestroy(); }
            else if (!WatchActive) { WatchCreate(); }
            VoiceRecorder = PhotonNetworkController.Instance.GetComponent<Recorder>();
            RegionEval();
        }

        void RoomLeft(object sender, EventArgs e)
        {
            watch.SetImage(null, DummyWatch.ImageType.LeftHand);
            watch.SetImage(null, DummyWatch.ImageType.RightHand);
            watch.SetImage(null, DummyWatch.ImageType.Badge);
        }

        void Init(object sender, EventArgs e) { InitDone = true; WatchCreate(); }

        void Update()
        {
            if (WatchActive && InitDone)
            {
                if (InputUtils.LeftAnalogStick.x > 0.75) pageManager.PageUp();
                if (InputUtils.LeftAnalogStick.x < -0.75) pageManager.PageDown();
                switch (pageManager.CurrentPage)
                {
                    case 0:
                        playTime = DateTime.Now - Process.GetCurrentProcess().StartTime;
                        if (TwentyFourHr.Value)
                        {
                            TempText =
                                $"{DateTime.Now:H:mm}\n" +
                                $"SESSION:{new TimeSpanRounder.RoundedTimeSpan(playTime.Ticks, 0).ToString().Substring(0, 5)}";
                        }
                        else
                        {
                            TempText =
                                $"{DateTime.Now:h:mmtt}\n" +
                                $"SESSION:{new TimeSpanRounder.RoundedTimeSpan(playTime.Ticks, 0).ToString().Substring(0, 5)}";
                        }


                        if (PhotonNetwork.InRoom)
                        {
                            if (GorillaGameManager.instance is GorillaTagManager tag && tag.currentInfected.Count > 0)
                            {
                                TempText += $"\n{tag.currentInfected.Count}/{PhotonNetwork.PlayerList.Length} TAGGED";
                            }
                            else
                            {
                                TempText += $"\n{PhotonNetwork.PlayerList.Length} PLAYERS";
                            }

                            if (VoiceRecorder != null && VoiceRecorder.IsCurrentlyTransmitting) { watch.SetImage(SpeakerSprite, DummyWatch.ImageType.Badge); }
                            else { watch.SetImage(null, DummyWatch.ImageType.Badge); }

                            int ping = PhotonNetwork.GetPing();
                            if (0 <= ping && ping <= 30)
                            {
                                watch.SetImage(FourBarSprite, DummyWatch.ImageType.LeftHand);
                            }
                            else if (30 <= ping && ping <= 60)
                            {
                                watch.SetImage(ThreeBarSprite, DummyWatch.ImageType.LeftHand);
                            }
                            else if (60 <= ping && ping <= 90)
                            {
                                watch.SetImage(TwoBarSprite, DummyWatch.ImageType.LeftHand);
                            }
                            else if (90 <= ping && ping <= 120)
                            {
                                watch.SetImage(OneBarSprite, DummyWatch.ImageType.LeftHand);
                            }
                        }
                        watch.SetWatchText(TempText);
                        return;
                    case 1:
                        TempText = "PLACEHOLDER";
                        watch.SetWatchText(TempText);
                        return;
                    case 2:
                        TempText = "come up with\nanother idea bro";
                        watch.SetWatchText(TempText);
                        return;
                }
            }
        }

        void PageChange(object sender, EventArgs e)
        {
            switch (pageManager.CurrentPage)
            {
                case 0:
                    RegionEval();
                    return;
            }
        }

        async void WatchCreate()
        {
            watch = await DummyWatch.CreateDummyWatch(Assembly.GetExecutingAssembly(), GorillaTagger.Instance.offlineVRRig);
            watch.SetImage(null, DummyWatch.ImageType.Hat);
            watch.SetImage(null, DummyWatch.ImageType.Badge);
            watch.SetImage(null, DummyWatch.ImageType.Face);
            watch.SetImage(null, DummyWatch.ImageType.LeftHand);
            watch.SetImage(null, DummyWatch.ImageType.RightHand);
            watch.SetColourSwatch(Color.clear);
            WatchActive = true;
        }

        void WatchDestroy()
        {
            DummyWatch.RemoveDummyWatch(Assembly.GetExecutingAssembly(), GorillaTagger.Instance.offlineVRRig);
            WatchActive = false;
        }

        void RegionEval()
        {
            switch (PhotonNetwork.CloudRegion.Replace("/*", "").ToUpper())
            {
                case "US":
                    watch.SetImage(usSprite, DummyWatch.ImageType.RightHand);
                    return;
                case "USW":
                    watch.SetImage(uswSprite, DummyWatch.ImageType.RightHand);
                    return;
                case "EU":
                    watch.SetImage(euSprite, DummyWatch.ImageType.RightHand);
                    return;
            }
        }
    }
}
﻿using BMDSwitcherAPI;
using Integrated_Presenter.BMDSwitcher;
using Integrated_Presenter.BMDSwitcher.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurations.SwitcherConfig
{
    public static class DefaultConfig
    {
        public static BMDSwitcherConfigSettings GetDefaultConfig()
        {
            BMDSwitcherConfigSettings cfg = new BMDSwitcherConfigSettings()
            {
                VideoSettings = new BMDSwitcherVideoSettings()
                {
                    VideoFPS = 30,
                    VideoHeight = 1080,
                    VideoWidth = 1920
                },
                AudioSettings = new BMDSwitcherAudioSettings()
                {
                    ProgramOutGain = 2,
                    XLRInputGain = 6,
                },
                DefaultAuxSource = (int)BMDSwitcherVideoSources.ME1Prog,
                Routing = new List<ButtonSourceMapping>() {
                    new ButtonSourceMapping() { KeyName = "left", ButtonId = 1, ButtonName = "PULPIT", PhysicalInputId = 8, LongName = "PULPIT", ShortName = "PLPT" },
                    new ButtonSourceMapping() { KeyName = "center", ButtonId = 2, ButtonName = "CENTER", PhysicalInputId = 7, LongName = "CENTER", ShortName = "CNTR" },
                    new ButtonSourceMapping() { KeyName = "right", ButtonId = 3, ButtonName = "LECTERN", PhysicalInputId = 6, LongName = "LECTERN", ShortName = "LTRN" },
                    new ButtonSourceMapping() { KeyName = "organ", ButtonId = 4, ButtonName = "ORGAN", PhysicalInputId = 5, LongName = "ORGAN", ShortName = "ORGN" },
                    new ButtonSourceMapping() { KeyName = "slide", ButtonId = 5, ButtonName = "SLIDE", PhysicalInputId = 4, LongName = "SLIDESHOW", ShortName = "SLDE" },
                    new ButtonSourceMapping() { KeyName = "key", ButtonId = 6, ButtonName = "AKEY", PhysicalInputId = 3, LongName = "ALPHA KEY", ShortName = "AKEY" },
                    new ButtonSourceMapping() { KeyName = "proj", ButtonId = 7, ButtonName = "PROJ", PhysicalInputId = 2, LongName = "PROJECTOR", ShortName = "PROJ" },
                    new ButtonSourceMapping() { KeyName = "c1", ButtonId = 8, ButtonName = "CAM1", PhysicalInputId = 1, LongName = "HDMI 1", ShortName = "CAM1" },
                    new ButtonSourceMapping() { KeyName = "cf1", ButtonId = 9, ButtonName = "CLF1", PhysicalInputId = (int)BMDSwitcherVideoSources.CleanFeed1, LongName = "CLEAN FEED 1", ShortName = "CLF1" },
                    new ButtonSourceMapping() { KeyName = "cf2", ButtonId = 0, ButtonName = "CLF2", PhysicalInputId = (int)BMDSwitcherVideoSources.CleanFeed2, LongName = "CLEAN FEED 2", ShortName = "CLF2" },
                    new ButtonSourceMapping() { KeyName = "black", ButtonId = 10, ButtonName = "BLACK", PhysicalInputId = (int)BMDSwitcherVideoSources.Black, LongName = "BLACK", ShortName = "BLK" },
                    new ButtonSourceMapping() { KeyName = "cbar", ButtonId = 11, ButtonName = "CBAR", PhysicalInputId = (int)BMDSwitcherVideoSources.ColorBars, LongName = "COLOR BARS", ShortName = "CBAR" },
                    new ButtonSourceMapping() { KeyName = "program", ButtonId = 12, ButtonName = "PRGM", PhysicalInputId = (int)BMDSwitcherVideoSources.ME1Prog, LongName = "PROGRAM", ShortName = "PRGM" },
                    new ButtonSourceMapping() { KeyName = "preview", ButtonId = 13, ButtonName = "PREV", PhysicalInputId = (int)BMDSwitcherVideoSources.ME1Prev, LongName = "PREVIEW", ShortName = "PREV" },
                },
                MixEffectSettings = new BMDMixEffectSettings()
                {
                    Rate = 30,
                    FTBRate = 30,
                },
                MultiviewerConfig = new BMDMultiviewerSettings()
                {
                    Layout = (int)_BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramTop, // 12
                    Window2 = 8,
                    Window3 = 7,
                    Window4 = 6,
                    Window5 = 5,
                    Window6 = 4,
                    Window7 = 3,
                    Window8 = 2,
                    Window9 = 1,
                    ShowVUMetersOnWindows = new List<int>() // by defaul don't show vu meters
                },
                DownstreamKey1Config = new BMDDSKSettings()
                {
                    InputFill = 4,
                    InputCut = 3,
                    Clip = 0.5,
                    Gain = 0.35,
                    Rate = 30,
                    Invert = 0,
                    IsPremultipled = 0,
                    IsMasked = 0,
                    MaskTop = -5.5f,
                    MaskBottom = -9,
                    MaskLeft = -16,
                    MaskRight = 16
                },
                DownstreamKey2Config = new BMDDSKSettings()
                {
                    InputFill = 4,
                    InputCut = 0,
                    Clip = 1,
                    Gain = 1,
                    Rate = 30,
                    Invert = 1,
                    IsPremultipled = 0,
                    IsMasked = 1,
                    MaskTop = 9,
                    MaskBottom = -9,
                    MaskLeft = 0,
                    MaskRight = 16
                },
                USKSettings = new BMDUSKSettings()
                {
                    IsDVE = 1,
                    IsChroma = 0,
                    PIPSettings = new BMDUSKDVESettings()
                    {
                        DefaultFillSource = 1,
                        IsBordered = 0,
                        IsMasked = 0,
                        MaskTop = 0,
                        MaskBottom = 0,
                        MaskLeft = 0,
                        MaskRight = 0,
                        Current = new KeyFrameSettings()
                        {
                            PositionX = 9.6,
                            PositionY = -5.4,
                            SizeX = 0.4,
                            SizeY = 0.4
                        },
                        KeyFrameA = new KeyFrameSettings()
                        {
                            PositionX = 9.6,
                            PositionY = -5.4,
                            SizeX = 0.4,
                            SizeY = 0.4
                        },
                        KeyFrameB = new KeyFrameSettings()
                        {
                            PositionX = 23,
                            PositionY = -5.4,
                            SizeX = 0.4,
                            SizeY = 0.4
                        }
                    },
                    ChromaSettings = new BMDUSKChromaSettings()
                    {
                        FillSource = 4,
                        Hue = 321.8,
                        Gain = 0.652,
                        YSuppress = 0.595,
                        Lift = 0.095,
                        Narrow = 0
                    },
                },
                PrerollSettings = new PrerollSettings()
                {
                    VideoPreRoll = 2000,
                    ChromaVideoPreRoll = 2000,
                    PresetSelectDelay = 100,
                }

            };

            return cfg;
        }
    }
}

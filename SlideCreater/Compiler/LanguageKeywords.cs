﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Compiler
{
    public static class LanguageKeywords
    {

        public static Dictionary<LanguageKeywordCommand, string> Commands = new Dictionary<LanguageKeywordCommand, string>()
        {
            [LanguageKeywordCommand.Break] = "break", 
            [LanguageKeywordCommand.Image] = "image", 
            [LanguageKeywordCommand.Video] = "video", 
            [LanguageKeywordCommand.FullImage] = "fullimage", 
            [LanguageKeywordCommand.FitImage] = "fitimage", 
            [LanguageKeywordCommand.LiturgyImage] = "litimage", 
            [LanguageKeywordCommand.Liturgy] = "liturgy", 
            [LanguageKeywordCommand.Reading] = "reading", 
            [LanguageKeywordCommand.Sermon] = "sermon", 
        };


    }

    public enum LanguageKeywordCommand
    {
        Break,
        Image,
        Video,
        FullImage,
        FitImage,
        LiturgyImage,
        Liturgy,
        Reading,
        Sermon,
    }

}

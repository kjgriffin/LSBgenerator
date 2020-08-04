﻿using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlideCreater.Renderer
{
    public class RenderedSlide
    {
        public MediaType MediaType { get; set; }
        public string AssetPath { get; set; }
        public System.Drawing.Bitmap? Bitmap {get; set;}

        public static RenderedSlide Default()
        {
            return new RenderedSlide() { MediaType = MediaType.Empty, AssetPath = "" };
        }
    }
}

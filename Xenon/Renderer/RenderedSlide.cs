﻿using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xenon.Renderer
{
    public class RenderedSlide
    {
        public MediaType MediaType { get; set; }
        public string AssetPath { get; set; }
        public string RenderedAs { get; set; }
        public Bitmap Bitmap {get; set;}
        public int Number { get; set; }

        public static RenderedSlide Default()
        {
            return new RenderedSlide() { MediaType = MediaType.Empty, AssetPath = "", RenderedAs = "Default", Number = 0 };
        }
    }
}

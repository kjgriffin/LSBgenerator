﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class TwoPartTitleSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";

            Bitmap bmp = new Bitmap(Layouts.TwoPartTitleLayout.Size.Width, Layouts.TwoPartTitleLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.Gray);
            gfx.FillRectangle(Brushes.Black, Layouts.TwoPartTitleLayout.Key);

            Font bf = new Font(Layouts.TwoPartTitleLayout.Font, FontStyle.Bold);

            var lineheight = gfx.MeasureStringCharacters(slide.Lines[0].Content[0].Data, bf, Layouts.AnthemTitleLayout.Key);

            int ycord = (int)((Layouts.TwoPartTitleLayout.Key.Height / 2) - (lineheight.Height / 2));

            gfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, Layouts.TwoPartTitleLayout.MainLine.Move(Layouts.TwoPartTitleLayout.Key.Location).Move(0, ycord), GraphicsHelper.LeftVerticalCenterAlign);
            gfx.DrawString(slide.Lines[1].Content[0].Data, Layouts.TwoPartTitleLayout.Font, Brushes.White, Layouts.TwoPartTitleLayout.MainLine.Move(Layouts.TwoPartTitleLayout.Key.Location).Move(0, ycord), GraphicsHelper.RightVerticalCenterAlign);

            res.Bitmap = bmp;
            return res;
        }
    }
}
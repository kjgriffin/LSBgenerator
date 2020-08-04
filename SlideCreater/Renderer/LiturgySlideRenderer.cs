﻿using SlideCreater.LayoutEngine;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SlideCreater.Renderer
{
    public class LiturgySlideRenderer
    {

        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(LiturgyLayoutRenderInfo renderInfo, Slide slide)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            // draw it

            // for now just draw the layout
            Bitmap bmp = new Bitmap(Layouts.LiturgyLayout.Size.Width, Layouts.LiturgyLayout.Size.Height);

            Graphics gfx = Graphics.FromImage(bmp);


            gfx.Clear(Color.Black);

            gfx.FillRectangle(Brushes.White, Layouts.LiturgyLayout.Key);

            StringFormat topleftalign = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };
            StringFormat centeralign = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

            System.Drawing.Rectangle text = Layouts.LiturgyLayout.Text.Move(Layouts.LiturgyLayout.Key.Location);

            System.Drawing.Rectangle speaker = Layouts.LiturgyLayout.Speaker.Move(Layouts.LiturgyLayout.Key.Location);

            //gfx.DrawRectangle(Pens.Red, text);
            //gfx.DrawRectangle(Pens.Purple, speaker);

            // compute line spacing
            int textlinecombinedheight = 0;
            foreach (var line in slide.Lines)
            {
                textlinecombinedheight += (int)(float)line.Content[1].Attributes["height"];
            }
            int interspace = (renderInfo.TextBox.Height - textlinecombinedheight) / (slide.Lines.Count + 1);

            int linenum = 0;
            int linepos = interspace;
            foreach (var line in slide.Lines)
            {
                System.Drawing.Rectangle speakerblock = new System.Drawing.Rectangle(speaker.Move(0, linepos + interspace * linenum).Location, new System.Drawing.Size(60, 60));
                switch (line.Content[0].Data)
                {
                    case "C":
                        gfx.FillRectangle(Brushes.Red, speakerblock);
                        gfx.DrawString(line.Content[0].Data, renderInfo.BoldFont, Brushes.White, speakerblock, centeralign);
                        gfx.DrawString(line.Content[1].Data, renderInfo.BoldFont, Brushes.Black, text.Move(0, linepos + interspace * linenum).Location, topleftalign);
                        break;
                    default:
                        gfx.DrawRectangle(Pens.Red, speakerblock);
                        gfx.DrawString(line.Content[0].Data, renderInfo.RegularFont, Brushes.Red, speakerblock, centeralign);
                        gfx.DrawString(line.Content[1].Data, renderInfo.RegularFont, Brushes.Black, text.Move(0, linepos + interspace * linenum).Location, topleftalign);
                        break;
                }
                linenum++;
                linepos += (int)(float)line.Content[1].Attributes["height"];
            }

            res.Bitmap = bmp;

            return res;
        }
    }
}

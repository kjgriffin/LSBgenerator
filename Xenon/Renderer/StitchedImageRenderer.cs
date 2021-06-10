﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.SlideAssembly;
using System.Linq;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.AssetManagment;

namespace Xenon.Renderer
{
    class StitchedImageRenderer
    {

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, List<ProjectAsset> projassets)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(1920, 1080);
            Bitmap kbmp = new Bitmap(1920, 1080);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.Gray);
            kgfx.Clear(Color.White);

            List<LSBImageResource> imageresources = (List<LSBImageResource>)slide.Data.GetOrDefault("ordered-images", new List<LSBImageResource>());
            string title = (string)slide.Data.GetOrDefault("title", "");
            string hymnname = (string)slide.Data.GetOrDefault("hymnname", "");
            string number = (string)slide.Data.GetOrDefault("number", "");

            // create new bitmap for the image for now...
            // compute max bounds of new bitmap
            // 1920x100 box for title info
            // find max width of images and compare
            int twidth = (int)gfx.MeasureString(hymnname, new Font("Arial", 36, FontStyle.Bold)).Width;
            int width = Math.Max(twidth, imageresources.Select(i => i.Size.Width).Max());
            int height = 100 + imageresources.Select(i => i.Size.Height).Aggregate((a, b) => a + b) + 30;
            Bitmap sbmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(sbmp);

            g.Clear(Color.White);

            // draw text
            g.DrawString(hymnname, new Font("Arial", 36, FontStyle.Bold), Brushes.Black, new Rectangle(0, 0, width, 100), GraphicsHelper.CenterAlign);
            g.DrawString(title, new Font("Arial", 30, FontStyle.Regular), Brushes.Black, new Rectangle(0, 0, width, 100), GraphicsHelper.LeftVerticalCenterAlign);
            g.DrawString(number, new Font("Arial", 30, FontStyle.Regular), Brushes.Black, new Rectangle(0, 0, width, 100), GraphicsHelper.RightVerticalCenterAlign);
            // draw copyright
            if (slide.Data.ContainsKey("copyright-text"))
            {
                string copyrighttune = (string)slide.Data.GetOrDefault("copyright-tune", "");
                string copyrighttext = (string)slide.Data.GetOrDefault("copyright-text", "");
                g.DrawString(copyrighttune, new Font("Arial", 8, FontStyle.Regular), Brushes.Gray, new Rectangle(0, height - 30, width, 15), GraphicsHelper.LeftVerticalCenterAlign);
                g.DrawRectangle(Pens.Orange, 0, height - 30, width, 15);
                g.DrawRectangle(Pens.Orange, 0, height - 15, width, 15);
                g.DrawString(copyrighttext, new Font("Arial", 8, FontStyle.Regular), Brushes.Gray, new Rectangle(0, height - 15, width, 15), GraphicsHelper.LeftVerticalCenterAlign);
            }
            else
            {
                string copyright = (string)slide.Data.GetOrDefault("copyright", "");
                g.DrawString(copyright, new Font("Arial", 8, FontStyle.Regular), Brushes.Gray, new Rectangle(0, height - 20, width, 20), GraphicsHelper.LeftVerticalCenterAlign);
                g.DrawRectangle(Pens.Blue, 0, height - 20, width, 20);
            }

            // draw all images
            int hoff = 100;
            foreach (var image in imageresources)
            {
                // load image
                try
                {
                    Bitmap b = new Bitmap(projassets.Find(a => a.Name == image.AssetRef).CurrentPath);
                    int x = (width - b.Width) / 2;
                    g.DrawImage(b, x, hoff, b.Width, b.Height);
                    g.DrawRectangle(Pens.Red, x, hoff, b.Width, b.Height);
                    hoff += b.Height;
                }
                catch (Exception ex)
                {
                    messages.Add(new Compiler.XenonCompilerMessage() { ErrorMessage = "Error loading image", ErrorName = "Error Loading Asset", Generator = "StitchedImageRenderer", Inner = ex.Message, Level = Compiler.XenonCompilerMessageType.Error, Token = image.AssetRef });
                }
            }

            // TODO: FIX

            var resized = ImageFilters.ImageFilters.UniformStretch(sbmp, sbmp, new ImageFilters.UniformStretchFilterParams() { Fill = Color.White, KFill = Color.White, Height = 1080, Width = 1920 });

            res.Bitmap = resized.b;
            res.KeyBitmap = kbmp;
            return res;
        }

    }
}

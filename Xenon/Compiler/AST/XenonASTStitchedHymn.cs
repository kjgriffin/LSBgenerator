﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTStitchedHymn : IXenonASTCommand
    {

        public List<string> ImageAssets { get; set; } = new List<string>();
        public string Title { get; set; }
        public string HymnName { get; set; }
        public string Number { get; set; }
        public string CopyrightInfo { get; set; }
        public bool StitchAll { get; set; }

        private string CopyrightTune
        {
            get
            {
                var split = CopyrightInfo.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 3)
                {
                    return "Tune: " + split[0];
                }
                else
                {
                    return CopyrightInfo;
                }
            }
        }

        private string CopyrightText
        {
            get
            {
                var split = CopyrightInfo.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 3)
                {
                    return "Text: " + split[2];
                }
                else
                {
                    return CopyrightInfo;
                }
            }
        }



        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTStitchedHymn hymn = new XenonASTStitchedHymn();

            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "name", "number", "copyright");

            hymn.Title = args["title"];
            hymn.HymnName = args["name"];
            hymn.Number = args["number"];
            hymn.CopyrightInfo = args["copyright"];

            Lexer.GobbleWhitespace();

            if (Lexer.Inspect("("))
            {
                var optionalparams = Lexer.ConsumeArgList(false, "stitchall");
                if (optionalparams["stitchall"] == "stitchall")
                {
                    hymn.StitchAll = true;
                }
                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleandLog("{", "Expected opening '{'");
            while (!Lexer.InspectEOF() && !Lexer.Inspect("}"))
            {
                Lexer.GobbleWhitespace();
                string assetline = Lexer.ConsumeUntil(";");
                hymn.ImageAssets.Add(assetline);
                Lexer.GobbleandLog(";", "Expected ';' at end of asset dependency");
                Lexer.GobbleWhitespace();
            }
            Lexer.GobbleandLog("}", "Expected closing '}'");

            return hymn;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Generating StitchedHymn {HymnName}", ErrorName = "Generation Debug Log", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Debug, Token = "" });
            // main steps
            // 1. Figure out how many lines/stanzas and how many stanzas there are
            // 2. Figure out if we can squish everything on one slide, or if we need to go stanza by stanza

            // 1. Go through every asset and check its size.
            //      if its height is less than 45px its text, if its more than 85 its music
            //      might need to be inefficient here and open the file to check the height. Don't think we've got that info yet

            Dictionary<string, Size> ImageSizes = new Dictionary<string, Size>();

            foreach (var item in ImageAssets)
            {
                try
                {
                    using (Bitmap b = new Bitmap(project.Assets.Find(a => a.Name == item).CurrentPath))
                    {
                        ImageSizes[item] = b.Size;
                        Debug.WriteLine($"Image {item} has size {b.Size}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = "Generating StitchedHymn", ErrorName = "Failed to load image asset", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"{ex}", Level = XenonCompilerMessageType.Warning, Token = "" });
                    Debug.WriteLine($"Error opening image to check size: {ex}");
                    // hmmm....
                }
            }

            // create stanzainfo for everything
            // loop over every image
            // when we find a music line we find all text related to it (everything after until next music line)
            // create a bunch of verse lines for everything
            // when we get to end build into stanzas
            // this doesn't account for refrains.... we can detect refrain by height as well (even taller for 'Refrain' text included)
            // not sure how to detect how long refrain is... might be able to assume refrains have only one line, and stanzas would have multiple...
            // also not sure how to handle optional refrains....
            // refrains at the end can just all be refrain??
            // refrains at the beginnnig will be harder
            // expecially hymns that repeat it again after last verse too...
            // ....
            // perhaps we'll do a best effort (focused on making the default [read: no-refrain] cases work)
            // we'll try sorting out refrains - but we'll flag it, and add a comment line for manual inspection

            if (StitchAll)
            {
                // just a bunch of image lines...
                // can stop right here and build a slide
                Slide slide = new Slide();
                slide.Name = $"stitchedhymn";
                slide.Number = project.NewSlideNumber;
                slide.Asset = "";
                slide.Lines = new List<SlideLine>();
                slide.Format = SlideFormat.StichedImage;
                slide.MediaType = MediaType.Image;

                slide.Data["title"] = Title;
                slide.Data["hymnname"] = HymnName;
                slide.Data["number"] = Number;
                slide.Data["copyright"] = CopyrightInfo;
                if (CopyrightText != CopyrightTune)
                {
                    slide.Data["copyright-text"] = CopyrightText;
                    slide.Data["copyright-tune"] = CopyrightTune;
                }

                slide.Data["ordered-images"] = ImageSizes.Select(i => new LSBImageResource(i.Key, i.Value)).ToList();

                slide.AddPostset(_Parent, true, true);

                project.Slides.Add(slide);


                var heightaprox = 200 + ImageSizes.Select(i => i.Value.Height).Aggregate((item, sum) => sum + item);
                if (heightaprox > 1200)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Hymn over height: {HymnName}. Hymn requested with 'stichall' flag, but hymn is expected to have {heightaprox} height.", ErrorName = "StitchAll Overheight", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = "" });
                }

                return;

            }

            bool unconfidentaboutrefrain = false;
            bool unconfidentaboutlinetype = false;
            const int refrainheight = 136;
            LSBImageResource linemusic = null;
            List<LSBImageResource> linetexts = new List<LSBImageResource>();
            List<(LSBImageResource music, List<LSBImageResource> words)> CollatedLines = new List<(LSBImageResource music, List<LSBImageResource> words)>();
            for (int i = 0; i < ImageAssets.Count; i++)
            {
                // check for new music line
                if (ImageSizes[ImageAssets[i]].Height > 95)
                {
                    if (ImageSizes[ImageAssets[i]].Height > 130 && i == 0)
                    {
                    }
                    // add previous lines
                    if (linemusic != null)
                    {
                        CollatedLines.Add((linemusic, linetexts));
                    }

                    // create new stuff
                    linemusic = new LSBImageResource(ImageAssets[i], ImageSizes[ImageAssets[i]]);
                    linetexts = new List<LSBImageResource>();
                }
                // is text. attach to previous line
                else if (ImageSizes[ImageAssets[i]].Height < 45)
                {
                    linetexts.Add(new LSBImageResource(ImageAssets[i], ImageSizes[ImageAssets[i]]));
                }
                else
                {
                    unconfidentaboutlinetype = true;
                }
            }
            if (linemusic != null)
            {
                CollatedLines.Add((linemusic, linetexts));
            }

            if (unconfidentaboutlinetype)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Unconfident about linetype for hymn {HymnName}", ErrorName = "Autogen Unconfident", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = "" });
            }
            //if (unconfidentaboutlinetype)
            //{
            //Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Unconfident about refrain for hymn {HymnName}", ErrorName = "Autogen Unconfident", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = "" });
            //}

            // now we go through collated lines and try and understand if they're regular verses/ if we got into refrains
            // and then uncollate and turn into verse based format


            // check all collated lines. If some lines have more words than others, then we'd suspect it's a refrain - not part of verse
            // (NOTE) LSB has an option to output stz#'s if not all are selected. Might be able to assume all hymns have multiple verses. then could use this to indicate if not the case

            // TODO: do something about refrains

            // for now handle
            /* Case 1 Hymns: only verse
             * Case 2 Hymns: verse, refrain
             */

            // check for a case 2 hymn
            var firstcollated = CollatedLines.FirstOrDefault();
            int numverses = firstcollated.words?.Count ?? 0;

            if (numverses == 0)
            {
                // just a bunch of image lines...
                // can stop right here and build a slide
                Slide slide = new Slide();
                slide.Name = $"stitchedhymn";
                slide.Number = project.NewSlideNumber;
                slide.Asset = "";
                slide.Lines = new List<SlideLine>();
                slide.Format = SlideFormat.StichedImage;
                slide.MediaType = MediaType.Image;

                slide.Data["title"] = Title;
                slide.Data["hymnname"] = HymnName;
                slide.Data["number"] = Number;
                slide.Data["copyright"] = CopyrightInfo;
                if (CopyrightText != CopyrightTune)
                {
                    slide.Data["copyright-text"] = CopyrightText;
                    slide.Data["copyright-tune"] = CopyrightTune;
                }

                slide.Data["ordered-images"] = ImageAssets.Select(i => new LSBImageResource(i, ImageSizes[i])).ToList();

                slide.AddPostset(_Parent, true, true);

                project.Slides.Add(slide);

                var heightaprox = 200 + ImageAssets.Select(i => ImageSizes[i].Height).Aggregate((item, sum) => sum + item);
                if (heightaprox > 1200)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Hymn over height: {HymnName}. Hymn interpreted to have only one verse,w with all lines on the same slide. Expected to have {heightaprox} height.", ErrorName = "Verse Overheight", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = "" });
                }


                return;
            }

            List<(LSBImageResource music, List<LSBImageResource> words)> VerseCollatedLines = new List<(LSBImageResource music, List<LSBImageResource> words)>();
            List<(LSBImageResource music, List<LSBImageResource> words)> RefrainCollatedLines = new List<(LSBImageResource music, List<LSBImageResource> words)>();
            bool foundrefrain = false;
            // NOTE: approach fails if refrain comes first
            foreach (var cl in CollatedLines)
            {
                if (cl.words.Count != numverses && cl.words.Count == 1)
                {
                    // after this everything is refrain???
                    foundrefrain = true;
                }
                if (foundrefrain)
                {
                    RefrainCollatedLines.Add(cl);
                }
                else
                {
                    VerseCollatedLines.Add(cl);
                }
            }

            // build into a StitchedImageHymnVerses
            List<StitchedImageHymnStanza> stanzas = new List<StitchedImageHymnStanza>();
            for (int i = 0; i < numverses; i++)
            {
                List<LSBPairedHymnLine> verselines = new List<LSBPairedHymnLine>();
                for (int j = 0; j < VerseCollatedLines.Count; j++)
                {
                    LSBPairedHymnLine line = new LSBPairedHymnLine(VerseCollatedLines[j].music, VerseCollatedLines[j].words[i]);
                    verselines.Add(line);
                }
                StitchedImageHymnStanza stanza = new StitchedImageHymnStanza(verselines);
                stanzas.Add(stanza);
            }

            List<LSBPairedHymnLine> refrainlines = new List<LSBPairedHymnLine>();
            for (int i = 0; i < RefrainCollatedLines.Count; i++)
            {
                refrainlines.Add(new LSBPairedHymnLine(RefrainCollatedLines[i].music, RefrainCollatedLines[i].words[0]));
            }
            StitchedImageHymnStanza refrain = new StitchedImageHymnStanza(refrainlines);

            StitchedImageHymnVerses hymn = new StitchedImageHymnVerses() { Refrain = refrain, RepeatingPostRefrain = foundrefrain, Verses = stanzas };

            // At this point we should probably double check that total lines of refrain and all verses is the same number of lines we started with
            // if not- then we probably split it into verses/refrains incorrectly
            if (!hymn.PerformSanityCheck(ImageAssets.Count()))
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Unconfident splitting verses/refrains for {HymnName}", ErrorName = "Autogen Unconfident", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"Provided {ImageAssets.Count()} source images. Generated Hymn only uses {hymn.ComputeSourceLinesUsed()}", Level = XenonCompilerMessageType.Warning, Token = "" });
            }

            // 2. add the height of all the images. if height > 1200??? then we'll do it by stanza
            const int MaxHeightForImags = 1200;
            int height = 0;
            foreach (var lineitem in ImageAssets)
            {
                height += ImageSizes[lineitem].Height;
            }

            if (height < MaxHeightForImags)
            {
                // do it all on one slide
                Slide slide = new Slide();
                slide.Name = $"stitchedhymn";
                slide.Number = project.NewSlideNumber;
                slide.Asset = "";
                slide.Lines = new List<SlideLine>();
                slide.Format = SlideFormat.StichedImage;
                slide.MediaType = MediaType.Image;

                slide.Data["title"] = Title;
                slide.Data["hymnname"] = HymnName;
                slide.Data["number"] = Number;
                slide.Data["copyright"] = CopyrightInfo;
                if (CopyrightText != CopyrightTune)
                {
                    slide.Data["copyright-text"] = CopyrightText;
                    slide.Data["copyright-tune"] = CopyrightTune;
                }

                slide.Data["ordered-images"] = hymn.OrderAllAsOne();
                slide.AddPostset(_Parent, true, true);
                project.Slides.Add(slide);
            }
            else
            {
                // go verse by verse
                int versenum = 0;
                foreach (var verse in hymn.Verses)
                {
                    Slide slide = new Slide();
                    slide.Name = $"stitchedhymn";
                    slide.Number = project.NewSlideNumber;
                    slide.Asset = "";
                    slide.Lines = new List<SlideLine>();
                    slide.Format = SlideFormat.StichedImage;
                    slide.MediaType = MediaType.Image;

                    slide.Data["title"] = Title;
                    slide.Data["hymnname"] = HymnName;
                    slide.Data["number"] = Number;
                    slide.Data["copyright"] = CopyrightInfo;
                    if (CopyrightText != CopyrightTune)
                    {
                        slide.Data["copyright-text"] = CopyrightText;
                        slide.Data["copyright-tune"] = CopyrightTune;
                    }

                    slide.Data["ordered-images"] = hymn.OrderVerse(versenum++);

                    slide.AddPostset(_Parent, versenum == 0, hymn.Verses.Count == versenum && !hymn.RepeatingPostRefrain);
                    project.Slides.Add(slide);

                    if (hymn.RepeatingPostRefrain)
                    {
                        Slide refrainslide = new Slide();
                        refrainslide.Name = $"stitchedhymn";
                        refrainslide.Number = project.NewSlideNumber;
                        refrainslide.Asset = "";
                        refrainslide.Lines = new List<SlideLine>();
                        refrainslide.Format = SlideFormat.StichedImage;
                        refrainslide.MediaType = MediaType.Image;

                        refrainslide.Data["title"] = Title;
                        refrainslide.Data["hymnname"] = HymnName;
                        refrainslide.Data["number"] = Number;
                        refrainslide.Data["copyright"] = CopyrightInfo;
                        if (CopyrightText != CopyrightTune)
                        {
                            refrainslide.Data["copyright-text"] = CopyrightText;
                            refrainslide.Data["copyright-tune"] = CopyrightTune;
                        }

                        refrainslide.Data["ordered-images"] = hymn.OrderRefrain();


                        refrainslide.AddPostset(_Parent, false, hymn.Verses.Count == versenum);

                        project.Slides.Add(refrainslide);
                    }
                }

            }
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTStitchedHymn>");
            Debug.WriteLine($"Title={Title}");
            Debug.WriteLine($"HymnName={HymnName}");
            Debug.WriteLine($"Number={Number}");
            Debug.WriteLine($"Copyright={CopyrightInfo}");
            foreach (var asset in ImageAssets)
            {
                Debug.WriteLine($"ImageAsset={asset}");
            }
            Debug.WriteLine("</XenonASTStitchedHymn>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}

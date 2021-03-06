﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xenon.Compiler.AST;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    class XenonASTExpression : IXenonASTElement
    {
        public IXenonASTCommand Command { get; set; }
        public bool Postset { get; set; } = false;
        public bool Postset_forAll { get => Postset_All >= 0; }
        public bool Postset_forFirst { get => Postset_First >= 0; }
        public bool Postset_forLast { get => Postset_Last >= 0; }
        public int Postset_All { get; set; } = -1;
        public int Postset_First { get; set; } = -1;
        public int Postset_Last { get; set; } = -1;

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Command?.Generate(project, this, Logger);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTExperession>");
            Command?.GenerateDebug(project);
            Debug.WriteLine("</XenonASTExperession>");
        }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTExpression expr;
            // parse expressions command
            if (Lexer.GobbleandLog("#"))
            {
                expr = CompileCommand(Lexer, Logger);
            }
            else
            {
                throw new XenonCompilerException();
            }

            Lexer.GobbleWhitespace();

            // parse optional expression postshot
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("::"))
                {
                    CompilePostset(expr, Lexer, Logger);
                }
            }

            return expr;
        }

        private void CompilePostset(XenonASTExpression expr, Lexer lexer, XenonErrorLogger logger)
        {
            lexer.Gobble("::");

            lexer.GobbleandLog("postset", "Expected 'postset' tag.");

            var args = lexer.ConsumeOptionalNamedArgsUnenclosed("all", "first", "last");

            if (args.ContainsKey("all"))
            {
                if (int.TryParse(args["all"], out int val))
                {
                    expr.Postset_All = val;
                }
            }
            if (args.ContainsKey("first"))
            {
                if (int.TryParse(args["first"], out int val))
                {
                    expr.Postset_First = val;
                }
            }
            if (args.ContainsKey("last"))
            {
                if (int.TryParse(args["last"], out int val))
                {
                    expr.Postset_Last = val;
                }
            }

            // handle missing params

            expr.Postset = true;

            if (!expr.Postset_forAll && !expr.Postset_forFirst && !expr.Postset_forLast)
            {
                // bad params
                // let it compile still
                expr.Postset = false;
                // log error
                logger.Log(new XenonCompilerMessage() { ErrorName = "Missing 'postset' parameters", ErrorMessage = $"Expression marked to have 'postset' but no parameters were provided. Use any of 'all', 'first', 'last'.", Generator = "XenonASTExpression.CompilePostset", Inner = "Will ignore postset.", Token = "", Level = XenonCompilerMessageType.Error });
            }

        }

        private XenonASTExpression CompileCommand(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTExpression expr = new XenonASTExpression();

            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Resource]))
            {
                XenonASTResource resource = new XenonASTResource();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Resource]);
                expr.Command = (IXenonASTCommand)resource.Compile(Lexer, Logger);
                return expr;
            }
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Script]))
            {
                XenonASTScript script = new XenonASTScript();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Script]);
                expr.Command = (IXenonASTCommand)script.Compile(Lexer, Logger);
                return expr;
            }


            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.SetVar]))
            {
                XenonASTSetVariable xenonASTSetVariable = new XenonASTSetVariable();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.SetVar]);
                expr.Command = (IXenonASTCommand)xenonASTSetVariable.Compile(Lexer, Logger);
                return expr;
            }
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Video]))
            {
                XenonASTVideo video = new XenonASTVideo();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Video]);
                expr.Command = (IXenonASTCommand)video.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FilterImage]))
            {
                XenonASTFilterImage fimage = new XenonASTFilterImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FilterImage]);
                expr.Command = (IXenonASTCommand)fimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]))
            {
                XenonASTFullImage fullimage = new XenonASTFullImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]);
                expr.Command = (IXenonASTCommand)fullimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]))
            {
                XenonASTFitImage fitimage = new XenonASTFitImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]);
                expr.Command = (IXenonASTCommand)fitimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.AutoFitImage]))
            {
                XenonASTAutoFitImage autofit = new XenonASTAutoFitImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.AutoFitImage]);
                expr.Command = (IXenonASTCommand)autofit.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.StitchedImage]))
            {
                XenonASTStitchedHymn hymn = new XenonASTStitchedHymn();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.StitchedImage]);
                expr.Command = (IXenonASTCommand)hymn.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]))
            {
                XenonASTLiturgyImage liturgyimage = new XenonASTLiturgyImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]);
                expr.Command = (IXenonASTCommand)liturgyimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Break]))
            {
                XenonASTSlideBreak slidebreak = new XenonASTSlideBreak();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Break]);
                expr.Command = (IXenonASTCommand)slidebreak.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]))
            {
                XenonASTLiturgy liturgy = new XenonASTLiturgy();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]);
                expr.Command = (IXenonASTCommand)liturgy.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyVerse]))
            {
                XenonASTLiturgyVerse litverse = new XenonASTLiturgyVerse();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyVerse]);
                expr.Command = (IXenonASTCommand)litverse.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]))
            {
                XenonASTTitledLiturgyVerse tlverse = new XenonASTTitledLiturgyVerse();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]);
                expr.Command = (IXenonASTCommand)tlverse.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]))
            {
                XenonASTReading reading = new XenonASTReading();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]);
                expr.Command = (IXenonASTCommand)reading.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]))
            {
                XenonASTSermon sermon = new XenonASTSermon();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]);
                expr.Command = (IXenonASTCommand)sermon.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.AnthemTitle]))
            {
                XenonASTAnthemTitle anthem = new XenonASTAnthemTitle();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.AnthemTitle]);
                expr.Command = (IXenonASTCommand)anthem.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle]))
            {
                XenonAST2PartTitle title = new XenonAST2PartTitle();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle]);
                expr.Command = (IXenonASTCommand)title.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]))
            {
                XenonASTTextHymn texthymn = new XenonASTTextHymn();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]);
                expr.Command = (IXenonASTCommand)texthymn.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]);
                expr.Command = new XenonASTPrefabCopyright();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]);
                expr.Command = new XenonASTPrefabViewServices();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]);
                expr.Command = new XenonASTPrefabViewSeries();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]);
                expr.Command = new XenonASTPrefabApostlesCreed();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]);
                expr.Command = new XenonASTPrefabLordsPrayer();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.NiceneCreed]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.NiceneCreed]);
                expr.Command = new XenonASTPrefabNiceneCreed();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Script_LiturgyOff]))
            {
                var command = new XenonASTPrefabScriptLiturgyOff();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Script_LiturgyOff]);
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Script_OrganIntro]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Script_OrganIntro]);
                expr.Command = new XenonASTPrefabScriptOrganIntro();
                return expr;
            }
            else
            {
                Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Error, ErrorName = "Unknown Command", ErrorMessage = $"{Lexer.Peek()} is not a recognized command", Token = Lexer.Peek(), Generator = "Compiler" });
                throw new XenonCompilerException();
            }
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}

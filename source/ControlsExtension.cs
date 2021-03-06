﻿using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Mvc;
using Sitecore.Mvc.Presentation;
using TheReference.DotNet.Sitecore.EasyLingo.Speak;

namespace TheReference.DotNet.Sitecore.EasyLingo
{
    public static class ControlsExtension
    {
        public static HtmlString LanguageBar(this Controls controls, Rendering rendering)
        {
            Assert.ArgumentNotNull(controls, "controls");
            Assert.ArgumentNotNull(rendering, "rendering");
            return new HtmlString(new LanguageBar(controls.GetParametersResolver(rendering)).Render());
        }
    }
}
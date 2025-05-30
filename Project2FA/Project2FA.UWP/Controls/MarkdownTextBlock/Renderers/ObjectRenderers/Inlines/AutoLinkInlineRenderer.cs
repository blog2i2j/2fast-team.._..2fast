// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax.Inlines;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements;
using System;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.Renderers.ObjectRenderers.Inlines
{
    internal class AutoLinkInlineRenderer : UWPObjectRenderer<AutolinkInline>
    {
        protected override void Write(WinUIRenderer renderer, AutolinkInline link)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (link == null) throw new ArgumentNullException(nameof(link));

            var url = link.Url;
            if (link.IsEmail)
            {
                url = "mailto:" + url;
            }

            //if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            //{
            //    url = "#";
            //}

            var autolink = new MyAutolinkInline(link);
            //autolink.ClickEvent += (sender, e) =>
            //{
            //    renderer.MarkdownTextBlock.RaiseLinkClickedEvent(sender.NavigateUri);
            //};

            renderer.Push(autolink);

            renderer.WriteText(link.Url);
            renderer.Pop();
        }
    }
}

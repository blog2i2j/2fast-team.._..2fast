// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Markdig.Extensions.TaskLists;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.Foundation;
using System;
using System.Linq;
using System.Globalization;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyLineBreak : IAddChild
    {
        private LineBreak _lineBreak;

        public TextElement TextElement
        {
            get => _lineBreak;
        }

        public MyLineBreak()
        {
            _lineBreak = new LineBreak();
        }

        public void AddChild(IAddChild child) {}
    }
}

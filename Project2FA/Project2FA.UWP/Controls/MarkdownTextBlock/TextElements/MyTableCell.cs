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

using Markdig.Extensions.Tables;
using Windows.UI.Text;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyTableCell : IAddChild
    {
        private TableCell _tableCell;
        private Paragraph _paragraph = new Paragraph();
        private MyFlowDocument _flowDocument;
        private bool _isHeader;
        private int _columnIndex;
        private int _rowIndex;
        private Grid _container;

        public TextElement TextElement
        {
            get => _paragraph;
        }

        public Grid Container
        {
            get => _container;
        }

        public int ColumnSpan
        {
            get => _tableCell.ColumnSpan;
        }

        public int RowSpan
        {
            get => _tableCell.RowSpan;
        }

        public int ColumnIndex
        {
            get => _columnIndex;
        }

        public int RowIndex
        {
            get => _rowIndex;
        }

        public MyTableCell(TableCell tableCell, TextAlignment textAlignment, bool isHeader, int columnIndex, int rowIndex)
        {
            _isHeader = isHeader;
            _tableCell = tableCell;
            _columnIndex = columnIndex;
            _rowIndex = rowIndex;
            _container = new Grid();

            _flowDocument = new MyFlowDocument(tableCell);
            _flowDocument.RichTextBlock.TextWrapping = TextWrapping.Wrap;
            _flowDocument.RichTextBlock.TextAlignment = textAlignment;
            _flowDocument.RichTextBlock.HorizontalTextAlignment = textAlignment;
            _flowDocument.RichTextBlock.HorizontalAlignment = textAlignment switch
            {
                TextAlignment.Left => HorizontalAlignment.Left,
                TextAlignment.Center => HorizontalAlignment.Center,
                TextAlignment.Right => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Left,
            };

            _container.Padding = new Thickness(4);
            if (_isHeader)
            {
                _flowDocument.RichTextBlock.FontWeight = FontWeights.Bold;
            }
            _flowDocument.RichTextBlock.HorizontalAlignment = textAlignment switch
            {
                TextAlignment.Left => HorizontalAlignment.Left,
                TextAlignment.Center => HorizontalAlignment.Center,
                TextAlignment.Right => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Left,
            };
            _container.Children.Add(_flowDocument.RichTextBlock);
        }

        public void AddChild(IAddChild child)
        {
            _flowDocument.AddChild(child);
        }
    }
}

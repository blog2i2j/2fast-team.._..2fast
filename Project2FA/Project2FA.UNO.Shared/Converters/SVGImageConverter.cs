﻿using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Security.Cryptography;
using Project2FA.Repository.Models;

namespace Project2FA.UNO.Converters
{
    public class SVGImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var svg = new SvgImageSource();
            if (value != null)
            {
                try
                {
                    var utf8 = new UTF8Encoding();
                    var svgBuffer = utf8.GetBytes(value.ToString());

                    using (var stream = new MemoryStream(svgBuffer) { Position = 0 })
                    {
                        svg.SetSourceAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            return svg;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

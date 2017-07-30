using System;
using System.Diagnostics;
using System.Windows;
using GedViewWPF.DataAccess;
using GedViewWPF.Model;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace GedViewWPF.Utilities
{
    public class Utils
    {
        public static string CalculateAge(Fact fact, Fact birth, Fact death)
        {
            var age = string.Empty;

            if ((birth != null) && (birth.Date != null) && (birth.Date.DateDT != null))
            {
                if ((fact != null) && (fact.Date != null) && (fact.Date.DateDT != null))
                {
                    if (fact.Type != "BIRT")
                    {
                        var span = (DateTime)fact.Date.DateDT - (DateTime)birth.Date.DateDT;
                        age = string.Format("age: {0:n0}", span.Days / 365.25);
                    }

                    if ((death != null) && (death.Date != null) && (death.Date.DateDT != null))
                    {
                        if ((DateTime)death.Date.DateDT < (DateTime)fact.Date.DateDT)
                        {
                            age = string.Empty;
                        }
                    }
                }
            }

            return age;
        }

        public static void SortChildren(Family family)
        {
            if ((family != null) && (family.Children != null))
            {
                family.Children.Sort((c1, c2) =>
                {
                    if ((c1.Birth != null) && (c2.Birth != null))
                    {
                        return c1.Birth.CompareTo(c2.Birth);
                    }
                    else if ((c1.Birth != null) && (c2.Birth != null))
                    {
                        return -1;
                    }
                    else if ((c1.Birth != null) && (c2.Birth != null))
                    {
                        return 1;
                    }

                    return c1.CompareTo(c2);
                });
            }
        }

        public static ImageSource CreateImageSource(Bitmap image)
        {
            if (image == null)
                return null;

            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(),
                                                                                    IntPtr.Zero,
                                                                                    Int32Rect.Empty,
                                                                                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static ImageSource CreateThumbnailImageSource(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;

            try
            {
				var bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.DecodePixelWidth = 64;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(imagePath);
                bitmapImage.EndInit();

				return bitmapImage;
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}

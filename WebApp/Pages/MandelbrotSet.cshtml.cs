using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;
using System.Numerics;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Pages;

public class MandelbrotSet : PageModel
{
    public MandelbrotSet()
    {
        ViewModel = new MandelbrotSetViewModel();
    }
    
    public class MandelbrotSetViewModel
    {
        public byte[] ImageBytes { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int MaxIterations { get; set; }

        public MandelbrotSetViewModel()
        {
            ImageBytes = Array.Empty<byte>();
        }
    }

    [BindProperty]
    public MandelbrotSetViewModel ViewModel { get; set; }
    
    private static byte[] BitmapToByteArray(Bitmap bitmapImage)
    {
        return (byte[])new ImageConverter().ConvertTo(bitmapImage, typeof(byte[]))!;
    }
    
    public void OnGet(string returnUrl = "Index")
    {
        ViewModel.Height = 400;
        ViewModel.Width = 400;
        ViewModel.MaxIterations = 10;
    }

    public IActionResult OnPost(string returnUrl = "Index")
    {
        var width = ViewModel.Width;
        var height = ViewModel.Height;
        var maxK = ViewModel.MaxIterations;
        
        Bitmap bitmapImage = new (width, height);
        for (var i = 0; i < width; ++i)
        {
            for (var j = 0; j < height; ++j)
            {
                var real = (i - width / 2.0) / (width / 4.0);
                var imaginary = (j - height / 2.0) / (height / 4.0);
                var c = new Complex(real, imaginary);
                var z = new Complex(0, 0);
                var k = 0;
                while (k < maxK)
                {
                    ++k;
                    z *= z;
                    z += c;
                    if (z.Magnitude > 2.0)
                    {
                        break;
                    }
                }

                var bitColor = k < maxK ? Color.DarkBlue  : Color.Aquamarine;
                bitmapImage.SetPixel(i, j, bitColor);
            }
        }

        ViewModel.ImageBytes = BitmapToByteArray(bitmapImage);
        return Page();
    }
}
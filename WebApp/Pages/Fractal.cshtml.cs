using System.ComponentModel.DataAnnotations;
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
        
        [Range(1, 5000)]
        public int Height { get; set; }
        
        [Range(1, 5000)]
        public int Width { get; set; }
        
        [Range(1, 100)]
        public int MaxIterations { get; set; }
        public Color SetColor { get; set; }
        public Color BackgroundColor { get; set; }
        public int C { get; set; }

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
    
    public void OnGet()
    {
        ViewModel.Height = 400;
        ViewModel.Width = 400;
        ViewModel.MaxIterations = 10;
        ViewModel.SetColor = Color.Blue;
        ViewModel.SetColor = Color.Aquamarine;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
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

                var bitColor = k < maxK ? ViewModel.BackgroundColor : ViewModel.SetColor;
                bitmapImage.SetPixel(i, j, bitColor);
            }
        }

        ViewModel.ImageBytes = BitmapToByteArray(bitmapImage);
        return Page();
    }
}
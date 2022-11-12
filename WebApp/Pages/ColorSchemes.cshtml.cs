using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class ColorSchemes : PageModel
{
    // Statics
    private static string? _imageFileGuid;
    private static string? _imageFileName;
    private static string? _imageFileFullName;
    private static string? _imageFileAbsolutePath;
    private static string? _imageFileRelativePath;

    // Const values
    private const string WhiteColor = "#FFFFFF";
    private const string DefaultImageName = "logo.png";
    private const string DefaultImageRelativeFilePath = "/images/logo.png";
    private const string DefaultImageAbsoluteFilePath = "C:/Users/Oleg/Desktop/KG/ComputerGraphics/WebApp/wwwroot/images/logo.png";
    private const string ImagesFolderRelativePath = "/images/";
    private const string ImagesFolderAbsolutePath = "C:/Users/Oleg/Desktop/KG/ComputerGraphics/WebApp/wwwroot/images";
    
    public ColorSchemes()
    {
        ViewModel = new ColorSchemesViewModel();
    }
    
    public class ColorSchemesViewModel
    {
        // Image
        public IFormFile? ImageFile { get; set; }
        public string? ImageFileRelativePath { get; set; }
        public byte[] ImageBytes { get; set; } = null!;
        public int GrayColorLightness { get; set; }
        public int ImageFromX { get; set; }
        public int ImageFromY { get; set; }
        public int ImageToX { get; set; }
        public int ImageToY { get; set; }
        
        // Colors
        public string Cmyk { get; set; } = string.Empty;
        public string Hsl { get; set; } = string.Empty;
        
        // CMYK color components
        public int C { get; set; }
        public int M { get; set; }
        public int Y { get; set; }
        public int K { get; set; }
        
        // HSL color components
        public int H { get; set; }
        public int S { get; set; }
        public int L { get; set; }
    }

    [BindProperty]
    public ColorSchemesViewModel ViewModel { get; set; }
    
    public void OnGet()
    {
        _imageFileRelativePath = DefaultImageRelativeFilePath;
        ViewModel.ImageFileRelativePath = _imageFileRelativePath;
        var imageBitmap = new Bitmap(DefaultImageAbsoluteFilePath);
        ViewModel.ImageToX = imageBitmap.Width;
        ViewModel.ImageToY = imageBitmap.Height;
        
        // White as a default color
        ViewModel.Cmyk = WhiteColor;
        ViewModel.Hsl = WhiteColor;
        ViewModel.L = 100;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // If user set new image file
        if (ViewModel.ImageFile != null && ViewModel.ImageFile.FileName != _imageFileFullName)
        {
            // Save user image
            _imageFileGuid = Guid.NewGuid() + "_";
            _imageFileName = ViewModel.ImageFile.FileName;
            _imageFileFullName = _imageFileGuid + _imageFileName;
            _imageFileRelativePath = Path.Combine(ImagesFolderRelativePath, _imageFileFullName);
            _imageFileAbsolutePath = Path.Combine(ImagesFolderAbsolutePath, _imageFileFullName);
            await using var fileStreamInitial = new FileStream(_imageFileAbsolutePath, FileMode.Create);
            await ViewModel.ImageFile.CopyToAsync(fileStreamInitial);
            fileStreamInitial.Close();
        }
        
        if (_imageFileName == null)
        {
            _imageFileName = DefaultImageName;
            _imageFileGuid = string.Empty;
            _imageFileRelativePath = DefaultImageRelativeFilePath;
            _imageFileAbsolutePath = DefaultImageAbsoluteFilePath;
        }

        // TODO: change Bitmap to a cross-platform alternative
        var imageBitmap = new Bitmap(_imageFileAbsolutePath);
        
        // Starting coordinates
        int fromX = ViewModel.ImageFromX;
        fromX = fromX < 0 ? 0 : fromX;
        fromX = fromX >= imageBitmap.Width ? imageBitmap.Width - 1 : fromX;
        int fromY = ViewModel.ImageFromY;
        fromY = fromY < 0 ? 0 : fromY;
        fromY = fromY >= imageBitmap.Height ? imageBitmap.Height - 1 : fromY;
        
        // Ending coordinates
        int toX = ViewModel.ImageToX;
        toX = toX < 0 ? 0 : toX;
        toX = toX > imageBitmap.Width ? imageBitmap.Width : toX;
        int toY = ViewModel.ImageToY;
        toY = toY < 0 ? 0 : toY;
        toY = toY > imageBitmap.Height ? imageBitmap.Height : toY;

            
        for (int i = fromX; i < toX; ++i)
        {
            for (int j = fromY; j < toY; ++j)
            {
                var pixel = imageBitmap.GetPixel(i, j);
                
                // Red component
                int r = pixel.R + ViewModel.GrayColorLightness;
                r = r < 0 ? 0 : r;
                r = r > 255 ? 255 : r;
                
                // Green component
                int g = pixel.G + ViewModel.GrayColorLightness;
                g = g < 0 ? 0 : g;
                g = g > 255 ? 255 : g;
                
                // Blue component
                int b = pixel.B + ViewModel.GrayColorLightness;
                b = b < 0 ? 0 : b;
                b = b > 255 ? 255 : b;

                var updatedColor = Color.FromArgb(r, g, b);
                imageBitmap.SetPixel(i, j, updatedColor);
            }
        }
        
        // Save and display updated image
        _imageFileGuid = Guid.NewGuid() + "_";
        _imageFileFullName = _imageFileGuid + _imageFileName;
        _imageFileRelativePath = Path.Combine(ImagesFolderRelativePath, _imageFileFullName);
        _imageFileAbsolutePath = Path.Combine(ImagesFolderAbsolutePath, _imageFileFullName);
        await using var fileStreamUpdated = new FileStream(_imageFileAbsolutePath, FileMode.Create);
        imageBitmap.Save(fileStreamUpdated, ImageFormat.Png);
        fileStreamUpdated.Close();
        
        ViewModel.ImageFileRelativePath = _imageFileRelativePath!;
        ViewModel.ImageBytes = BitmapToByteArray(imageBitmap);

        // CMYK
        ViewModel.Cmyk = GetHexFromCmyk(ViewModel.C, ViewModel.M, ViewModel.Y, ViewModel.K);
        
        // HSL
        ViewModel.Hsl = GetHexFromHsl(ViewModel.H, ViewModel.S, ViewModel.L);

        return Page();
    }
    
    private static byte[] BitmapToByteArray(Bitmap bitmapImage)
    {
        return (byte[])new ImageConverter().ConvertTo(bitmapImage, typeof(byte[]))!;
    }
    
    private static string GetHexFromCmyk(int c, int m, int y, int k)
    {
        double cTemp = c / 100.0;
        double mTemp = m / 100.0;
        double yTemp = y / 100.0;
        double kTemp = k / 100.0;

        // Converting to RGB
        double r = 255 * (1 - cTemp) * (1 - kTemp);
        double g = 255 * (1 - mTemp) * (1 - kTemp);
        double b = 255 * (1 - yTemp) * (1 - kTemp);
        
        // Converting to hex
        var color = Color.FromArgb((int)Math.Ceiling(r), (int)Math.Ceiling(g), (int)Math.Ceiling(b));
        var hex = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        return hex;
    }

    private static string GetHexFromHsl(int h, int s, int l)
    {
        double sTemp = s / 100.0;
        double lTemp = l / 100.0;
        
        // Converting to RGB
        var c = (1 - Math.Abs(2 * lTemp - 1)) * sTemp;
        var x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
        var m = lTemp - c / 2;

        double r, g, b;
        if (h < 60)
        {
            (r, g, b) = (c, x, 0);
        }
        else if (h < 120)
        {
            (r, g, b) = (x, c, 0);
        }
        else if (h < 180)
        {
            (r, g, b) = (0, c, x);
        }
        else if (h < 240)
        {
            (r, g, b) = (0, x, c);
        }
        else if (h < 300)
        {
            (r, g, b) = (x, 0, c);
        }
        else
        {
            (r, g, b) = (c, 0, x);
        }
        (r, g, b) = ((r + m) * 255, (g + m) * 255, (b + m) * 255);
        
        // Converting to hex
        var color = Color.FromArgb((int)Math.Ceiling(r), (int)Math.Ceiling(g), (int)Math.Ceiling(b));
        var hex = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        return hex;
    }
}
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

[BindProperties]
public class ColorSchemes : PageModel
{
    // Statics
    private static string? _imageFileGuid;
    private static string? _imageFileName;
    private static string? _imageFileFullName;
    private static string? _imageFileAbsolutePath;
    private static string? _imageFileRelativePath;

    public static byte[]? ImageBytes;
    public static int CurrentC;
    public static int CurrentM;
    public static int CurrentY;
    public static int CurrentK;
    public static int CurrentH;
    public static int CurrentS;
    public static int CurrentL;

    // Const values
    private const string WhiteColor = "#FFFFFF";
    private const string DefaultImageName = "logo.png";
    private const string DefaultImageRelativeFilePath = "/images/logo.png";
    private const string DefaultImageAbsoluteFilePath = "C:/Users/Oleg/Desktop/KG/ComputerGraphics/WebApp/wwwroot/images/logo.png";
    private const string ImagesFolderRelativePath = "/images/";
    private const string ImagesFolderAbsolutePath = "C:/Users/Oleg/Desktop/KG/ComputerGraphics/WebApp/wwwroot/images";
    private const double EqualityPrecision = 0.001;
    
    public ColorSchemes()
    {
        ImageViewModel = new ColorSchemesImageViewModel();
        CmykViewModel = new ColorSchemesCmykViewModel();
        HslViewModel = new ColorSchemesHslViewModel();
    }
    
    public class ColorSchemesImageViewModel
    {
        // Image
        public IFormFile? ImageFile { get; set; }
        public string? ImageFileRelativePath { get; set; }
        public int GrayColorLightness { get; set; }
        public int ImageFromX { get; set; }
        public int ImageFromY { get; set; }
        public int ImageToX { get; set; }
        public int ImageToY { get; set; }
    }

    public class ColorSchemesCmykViewModel
    {
        public string CmykHex { get; set; } = string.Empty;
        
        // CMYK color components
        public int C { get; set; }
        public int M { get; set; }
        public int Y { get; set; }
        public int K { get; set; }
    }
    
    public class ColorSchemesHslViewModel
    {
        public string HslHex { get; set; } = string.Empty;
        
        // HSL color components
        public int H { get; set; }
        public int S { get; set; }
        public int L { get; set; }
    }

    public ColorSchemesImageViewModel ImageViewModel { get; set; }
    public ColorSchemesCmykViewModel CmykViewModel { get; set; }
    public ColorSchemesHslViewModel HslViewModel { get; set; }
    
    public void OnGet()
    {
        _imageFileRelativePath = DefaultImageRelativeFilePath;
        ImageViewModel.ImageFileRelativePath = _imageFileRelativePath;
        var imageBitmap = new Bitmap(DefaultImageAbsoluteFilePath);
        ImageBytes = BitmapToByteArray(imageBitmap);
        ImageViewModel.ImageToX = imageBitmap.Width;
        ImageViewModel.ImageToY = imageBitmap.Height;
        
        // White as a default color
        CmykViewModel.CmykHex = WhiteColor;
        HslViewModel.HslHex = WhiteColor;
        HslViewModel.L = 100;
    }

    public async Task<IActionResult> OnPostImage()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // If user set new image file
        if (ImageViewModel.ImageFile != null && ImageViewModel.ImageFile.FileName != _imageFileFullName)
        {
            // Save user image
            _imageFileGuid = Guid.NewGuid() + "_";
            _imageFileName = ImageViewModel.ImageFile.FileName;
            _imageFileFullName = _imageFileGuid + _imageFileName;
            _imageFileRelativePath = Path.Combine(ImagesFolderRelativePath, _imageFileFullName);
            _imageFileAbsolutePath = Path.Combine(ImagesFolderAbsolutePath, _imageFileFullName);
            await using var fileStreamInitial = new FileStream(_imageFileAbsolutePath, FileMode.Create);
            await ImageViewModel.ImageFile.CopyToAsync(fileStreamInitial);
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
        int fromX = ImageViewModel.ImageFromX;
        fromX = fromX < 0 ? 0 : fromX;
        fromX = fromX >= imageBitmap.Width ? imageBitmap.Width - 1 : fromX;
        int fromY = ImageViewModel.ImageFromY;
        fromY = fromY < 0 ? 0 : fromY;
        fromY = fromY >= imageBitmap.Height ? imageBitmap.Height - 1 : fromY;
        
        // Ending coordinates
        int toX = ImageViewModel.ImageToX;
        toX = toX < 0 ? 0 : toX;
        toX = toX > imageBitmap.Width ? imageBitmap.Width : toX;
        int toY = ImageViewModel.ImageToY;
        toY = toY < 0 ? 0 : toY;
        toY = toY > imageBitmap.Height ? imageBitmap.Height : toY;

            
        for (int i = fromX; i < toX; ++i)
        {
            for (int j = fromY; j < toY; ++j)
            {
                var pixel = imageBitmap.GetPixel(i, j);
                
                // Red component
                int r = pixel.R + ImageViewModel.GrayColorLightness;
                r = r < 0 ? 0 : r;
                r = r > 255 ? 255 : r;
                
                // Green component
                int g = pixel.G + ImageViewModel.GrayColorLightness;
                g = g < 0 ? 0 : g;
                g = g > 255 ? 255 : g;
                
                // Blue component
                int b = pixel.B + ImageViewModel.GrayColorLightness;
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
        
        ImageViewModel.ImageFileRelativePath = _imageFileRelativePath;
        ImageBytes = BitmapToByteArray(imageBitmap);

        return Page();
    }

    public IActionResult OnPostCmyk()
    {
        GetRgbFromCmyk(CmykViewModel.C, CmykViewModel.M, CmykViewModel.Y, CmykViewModel.K, out int r, out int g, out int b);
        CmykViewModel.CmykHex = GetHexFromRgb(r, g, b);
        GetHslFromRgb(r, g, b, out int h, out int s, out int l);
        CurrentC = CmykViewModel.C;
        CurrentM = CmykViewModel.M;
        CurrentY = CmykViewModel.Y;
        CurrentK = CmykViewModel.K;
        CurrentH = h;
        CurrentS = s;
        CurrentL = l;
        
        // Hex check
        GetRgbFromHsl(h, s, l, out r, out g, out b);
        HslViewModel.HslHex = GetHexFromRgb(r, g, b);
        return Page();
    }

    public IActionResult OnPostHsl()
    {
        GetRgbFromHsl(HslViewModel.H, HslViewModel.S, HslViewModel.L, out int r, out int g, out int b);
        HslViewModel.HslHex = GetHexFromRgb(r, g, b);
        GetCmykFromRgb(r, g, b, out int c, out int m, out int y, out int k);
        CurrentH = HslViewModel.H;
        CurrentS = HslViewModel.S;
        CurrentL = HslViewModel.L;
        CurrentC = c;
        CurrentM = m;
        CurrentY = y;
        CurrentK = k;
        
        // Hex check
        GetRgbFromCmyk(c, m, y, k, out r, out g, out b);
        CmykViewModel.CmykHex = GetHexFromRgb(r, g, b);
        return Page();
    }
    
    private static byte[] BitmapToByteArray(Bitmap bitmapImage)
    {
        return (byte[])new ImageConverter().ConvertTo(bitmapImage, typeof(byte[]))!;
    }
    
    private static void GetCmykFromRgb(int r, int g, int b, out int c, out int m, out int y, out int k)
    {
        double rTemp = r / 255.0;
        double gTemp = g / 255.0;
        double bTemp = b / 255.0;
        
        // K component
        double kTemp = 1 - Math.Max(rTemp, Math.Max(gTemp, bTemp));
        k = (int)Math.Ceiling(kTemp * 100);
        
        // C component
        double cTemp = (1 - rTemp - kTemp) / (1 - kTemp);
        c = (int)Math.Ceiling(cTemp * 100);
        
        // M component
        double mTemp = (1 - gTemp - kTemp) / (1 - kTemp);
        m = (int)Math.Ceiling(mTemp * 100);
        
        // Y component
        double yTemp = (1 - bTemp - kTemp) / (1 - kTemp);
        y = (int)Math.Ceiling(yTemp * 100);
    }
    
    private static void GetHslFromRgb(int r, int g, int b, out int h, out int s, out int l)
    {
        double rTemp = r / 255.0;
        double gTemp = g / 255.0;
        double bTemp = b / 255.0;

        double cMax = Math.Max(rTemp, Math.Max(gTemp, bTemp));
        double cMin = Math.Min(rTemp, Math.Min(gTemp, bTemp));
        double delta = cMax - cMin;

        // H component
        if (delta == 0)
        {
            h = 0;
        }
        else if (Math.Abs(cMax - rTemp) < EqualityPrecision)
        {
            h = (int)Math.Ceiling(60 * ((gTemp - bTemp) / delta % 6));
        }
        else if (Math.Abs(cMax - gTemp) < EqualityPrecision)
        {
            h = (int)Math.Ceiling(60 * ((bTemp - rTemp) / delta + 2));
        }
        else
        {
            h = (int)Math.Ceiling(60 * ((rTemp - gTemp) / delta + 4));
        }
        
        // L Component
        double lTemp = (cMax + cMin) / 2;
        l = (int)Math.Ceiling(lTemp * 100);
        
        // S component
        s = delta == 0 ? 0 : (int)Math.Ceiling(delta / (1 - Math.Abs(2 * lTemp - 1)) * 100);
    }
    
    private static void GetRgbFromCmyk(int c, int m, int y, int k, out int r, out int g, out int b)
    {
        double cTemp = c / 100.0;
        double mTemp = m / 100.0;
        double yTemp = y / 100.0;
        double kTemp = k / 100.0;

        // Converting to RGB
        double rTemp = 255 * (1 - cTemp) * (1 - kTemp);
        double gTemp = 255 * (1 - mTemp) * (1 - kTemp);
        double bTemp = 255 * (1 - yTemp) * (1 - kTemp);

        (r, g, b) = ((int)Math.Ceiling(rTemp), (int)Math.Ceiling(gTemp), (int)Math.Ceiling(bTemp));
    }

    private static void GetRgbFromHsl(int h, int s, int l, out int r, out int g, out int b)
    {
        double sTemp = s / 100.0;
        double lTemp = l / 100.0;
        
        // Converting to RGB
        var c = (1 - Math.Abs(2 * lTemp - 1)) * sTemp;
        var x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
        var m = lTemp - c / 2;

        double rTemp, gTemp, bTemp;
        if (h < 60)
        {
            (rTemp, gTemp, bTemp) = (c, x, 0);
        }
        else if (h < 120)
        {
            (rTemp, gTemp, bTemp) = (x, c, 0);
        }
        else if (h < 180)
        {
            (rTemp, gTemp, bTemp) = (0, c, x);
        }
        else if (h < 240)
        {
            (rTemp, gTemp, bTemp) = (0, x, c);
        }
        else if (h < 300)
        {
            (rTemp, gTemp, bTemp) = (x, 0, c);
        }
        else
        {
            (rTemp, gTemp, bTemp) = (c, 0, x);
        }
        (rTemp, gTemp, bTemp) = ((rTemp + m) * 255, (gTemp + m) * 255, (bTemp + m) * 255);

        (r, g, b) = ((int)Math.Ceiling(rTemp), (int)Math.Ceiling(gTemp), (int)Math.Ceiling(bTemp));
    }

    private static string GetHexFromRgb(int r, int g, int b)
    {
        var color = Color.FromArgb(r, g, b);
        var hex = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        return hex;
    }
}
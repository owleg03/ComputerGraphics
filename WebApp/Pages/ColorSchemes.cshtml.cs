using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class ColorSchemes : PageModel
{
    private const string WhiteColor = "#FFFFFF";
    
    public ColorSchemes()
    {
        ViewModel = new ColorSchemesViewModel();
    }
    
    public class ColorSchemesViewModel
    {
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
        // White as a default color
        ViewModel.Cmyk = WhiteColor;
        ViewModel.Hsl = WhiteColor;
        ViewModel.L = 100;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        // CMYK
        ViewModel.Cmyk = GetHexFromCmyk(ViewModel.C, ViewModel.M, ViewModel.Y, ViewModel.K);
        
        // HSL
        ViewModel.Hsl = GetHexFromHsl(ViewModel.H, ViewModel.S, ViewModel.L);

        return Page();
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
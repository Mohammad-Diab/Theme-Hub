using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ThemeController.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ThemesController : ControllerBase
    {
        // Settings from the "Themes" section of appsettings.json
        readonly string ThemeStoragePath;
        readonly string BaseThemesUrl;
        readonly string DefaultThemeName;
        const string SelectedThemeCookie = "SelectedTheme";

        public ThemesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            // A relative path is relative to the API folder
            ThemeStoragePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, configuration["Themes:StoragePath"] ?? "../themes"));
            BaseThemesUrl = configuration["Themes:BaseUrl"] ?? "/themes/";
            DefaultThemeName = configuration["Themes:DefaultTheme"] ?? "Light";
        }

        private static readonly Theme DefaultTheme = new Theme()
        {
            ThemeName = "new_theme",
            BodyBackgroundColor = "#eee",
            BodyColor = "#000",
            ControlBorderRadius = "3px",
            ControlBoxShadowColor = "#0000000d",
            ControlBackgroundColor = "#fff",
            PrimaryColor = "#fff",
            SecondaryColor = "#404040",
            BorderColor = "#ccc",
            IconLinkBorderRadius = "50%",
            HeadersColor = "#737373",
            NavbarColor = "#fff",
            TableOddBgColor = "#f5f5f5",
            TableHoverColor = "#404040",
            TableHoverBgColor = "#f0f0f0",
            TableBorderColor = "#dee2e6",
            DetailColor = "#999",
            DisabledColor = "#878787",
            LinkHoverColor = "#5a93ee",
            LinkFocusColor = "#327bf1",
            Primary = "#4285f4",
            Secondary = "#5e5e5e",
            Success = "#34a853",
            Warning = "#fbbc05",
            Danger = "#ea4335",
        };

        [HttpGet]
        [ActionName("GetDefaultTheme")]
        public Theme GetDefaultTheme()
        {
            return DefaultTheme;
        }

        [HttpPost]
        [ActionName("AddNewTheme")]
        public async Task<bool> AddNewTheme([FromBody] Theme theme)
        {
            string inputFile = await CreateSCSS(theme);
            bool result = false;
            if (!string.IsNullOrEmpty(inputFile))
            {
                result = CompileSCSS(inputFile, theme.ThemeName);
            }
            return result;
        }

        async Task<string> CreateSCSS(Theme theme)
        {
            string ThemeText =
    @$"@use ""basic"" with (
    $theme-name: ""{theme.ThemeName}"", 
    $colors: (""primary"": {theme.Primary}, ""secondary"":{theme.Secondary}, ""success"": {theme.Success}, ""warning"": {theme.Warning}, ""danger"": {theme.Danger}),
    $body-background-color: {theme.BodyBackgroundColor},
    $body-color: {theme.BodyColor},
    $control-border-radius: {theme.ControlBorderRadius},
    $control-box-shadow-color: {theme.ControlBoxShadowColor},
    $control-background-color: {theme.ControlBackgroundColor},
    $primary-color: {theme.PrimaryColor},
    $secondary-color: {theme.SecondaryColor},
    $border-color: {theme.BorderColor},
    $icon-link-border-radius: {theme.IconLinkBorderRadius},
    $headers-color: {theme.HeadersColor},
    $navbar-color: {theme.NavbarColor},
    $table-odd-bg-color: {theme.TableOddBgColor},
    $table-hover-color: {theme.TableHoverColor},
    $table-hover-bg-color: {theme.TableHoverBgColor},
    $table-border-color: {theme.TableBorderColor},
    $detail-color: {theme.DetailColor},
    $disabled-color: {theme.DisabledColor},
    $link-hover-color: {theme.LinkHoverColor},
    $link-focus-color: {theme.LinkFocusColor}
);";
            if (string.IsNullOrWhiteSpace(theme.ThemeName))
                return "";
            if (Directory.Exists(ThemeStoragePath))
            {
                if (Directory.Exists(Path.Combine(ThemeStoragePath, "scss")))
                {
                    string fileName = Path.Combine(ThemeStoragePath, "scss", theme.ThemeName + ".scss");
                    if (!Path.GetInvalidFileNameChars().Any((x) => theme.ThemeName.Any((y) => y == x)) && !System.IO.File.Exists(fileName))
                    {
                        try
                        {
                            using (var stream = System.IO.File.CreateText(fileName))
                            {
                                await stream.WriteAsync(ThemeText);
                                stream.Close();
                                return fileName;
                            }
                        }
                        catch (Exception)
                        {
                            return "";
                        }
                    }
                }
            }
            return "";
        }

        bool CompileSCSS(string inputFile, string themeName)
        {
            string outputPath = Path.Combine(ThemeStoragePath, themeName);
            bool folderExisted = Directory.Exists(outputPath);
            var outputFolder = Directory.CreateDirectory(outputPath);
            string outputFile = Path.Combine(outputFolder.FullName, themeName + ".css");
            string sassFile = Path.Combine(ThemeStoragePath, "scss", "dart-sass", "sass.bat");
            bool result = false;
            Task t = new Task(() =>
            {
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.FileName = "cmd.exe";
                        // sass.bat is called by its full path, so it does not depend on the current folder
                        process.StartInfo.Arguments = $"/c \"\"{sassFile}\" \"{inputFile}\" \"{outputFile}\"\"";
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.Start();

                        var e = process.StandardError.ReadToEndAsync();
                        var o = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        // Sass returns 0 only when the theme compiled
                        result = process.ExitCode == 0 && System.IO.File.Exists(outputFile);
                    }
                }
                catch (Exception)
                {
                    result = false;
                }
            });
            t.Start();
            t.Wait();

            if (!result)
            {
                // Remove the files of the failed theme, so it does not show in the list and the name can be used again
                System.IO.File.Delete(inputFile);
                if (!folderExisted)
                    Directory.Delete(outputPath, true);
            }
            return result;
        }
        
        [HttpGet]
        [ActionName("GetThemesList")]
        public List<ThemeItem> GetThemesList()
        {
            DirectoryInfo themesDir = new DirectoryInfo(ThemeStoragePath);
            DirectoryInfo[] themsDirs = themesDir.GetDirectories();
            List<ThemeItem> result = new List<ThemeItem>();
            // Each browser keeps its own selected theme in a cookie; without one, the default theme is selected
            string selectedTheme = Request.Cookies[SelectedThemeCookie] ?? DefaultThemeName;

            foreach (var item in themsDirs)
            {
                if (item.Name.ToLower() == "shared" || item.Name.ToLower() == "scss")
                    continue;
                result.Add(new ThemeItem(item.Name, item.Name, item.Name.ToLower() == selectedTheme.ToLower()));
            }
            return result;
        }

        [HttpGet]
        [ActionName("GetThemeUrl")]
        public string GetThemeUrl(string ThemeId)
        {
            if (string.IsNullOrWhiteSpace(ThemeId))
                ThemeId = DefaultThemeName;
            Response.Cookies.Append(SelectedThemeCookie, ThemeId, new CookieOptions() { Expires = DateTimeOffset.Now.AddYears(1) });
            return $"{BaseThemesUrl}{ThemeId}/{ThemeId}.css";
        }
    }
}
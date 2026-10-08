# Theme Hub

Create themes once and serve them to all your products, with a theme per customer.

A company with several web products wants them to look like one family, and each customer wants the products in their own colours. Theme Hub keeps **one design source**, a Sass template (`themes/scss/basic.scss`) that styles every page element from about 25 colour and shape settings. An **API generates a theme** from those settings, one per customer or product, and compiles it to a plain CSS file in a central **theme store**. **Every site just links the generated CSS** from the store's address: change the design once in the template, and every product and every customer theme follows.

Written in 2020 with ASP.NET Core 3.1, Sass and jQuery, as a portfolio demo.

## How it works

- **The template** `themes/scss/basic.scss` holds the whole design: layout, navbar, cards, tables, buttons, links, and the colours as Sass variables with defaults (`$body-background-color`, `$primary-color`, `$colors` and the rest).
- **A theme** is a small `.scss` file in `themes/scss` that loads the template with its own values (`@use "basic" with (...)`), compiled to `themes/<Name>/<Name>.css`. `Light` and `Dark` are the base themes; `Ocean` and `Forest` are examples created through the API.
- **The API** (`api`, project `ThemeController`) creates themes and tells the sites where they are:

  | Call | What it does |
  |---|---|
  | `GET /Themes/GetDefaultTheme` | The settings of a new theme, with their default values |
  | `POST /Themes/AddNewTheme` | Takes the settings as JSON (`themeName`, `primary`, `bodyBackgroundColor`, ...), writes `<themeName>.scss` and compiles it with the bundled dart-sass. Returns `true`, or `false` when the name is taken or invalid or Sass cannot compile the values (nothing is left behind then) |
  | `GET /Themes/GetThemesList` | The themes in the store, with the one this browser selected (kept in a cookie; the default theme otherwise) |
  | `GET /Themes/GetThemeUrl?ThemeId=Ocean` | The address of the theme's CSS, for example `/themes/Ocean/Ocean.css` |

- **The theme store** (`themes`) is a plain folder served by any web server. Besides the themes it holds what all the sites share: `shared/shared-script.js` (the API calls), jQuery, the icon font (Material Design Iconic Font) and the logo.
- **Theme Creator** (`theme-creator`) is the page that creates a theme: fill in the settings, click **Add**, and the new theme appears in the theme list.
- **Website 01** (`sample-site`) is a users page that stands for any product, with two pages of made-up users. It loads the list of themes into the selector in the middle of the header (drawn by the theme itself) and switches its theme by changing one `<link>` to the address the API returns.

### Why Sass instead of CSS variables

The products had to support Internet Explorer 11, which never supported CSS custom properties (`var(--primary)`). So a theme cannot be a set of variables changed in the browser: each theme is compiled on the server into plain CSS that every browser understands, and switching themes means switching one stylesheet.

## Layout

| Folder | What it is |
|---|---|
| `api` | The ThemeController Web API (ASP.NET Core 3.1) and its settings, `appsettings.json` |
| `themes` | The theme store: `scss` (the template, one `.scss` per theme and dart-sass), one folder per compiled theme, and `shared` |
| `theme-creator` | The Theme Creator page |
| `sample-site` | Website 01, a sample product page that uses the themes |
| `.vscode` | Build and launch settings for running the API from Visual Studio Code |

## Settings

The API reads its settings from `api/appsettings.json` (or environment variables such as `Themes__StoragePath`):

| Setting | Default | Meaning |
|---|---|---|
| `Themes:StoragePath` | `../themes` | The theme store folder. A relative path is relative to the API folder |
| `Themes:BaseUrl` | `/themes/` | The address the sites load the store from. Use a full address (`https://themes.example.com/`) when the store has its own host |
| `Themes:DefaultTheme` | `Light` | The theme selected for a browser that has not chosen one |
| `StaticFolders` | `themes`, `sample-site`, `theme-creator` | Folders the API serves itself (address = name, value = folder), so everything runs from one address without IIS. Empty it when IIS or another server hosts them |

The sites need one setting: `serverUrl` at the top of `themes/shared/shared-script.js`, the address of the API (`/Themes/` when the API serves the sites). The pages load the store from `../themes/`, the folder next to them; when the store lives elsewhere, change those links in `index.htm`.

## Run the release

Needs Windows 10 or 11 (x64) and the **ASP.NET Core Runtime 8.0 or later** ([download](https://dotnet.microsoft.com/download/dotnet), "ASP.NET Core Runtime", Windows x64). The code targets .NET Core 3.1, which is out of support; `Start.cmd` lets it run on the newer runtime you have.

1. From the [Releases](https://github.com/Mohammad-Diab/Theme-Hub/releases) page, download `Theme-Hub-v1.0.0-win-x64.7z` and extract it with [7-Zip](https://www.7-zip.org).
2. In the `Theme-Hub` folder, run `Start.cmd`. It starts the API on `http://localhost:5080` and opens Website 01 and Theme Creator. Close the "Theme Hub" window to stop it.
3. In Website 01, pick a theme in the **Theme** selector in the middle of the header.
4. In Theme Creator, change some colours, type a name in `ThemeName` and click **Add**. The new theme is now in the list of both pages.

## Run from source

Needs the .NET SDK 8.0 or later (it builds the `netcoreapp3.1` project with an out-of-support warning).

```
cd api
dotnet run
```

`dotnet run` uses the `ThemeController` profile in `api/Properties/launchSettings.json`: `http://localhost:5000`, the newer runtime allowed (`DOTNET_ROLL_FORWARD=Major`), and it opens `http://localhost:5000/sample-site/`. Theme Creator is at `http://localhost:5000/theme-creator/`. In Visual Studio Code, open the repository folder and run ".NET Core Launch (web)".

To host it on IIS instead: publish the API as an application, serve `themes` and the two sites as folders or applications, set `Themes:StoragePath` to the store's folder and `Themes:BaseUrl` to its address, empty `StaticFolders`, and set `serverUrl` in `shared-script.js`. The API's application pool needs write access to the store, because it writes the new themes there.

## Known limits

- Anyone who can reach the API can create themes: there is no sign-in. Use it for demos, not on a public server.
- Themes cannot be edited or deleted through the API; edit the `.scss` file and compile it again, or remove the theme's folder and `.scss` file.
- The settings are passed to Sass as they are: a wrong value makes the compile fail and AddNewTheme return `false`.
- `StaticFolders` serves the whole store, `scss\dart-sass` included.

## History

The first commit is the 2020 code, with its machine paths and server address moved into settings and a few small changes: AddNewTheme reports a failed compile, Sass is called by its full path, each browser keeps its own selected theme, the two pages have a clear header (site name, page title, theme selector, links to each other) and working links, and the icons are the original Material Design Iconic Font files. The commits after it add `.gitignore`, replace the test themes with the Ocean and Forest examples, and add this README and the license.

## Credits

- [Dart Sass](https://sass-lang.com/dart-sass) 1.25.0, bundled in `themes/scss/dart-sass` (MIT, `src/SASS_LICENSE`; the Dart runtime, BSD, `src/DART_LICENSE`)
- [jQuery](https://jquery.com) 3.4.1 (MIT)
- [Material Design Iconic Font](https://zavoloklom.github.io/material-design-iconic-font/) 2.2.0 by Sergey Kupletsky, the icons, unchanged from the [official release](https://github.com/zavoloklom/material-design-iconic-font/releases/tag/2.2.0) in `themes/shared/material-design-iconic-font` (font: SIL OFL 1.1, CSS: MIT; `LICENSE.txt` there)

## License

[Apache License 2.0](LICENSE). The bundled third-party files keep their own licenses (see Credits).

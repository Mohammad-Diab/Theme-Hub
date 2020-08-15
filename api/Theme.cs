using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThemeController
{
    public class Theme
    {

        public string ThemeName { get; set; }

        public string BodyBackgroundColor { get; set; }

        public string BodyColor { get; set; }

        public string ControlBorderRadius { get; set; }

        public string ControlBoxShadowColor { get; set; }

        public string ControlBackgroundColor { get; set; }

        public string PrimaryColor { get; set; }

        public string SecondaryColor { get; set; }

        public string BorderColor { get; set; }

        public string IconLinkBorderRadius { get; set; }

        public string HeadersColor { get; set; }

        public string NavbarColor { get; set; }

        public string TableOddBgColor { get; set; }

        public string TableHoverColor { get; set; }

        public string TableHoverBgColor { get; set; }

        public string TableBorderColor { get; set; }

        public string DetailColor { get; set; }

        public string DisabledColor { get; set; }

        public string LinkHoverColor { get; set; }

        public string LinkFocusColor { get; set; }

        public string Primary { get; set; }

        public string Secondary { get; set; }

        public string Success { get; set; }

        public string Warning { get; set; }

        public string Danger { get; set; }

        public Theme()
        {

        }
    }

    public class ThemeItem
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }
      
        public ThemeItem(string id,string name, bool isSelected)
        {
            Id = id;
            Name = name;
            IsSelected = isSelected;
        }
    }
}

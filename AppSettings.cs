using System.Collections.Generic;

namespace TaskBoardWf
{
    // TODO: Refactor AppSetting constructors

    public class AppSettings
    {
        public List<NameModifier> NameModifiers { get; set; }
        public byte ThumbnailOpacity { get; set; }
        public byte DeltaOpacity { get; set; }
        public bool BackgroundThumbnail { get; set; }


        // Configuration class to modify display of lblTaskName
        // If Pattern match, replace it with Substitution and set the Forecolor
        public class NameModifier
        {
            public string Pattern { get; set; }
            public string Substitution { get; set; }
            public string ForeColor { get; set; }

            public NameModifier()
            {
                Pattern = string.Empty;
                Substitution = string.Empty;
                ForeColor = string.Empty;
            }
        }

        public AppSettings()
        {
            NameModifiers = new List<NameModifier>();
            ThumbnailOpacity = 52;
            DeltaOpacity = 40;
            BackgroundThumbnail = false;
        }
    }
}

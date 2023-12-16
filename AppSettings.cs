using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace TaskBoardWf
{
    // TODO: Refactor AppSetting constructors

    // 設定を表すクラスの例
    public class AppSettings
    {
        public List<NameModifier> NameModifiers { get; set; }

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
        }
    }
}

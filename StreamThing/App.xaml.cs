using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.CommandLine;
using System.Windows;

namespace StreamThing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public int? ConfigWidth { get; private set; } = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var widthOption = new Option<int?>(new[] { "--width", "-w" });
            var cmd = new RootCommand();
            cmd.AddOption(widthOption);
            var parsed = cmd.Parse(e.Args);

            ConfigWidth = parsed.GetValueForOption(widthOption);
        }
    }
}

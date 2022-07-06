using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.CommandLine;
using System.Windows;
using System.IO;
using System.Diagnostics;

namespace StreamThing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public SettingsObject Settings = new SettingsObject();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamThing", "settings.json");
                Settings = JsonSerializer.Deserialize<SettingsObject>(File.ReadAllText(settingsPath));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            var widthOption = new Option<int?>(new[] { "--width", "-w" });
            var cmd = new RootCommand();
            cmd.AddOption(widthOption);
            var parsed = cmd.Parse(e.Args);

            Settings.Width = parsed.GetValueForOption(widthOption) ?? Settings.Width;
        }
    }
}

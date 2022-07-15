using System.CommandLine;
using System.Diagnostics;
using System.Reflection;

namespace AnimDL.Commands;

public static class ConfigureCommand
{
    public static Command Create()
    {
        var command = new Command("config", "configure options for application");
        command.SetHandler(() =>
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "appsettings.json"))
                {
                    UseShellExecute = true
                }
            };
            p.Start();
        });
        return command;
    }
}

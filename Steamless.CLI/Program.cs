using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Steamless.API;
using Steamless.API.Model;
using Steamless.API.Services;
using Steamless.API.Events;

class MainClass {
    private static readonly Version SteamlessApiVersion = new Version(1, 0);
    private LoggingService logService = new LoggingService();

    private static void AddLogMessage(object sender, LogMessageEventArgs e) {
        Console.WriteLine(e.Message);
    }
    public MainClass()
    {
        logService.AddLogMessage += AddLogMessage;
    }

    List<SteamlessPlugin> GetSteamlessPlugins()
    {
        try
        {
            // Obtain the view model locator..

            // The list of valid plugins..
            var plugins = new List<SteamlessPlugin>();

            // Build a path to the plugins folder..
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            Console.WriteLine("Trying to load plugins from " + path);

            // Loop the DLL files and attempt to load them..
            foreach (var dll in Directory.GetFiles(path, "*.dll"))
            {
                // Skip the Steamless.API.dll file..
                if (dll.ToLower().Contains("steamless.api.dll"))
                    continue;

                try
                {
                    // Load the assembly..
                    var asm = Assembly.Load(File.ReadAllBytes(dll));

                    // Locate the class inheriting the plugin base..
                    var baseClass = asm.GetTypes().SingleOrDefault(t => t.BaseType == typeof(SteamlessPlugin));
                    if (baseClass == null)
                    {
                        Console.WriteLine($"Failed to load plugin; could not find SteamlessPlugin base class. ({Path.GetFileName(dll)})");
                        continue;
                    }

                    // Locate the SteamlessApiVersion attribute on the base class..
                    var baseAttr = baseClass.GetCustomAttributes(typeof(SteamlessApiVersionAttribute), false);
                    if (baseAttr.Length == 0)
                    {
                        Console.WriteLine($"Failed to load plugin; could not find SteamlessApiVersion attribute. ({Path.GetFileName(dll)})");
                        continue;
                    }

                    // Validate the interface version..
                    var apiVersion = (SteamlessApiVersionAttribute)baseAttr[0];
                    if (apiVersion.Version != SteamlessApiVersion)
                    {
                        Console.WriteLine($"Failed to load plugin; invalid API version is being used. ({Path.GetFileName(dll)})");
                        continue;
                    }

                    // Create an instance of the plugin..
                    var plugin = (SteamlessPlugin)Activator.CreateInstance(baseClass);
                    if (!plugin.Initialize(logService))
                    {
                        Console.WriteLine($"Failed to load plugin; plugin failed to initialize. ({Path.GetFileName(dll)})");
                        continue;
                    }

                    plugins.Add(plugin);
                }
                catch
                {
                    Console.WriteLine($"Failed to load DLL as a Steamless plugin: ({Path.GetFileName(dll)})");
                }
            }

            // Order the plugins by their name..
            return plugins.OrderBy(p => p.Name).ToList();
        }
        catch
        {
            return new List<SteamlessPlugin>();
        }
    }



    public static void Main(string[] args) {
        if (args.Length != 1) {
            Console.WriteLine("Please pass an executable!");
            return;
        }
        SteamlessOptions options = new SteamlessOptions();
        String executable = args[0];

        MainClass main = new MainClass();
        List<SteamlessPlugin> plugins = main.GetSteamlessPlugins();
        if (plugins.Count == 0) {
            Console.WriteLine("Failed to load any plugins!");
            return;
        }
        SteamlessPlugin plugin = null;
        for (int i=0; i<plugins.Count; i++) {
            if (!plugins[i].CanProcessFile(executable)) {
                Console.WriteLine("Skipping " + plugins[i].Name);
                continue;
            }

            plugins[i].ProcessFile(executable, options);
        }
        if (plugin == null) {
            Console.WriteLine("Failed to find a plugin that could process " + executable);
            return;
        }

        Console.WriteLine("Decrypting with " + plugin.Name + " by " + plugin.Author);
        Console.WriteLine(plugin.Description);
    }
} // MainClass

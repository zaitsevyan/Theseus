//
//  File: Program.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Theseus {
    class MainClass {
        public static void Main(string[] args) {
            Console.Title = "Theseus platform";
            //Logging configuration
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"[${date:format=HH\:mm\:ss.fff}][${logger}.${level}]: ${message} ${exception:format=Message,StackTrace}";

            fileTarget.FileName = "${basedir}/logs/${date:format=yyyy.MM.dd}.txt";
            fileTarget.Layout = @"[${date:format=dd.MM.yyyy HH\:mm\:ss.fff}][${logger}.${level}]: ${message} ${exception:format=Message,StackTrace}";
            fileTarget.DeleteOldFileOnStartup = false;
            fileTarget.AutoFlush = true;

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Info, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;



            var core = new Core("configuration.json", "accounts.json");
            core.Start();
            core.Wait();
        }
    }
}

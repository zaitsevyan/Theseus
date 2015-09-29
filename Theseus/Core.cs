//
//  File: Core.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.Collections.Generic;

namespace Theseus {
    public class Core: ICore {
        private AdapterManager adapterManager;
        private ModuleManager moduleManager;
        private ManualResetEventSlim waitingEvent = new ManualResetEventSlim(false);
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Logger Logger { get; set;}
        private Accounts accounts;
        private Configuration configuration;

        public Core(String configurationFileName, String accountsFileName) {
            AppDomain.CurrentDomain.UnhandledException += 
                new UnhandledExceptionEventHandler(delegate(object sender, UnhandledExceptionEventArgs e) {
                    Logger.Error(e.ExceptionObject);
                });
            Logger = LogManager.GetLogger("Core");
            Logger.Info("Core initializing...");
            Logger.Info("Configuration initializing...");
            configuration = Configuration.Parse(configurationFileName, cancellationTokenSource.Token).Result;
            Logger.Info("Adapter manager initializing...");
            adapterManager = new AdapterManager(this, configuration.Adapters);
            adapterManager.PluginsDirectory = "./Adapters";
            Logger.Info("Module manager initializing...");
            moduleManager = new ModuleManager(this, configuration.Modules);
            moduleManager.PluginsDirectory = "./Modules";
            Logger.Info("Accounts initializing...");
            accounts = new Accounts(accountsFileName, cancellationTokenSource.Token);
            Logger.Info("Core initialized");
        }

        public void Start() {
            Logger.Info("Starting...");
            adapterManager.LoadPlugins();
            adapterManager.RunPlugins(cancellationTokenSource.Token);

            moduleManager.LoadPlugins();
            moduleManager.RunPlugins(cancellationTokenSource.Token);
        }
            
        public IAdapterManager GetAdapterManager(){
            return adapterManager;
        }

        public IModuleManager GetModuleManager(){
            return moduleManager;
        }

        public async Task Stop() {
            Logger.Info("Stopping...");
            cancellationTokenSource.Cancel();
            bool pluginsDisabled = false;
            var plugins = new List<Plugin>();
            plugins.AddRange(adapterManager.Plugins);
            plugins.AddRange(moduleManager.Plugins);
            var counter = 0;
            var abortTries = 5;
            while (!pluginsDisabled && counter <= abortTries + 1) {
                pluginsDisabled = true;
                counter++;
                await Task.Delay(500);
                foreach (var plugin in plugins) {
                    if (plugin.IsRunning && plugin.MainLoopThread.IsAlive) {
                        pluginsDisabled = false;

                        Logger.Warn("Adapter {0} still running...", plugin);
                        if (counter == abortTries) {
                            Logger.Warn("Abort {0}'s thread...", plugin);
                            plugin.MainLoopThread.Abort();
                        }
                        else if (counter > abortTries) {
                            Logger.Warn("Cannot do anything with {0}, ignoring...", plugin);
                        }
                    }
                }
            }

            waitingEvent.Set();
            Logger.Info("Stopped");
        }

        public void Wait() {

            waitingEvent.Wait();
        }

        public IAccounts GetAccountsDB(){
            return accounts;
        }
    }
}


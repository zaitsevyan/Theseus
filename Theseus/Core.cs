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
    /// <summary>
    /// Theseus.
    /// </summary>
    public class Core: ICore {
        /// <summary>
        /// The adapter manager.
        /// </summary>
        private AdapterManager adapterManager;

        /// <summary>
        /// The handler manager.
        /// </summary>
        private HandlerManager handlerManager;

        /// <summary>
        /// The waiting mutex.
        /// </summary>
        private ManualResetEventSlim waitingEvent = new ManualResetEventSlim(false);

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private Logger Logger { get; set;}

        /// <summary>
        /// The accounts subsystem.
        /// </summary>
        private Accounts accounts;

        /// <summary>
        /// Platform configuration.
        /// </summary>
        private Configuration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Theseus.Core"/> class.
        /// </summary>
        /// <param name="configurationFileName">Configuration file name.</param>
        /// <param name="accountsFileName">Accounts file name.</param>
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
            Logger.Info("Handler manager initializing...");
            handlerManager = new HandlerManager(this, configuration.Handlers);
            handlerManager.PluginsDirectory = "./Handlers";
            Logger.Info("Accounts initializing...");
            accounts = new Accounts(accountsFileName, cancellationTokenSource.Token);
            Logger.Info("Core initialized");
        }

        /// <summary>
        /// Start platform.
        /// </summary>
        public void Start() {
            Logger.Info("Starting...");
            adapterManager.LoadPlugins();
            adapterManager.RunPlugins(cancellationTokenSource.Token);

            handlerManager.LoadPlugins();
            handlerManager.RunPlugins(cancellationTokenSource.Token);
        }
        /// <summary>
        /// Gets the adapter manager.
        /// </summary>
        /// <returns>The adapter manager.</returns>
        public IAdapterManager GetAdapterManager(){
            return adapterManager;
        }

        /// <summary>
        /// Gets the handler manager.
        /// </summary>
        /// <returns>The handler manager.</returns>
        public IHandlerManager GetHandlerManager(){
            return handlerManager;
        }

        /// <summary>
        /// Stop platform.
        /// </summary>
        public async Task Stop() {
            Logger.Info("Stopping...");
            cancellationTokenSource.Cancel();
            bool pluginsDisabled = false;
            var plugins = new List<Plugin>();
            plugins.AddRange(adapterManager.Plugins);
            plugins.AddRange(handlerManager.Plugins);
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

        /// <summary>
        /// Wait until all plugins is ended. Yous should call Stop() to initiate shutdowning.
        /// </summary>
        public void Wait() {

            waitingEvent.Wait();
        }

        /// <summary>
        /// Gets the accountsDB subsystem.
        /// </summary>
        /// <returns>The accounts subsystem.</returns>
        public IAccounts GetAccountsDB(){
            return accounts;
        }
    }
}


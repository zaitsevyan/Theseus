//
//  File: PluginManager.cs
//  Created: 27.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.IO;
using System.Collections.Generic;
using Api;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.Reflection;

namespace Theseus {
    /// <summary>
    /// Plugin manager.
    /// </summary>
    public class PluginManager<T> : IPluginManager<T> where T : Plugin {
        /// <summary>
        /// List of loaded plugins.
        /// </summary>
        private List<T> plugins = new List<T>();

        /// <summary>
        /// Plugin configurations. See <see cref="Theseus.Configuration"/> class.
        /// </summary>
        protected List<Configuration.Plugin> Configs;

        /// <summary>
        /// Gets or sets the plugins directory path.
        /// </summary>
        /// <value>The plugins directory path.</value>
        public string PluginsDirectory { get; set; }

        /// <summary>
        /// Loaded assemblies.
        /// </summary>
        protected List<Assembly> Assemblies = new List<Assembly>();

        /// <summary>
        /// Gets the core.
        /// </summary>
        /// <value>Platform core.</value>
        protected ICore Core { get; private set; }

        /// <summary>
        /// Gets the list of loaded plugins.
        /// </summary>
        /// <value>List of loaded plugins.</value>
        public IReadOnlyCollection<T> Plugins { get { return plugins.AsReadOnly(); } }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected Logger Logger { get; set; }

        /// <summary>
        /// Gets the logger for specified plugin.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <param name="plugin">Plugin.</param>
        public Logger GetLogger(T plugin){
            return LogManager.GetLogger(typeof(T).Name + "." + plugin.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Theseus.PluginManager`1"/> class.
        /// </summary>
        /// <param name="core">Platform core.</param>
        /// <param name="configs">Plugin configurations. See <see cref="Theseus.Configuration"/> class.</param>
        public PluginManager(ICore core, List<Configuration.Plugin> configs) {
            PluginsDirectory = null;
            Core = core;
            Logger = LogManager.GetLogger(GetType().Name);
            Configs = configs ?? new List<Configuration.Plugin>();
        }

        /// <summary>
        /// Gets the platform core.
        /// </summary>
        /// <returns>The platform core.</returns>
        public ICore GetCore(){
            return Core;
        }

        /// <summary>
        /// Loads the assemblies from PluginsDirectory.
        /// </summary>
        private void LoadAssemblies(){
            if (Directory.Exists(PluginsDirectory)) {
                foreach (var dll in Directory.EnumerateFiles(PluginsDirectory,"*.dll", SearchOption.AllDirectories)) {
                    try {
                        var assembly = Assembly.LoadFile(new FileInfo(dll).FullName);
                        Assemblies.Add(assembly);
                        Logger.Debug("Assembly {0} loaded", assembly);
                    }
                    catch (FileLoadException e) {
                        Logger.Error(e);
                    }
                    catch (BadImageFormatException e) {
                        Logger.Error(e);
                    }
                }
            }
        }

        /// <summary>
        /// Lookups the plugin in loaded assemblies by name from plugin configuration.
        /// </summary>
        /// <returns>The plugin.</returns>
        /// <param name="name">Name.</param>
        protected Type LookupPlugin(String name){
            foreach (var assembly in Assemblies) {
                foreach (var type in assembly.GetExportedTypes()) {
                    if(type.Name == name && type.IsSubclassOf(typeof(T)))
                        return type;
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes the plugins from configuration.
        /// </summary>
        private void InitializePlugins() {
            foreach (var plugin in Configs) {
                var type = LookupPlugin(plugin.Class);
                if (type != null) {
                   AddPlugin((T)Activator.CreateInstance(type, new object[]{plugin.Config, this}));
                }
                else {
                    Logger.Error("Cannot find {0} plugin", plugin.Class);
                }
            }
        }

        /// <summary>
        /// Loads the assemblies and initializes plugins.
        /// </summary>
        public virtual void LoadPlugins(){
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolver);
            LoadAssemblies();
            InitializePlugins();
        }

        /// <summary>
        /// Adds the plugin.
        /// </summary>
        /// <param name="plugin">Plugin.</param>
        public virtual void AddPlugin(T plugin){
            plugins.Add(plugin);
        }

        /// <summary>
        /// Removes the plugin.
        /// </summary>
        /// <param name="plugin">Plugin.</param>
        public virtual void RemovePlugin(T plugin){
            plugins.Remove(plugin);
        }

        /// <summary>
        /// Runs the plugins.
        /// </summary>
        /// <param name="token">Token.</param>
        public void RunPlugins(CancellationToken token){
            foreach (var plugin in Plugins) {
                Run(plugin, token);
            }
        }

        /// <summary>
        /// Run the specified plugin.
        /// </summary>
        /// <param name="plugin">Plugin.</param>
        /// <param name="token">Cancellation token.</param>
        protected void Run(T plugin, CancellationToken token){
            Task.Factory.StartNew(() => {
                    Logger.Info("{0} starting...", plugin);
                    plugin.Start(token);
                });
        }

        /// <summary>
        /// Assembliy resolver for PluginDirectory path.
        /// </summary>
        /// <returns>The resolved assembly.</returns>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Resolve event arguments.</param>
        private Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, PluginsDirectory, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath) == false) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
    }
}


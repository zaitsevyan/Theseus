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
    public class PluginManager<T> : IPluginManager<T> where T : Plugin {
        private List<T> plugins = new List<T>();
        protected List<Configuration.Plugin> Configs;

        public string PluginsDirectory { get; set; }

        protected List<Assembly> Assemblies = new List<Assembly>();

        protected ICore Core { get; private set; }

        public IReadOnlyCollection<T> Plugins { get { return plugins.AsReadOnly(); } }

        protected Logger Logger { get; set; }

        public Logger GetLogger(T adapter){
            return LogManager.GetLogger(typeof(T).Name + "." + adapter.Name);
        }

        public PluginManager(ICore core, List<Configuration.Plugin> configs) {
            PluginsDirectory = null;
            Core = core;
            Logger = LogManager.GetLogger(GetType().Name);
            Configs = configs ?? new List<Configuration.Plugin>();
        }

        public ICore GetCore(){
            return Core;
        }

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

        protected Type LookupPlugin(String name){
            foreach (var assembly in Assemblies) {
                foreach (var type in assembly.GetExportedTypes()) {
                    if(type.Name == name && type.IsSubclassOf(typeof(T)))
                        return type;
                }
            }
            return null;
        }

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

        public virtual void LoadPlugins(){
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolver);
            LoadAssemblies();
            InitializePlugins();
        }

        public virtual void AddPlugin(T plugin){
            plugins.Add(plugin);
        }

        public virtual void RemovePlugin(T plugin){
            plugins.Remove(plugin);
        }

        public void RunPlugins(CancellationToken token){
            foreach (var plugin in Plugins) {
                Run(plugin, token);
            }
        }

        protected void Run(T plugin, CancellationToken token){
            Task.Factory.StartNew(() => {
                    Logger.Info("{0} starting...", plugin);
                    plugin.Start(token);
                });
        }

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


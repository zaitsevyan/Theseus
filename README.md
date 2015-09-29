# Theseus
**Theseus** is a support tool for gaming servers.

## Requirements
  - .NET

## Dependencies
- Core and most of plugins
  - NLog
  - Json.Net
- JabberAdapter
  - agsXMPP


## Windows
Open Visual Studio and Build it

## Mono
Open MonoDevelope/Xamarin and Build it :)

### Release structure
 - **Modules** - directory for comand processors
 - **Adapters** - directory for communication adapters
 - **accounts.json** - simple text DB file for accounts subsystem. Can be replaced by better IAccounts implementation
 - **configuration.json** - adapters and modules configuration

#### accounts.json
It is json array, every item is one acount record.
~~~{.json}
[
   {
      "Name":"Yan",
      "ID":"admin",
      "Password":"admin",
      "Role":"Admin"
   }
]
~~~
- **Name** - Visual name
- **ID** - Login ID
- **Password** - Account password
- **Role** -  Account permissions. One of Owner, Admin, Moderator, Normal, Ignore. Can be extended with **Api.Role** enum.

#### configuration.json
It is json object of two parts: **adapters** and **modules**. Both of it have same structure, but they are used to configurate different plugin types: **adapters** for **Api.Adapter** objects, **modules** for **Api.Module** objects
~~~{.json}
{
  "adapters": [
    {
      "class": "TerminalAdapter"
    },
    {
      "class": "JabberAdapter",
      "config": {
      	"nickname": "Theseus",
        "username": "theseus",
        "password": "************",
        "server": "xmpp.ru",
        "conference": "gamecoma@conference.jabber.ru"
      }
    }
  ],
  "modules": [
    {
      "class": "TheseusControl"
    },
    {
      "class": "AuthModule"
    },
    {
      "class": "Minecraft",
      "config": {
        "servers": {
          "classic": {
            "host": "play.gamecoma.ru",
            "port": 25565
          }
        }
      }
    }
  ]
}
~~~

##### Plugin structure
 - **class** - .NET class in assembly. It should be inherited from **Api.Adapter**/**Api.Module**
 - **config** - dictionary, which will be loaded during plugin initialization process. (Config will be visible in plugin constructor)

##### TerminalAdapter
It handles console IO to send command to **Theseus** and print results to console. No configuration required.

##### JabberAdapter
Every instance in config will be initialized as new adapter instance at runtime. It will connect as bot to configured room and will listen for private and channel messages. Defaul role is **Api.Role.Normal**. Jabber roles(**Moderator**) are handled, so, you don't need to add new accounts to **accounts.json**.
 - **nickname** - Adapter's name.
 - **username** - Jabber ID without '@'.
 - **password** - Jabber password.
 - **server** - Server host (jabber.org, jabber.ru, xmpp.ru, ...).
 - **conference** - Full room ID (gamecoma@conference.jabber.ru, ...).

##### TheseusControl
Configuration is not required.
Commands:
 - **/shutdown** - Stop **Theseus**.
 - **/help** - Print available commands which are allowed for your current role.

Please, use **/help** command to discover all other commands.

##### AuthModule
Configuration is not required. It uses accounts subsystem to authorizate users. 

Next versions: move accounts subsystem from Core to this plugin and create enviromental for plugin sharing(where one plugin can use another)

##### Minecraft
Configuration contains list **server** of observable servers.

Every item in list is object. Key is short name of server. Value :
 - **host** - Minecraft server port.
 - **port** - Integer port.

~~~{.json}
"servers": {
    "classic": {
      "host": "play.gamecoma.ru",
      "port": 25565
    }
  }
~~~
## For developers

## Initialization
To run **Theseus** you should:
 - Create instance of **Theseus.Core** with names of configurations and accounts file
 - Call **Core.Start()**
 - ...
 - Profit
 - To block current thread and wait for core end, call **Core.Wait()**

~~~{.cs}
public static void Main(string[] args) {
  var core = new Core("configuration.json", "accounts.json");
  core.Start();
  core.Wait();
}
~~~

Theseus widely use NLog framework. Example configuration:
~~~{.cs}
// Step 1. Create configuration object 
var config = new LoggingConfiguration();

// Step 2. Create targets and add them to the configuration 
var consoleTarget = new ConsoleTarget();
config.AddTarget("console", consoleTarget);

var fileTarget = new FileTarget();
config.AddTarget("file", fileTarget);

// Step 3. Set target properties 
consoleTarget.Layout = @"[${date:format=HH\:mm\:ss.fff}][${logger}.${level}]: ${message} ${exception:format=Message,StackTrace}";

fileTarget.FileName = "${basedir}/logs/${date:format=yyyy.MM.dd}.txt"; // FileName based on current date
fileTarget.Layout = @"[${date:format=dd.MM.yyyy HH\:mm\:ss.fff}][${logger}.${level}]: ${message} ${exception:format=Message,StackTrace}";
fileTarget.DeleteOldFileOnStartup = false; // Append file
fileTarget.AutoFlush = true;

// Step 4. Define rules
var rule1 = new LoggingRule("*", LogLevel.Info, consoleTarget); //Print only info messages and higher to console
config.LoggingRules.Add(rule1);

var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget); //Print debug messages and higher to file
config.LoggingRules.Add(rule2);

// Step 5. Activate the configuration
LogManager.Configuration = config;
~~~

## Main loop
**Theseus** is fully asynchronous platform and it uses *ThreadPool* with async/await *Task* methods for everything.

Main class is **Theseus.Core**. It uses plugin managers to initialize, load and run plugins at runtime. 
### Workflow
 - At start, core load plugins and run them in asynchronous tasks.
 - **Api.Adapter** should connect to his destination and handle IO operation. 
 - When new command appears, **Api.Adapter** sends it to **Api.IAdapterManager**  for processing. 
 - **Api.IAdapterManager** uses **Api.IModuleManager** to process command. 
 - Then, command processor(**Api.Module**) returns result to **Api.IModuleManager**, 
 - which sends it back to **Api.IAdapterManager** 
 - and finally to initial **Api.Adapter**.
 - 
Modules can run long term operations, for example, to handle alive connection to minecraft server. But, usually it is request-response processing.

Plugins should observe *CancellationToken* to abort their work.

## How to implement new adapter (communication adapter)
 - Create public class inherited from **Api.Adapter**
 - Implement public constuctor
~~~{.cs}
public class JabberAdapter: Api.Adapter {
  private Logger Logger { get; set; }
  public JabberAdapter(Dictionary<String, Object> config, IAdapterManager manager)
        : base("Jabber", config, manager) {
        Logger = Manager.GetLogger(this);
    }
}
~~~
 - Override **Start** method and save *CancellationToken* for future purposes. You should call **base.Start** method implementation.
~~~{.cs}
public override void Start(CancellationToken token){
    base.Start(token);
    token.Register(Finish);
}
~~~
   - You can block this thread for your operations(see **TerminalAdapter**) and register *Thread.CurrentThread.Abort* on token cancel
~~~{.cs}
public override void Start(CancellationToken token){
    try {
        base.Start(token);
        using (var abort = token.Register(Thread.CurrentThread.Abort)) {
            while (true) {
                var command = Console.ReadLine();
                var message = new Request(Sender, command);
                Manager.Process(this, message);
            }
        }
    }
    catch (ThreadAbortException) {
        //It is OK.
    }
    finally {
        Finish();
    }
}
~~~
 - When you have received new comand, you should send it to **Api.IAdapterManager** for processing. You should get account to identify command sender: you can use **ICore.GetAccountsDB()** or create your own new **Api.Account**.
~~~{.cs}
var sender = new Sender(new Account("Developer", Role.Owner));
var command = Console.ReadLine();
var message = new Request(sender, command);
Manager.Process(this, message);
~~~
 - When command is processed and response is not empty, **Api.AdapterManager** will call **Api.Adapter.Process** method to send results to initial sender;
~~~{.cs}
public override void Process(Request request, Response response){
    base.Process(request, response);
    Console.WriteLine(response.Info);
}
~~~
 - When *CancellationToken* is canceled, you should finish your operations as fast as possible and call **Api.Adapter.Finish** method. If it will not be called, **Theseus.Core** will wait some time and then, it abort plugin's thread(is not safe).

## How to implement new module (command processor)

# Theseus
**Theseus** is a support tool for gaming servers.

## Requirements
  - .NET 4.5

## Dependencies
- Core and most of plugins
  - NLog
  - Json.Net
- JabberAdapter
  - agsXMPP


## Windows
Open Visual Studio and Build All, than run Runner project

## Mono
Open MonoDevelop/Xamarin and Build All, than run Runner project

## Run
Runner application is included within Theseus project. 
 - Run Runner.exe from binary dictionary
 - Terminal will be opened. You will see plugin initialization process
 - Then, default TerminalAdapter will be runned and it begins to handle console input
 - Type **/?** command into terminal to print all available options.

### Release structure
 - **Handlers** - directory for comand handlers
 - **Adapters** - directory for communication adapters
 - **accounts.json** - simple text DB file for accounts subsystem. It would be replaced by better IAccounts implementation
 - **configuration.json** - adapter's and handler's configuration

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
- **Role** -  Account permissions. One of Owner, Admin, Moderator, Normal, Ignore. It could be extended with **Api.Role** enum.

#### configuration.json
It is json object of two parts: **adapters** and **handlers**. Both of it have same structure, but they are used to configurate different plugin types: **adapters** for **Api.Adapter** objects, **handlers** for **Api.Handler** objects
~~~{.json}
{
  "adapters": [
    {
      "class": "TerminalAdapter"
    },
    {
      "class": "JabberAdapter",
      "locale": "ru"
      "config": {
      	"nickname": "Theseus",
        "username": "theseus",
        "password": "************",
        "server": "xmpp.ru",
        "conference": "gamecoma@conference.jabber.ru"
      }
    }
  ],
  "handlers": [
    {
      "class": "TheseusControl"
    },
    {
      "class": "Auth",
      "locale": "cs"
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
 - **class** - .NET class in assembly. It should be inherited from **Api.Adapter**/**Api.Handler**
 - **locale** - .NET culture identifier. It will be used as *Thread.CurrentThread.CurrentCulture*, when plugin's methods would be called. Handler's locale override caller(adapter) locale.
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

##### Auth
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
 - **Api.IAdapterManager** uses **Api.IHandlerManager** to process command. 
 - Then, command handler(**Api.Handler**) returns result to **Api.IHandlerManager**, 
 - which sends it back to **Api.IAdapterManager** 
 - and finally to initial **Api.Adapter**.
 - 
Handlers can run long term operations, for example, to handle alive connection to minecraft server. But, usually it is request-response processing.

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
 - Override **Api.Plugin.Start(CancellationToken token)** method and save *CancellationToken* for future purposes. You should call **base.Start(token)** method.
~~~{.cs}
public override void Start(CancellationToken token){
    base.Start(token);
    token.Register(Finish);
}
~~~
   - You could block this thread for your operations(see **TerminalAdapter**) and register *Thread.CurrentThread.Abort* on *CancellationToken* cancel
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
 - When you have received new comand, you should send it to **Api.IAdapterManager** for processing. You should get account to identify command sender: you may use **ICore.GetAccountsDB()** or create your own new **Api.Account** instance.
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
 - When *CancellationToken* is canceled, you should finish your operations as fast as possible and call **Api.Plugin.Finish()** method. If it will not be called, **Theseus.Core** will wait some time and aborts plugin's thread(is not safe).

## How to implement new command handler
 Note: When command handler is invoked, required *CultureInfo* is set in *Thread.CurrentThread*. Custom *SynchronizationContext* is used to hold same CultureInfo through async/await calls. Command are looked up with diacritic and case ignore options, so, if your command is **přihlásitse**, you could type **/prihlAsItse** and it will work correctly.

 - Create public class inherited from **Api.Handler**
 - Implement public constuctor
~~~{.cs}
public class TheseusControl : Handler {
    public TheseusControl(Dictionary<String, Object> config, IHandlerManager manager)
        : base("Theseus Control", config, manager) {
    }
}
~~~
 - Override **Api.Plugin.Start(CancellationToken token)** method and save *CancellationToken* for future purposes. You should call **base.Start(token)** method. If your commands has not long-term operations, you could register **Api.Plugin.Finish()** method *CancellationToken* cancel.
~~~{.cs}
public override void Start(CancellationToken token){
    base.Start(token);
    token.Register(Finish);
}
~~~
   - You could block this thread for your operations(see **TerminalAdapter**) and register *Thread.CurrentThread.Abort* on token cancel
~~~{.cs}
public override void Start(CancellationToken token){
    try {
        base.Start(token);
        // Setup remote connections
        using (var abort = token.Register(Thread.CurrentThread.Abort)) {
            //Do some blocking work
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
 - To add new command, you should impelement method with **public Task<Response> Shutdown(Sender sender, String[] args)** signature(it could by *async*). It should contains **Api.Command** and **Api.Roles** attributes.
~~~{.cs}
[Command("shutdown", "", "Stop Theseus core")]
[Roles(Role.Owner)]
public Task<Response> Shutdown(Sender sender, String[] args){
    Manager.GetCore().Stop();
    return Task.FromResult<Response>(null);
}
~~~

~~~{.cs}
[Command("login", "<username> <password>", "Login as another user")]
[Roles(Role.Ignore)]
public async Task<Response> Login(Sender sender, String[] args){
    if (args.Length != 2) {
        var response = new Response(Channel.Private);
        response.SetError("Incorrect options, /login :username :password");
        return response;
    }

    var username = args[0];
    var password = args[1];
    Account account = await Manager.GetCore().GetAccountsDB().GetAccount(username, password);
    if (account != null) {
        if (account.Role <= sender.Role) {
            var response = new Response(Channel.Private);
            response.SetMessage("Your current role is better than authorized!");
            return response;
        }
        else {
            sender.Account = account;
            var response = new Response(Channel.Private);
            response.SetMessage("You are logger as {0} now.", account.Role);
            return response;
        }
    }
    else {
        var response = new Response(Channel.Private);
        response.SetError("Incorrect username or/and password!");
        return response;
    }
}
~~~
 - You can localize command with yours resources file. See **TheseusControl** project, there are exists TheseusControlStrings.resx, TheseusControlStrings.Designer.cs, TheseusControlStrings.ru.resx. When you define command attribute, you could set ResourseType with yours resources class type and use localization keys for **name**, **usage** and **note** params.
 ~~~{.cs}
 [Command("Shutdown_Command", "Shutdown_Usage", "Shutdown_Command", ResourceType = typeof(TheseusControlStrings))]
 [Roles(Role.Owner)]
 public Task<Response> Shutdown(Sender sender, String[] args){
    Manager.GetCore().Stop();
    return Task.FromResult<Response>(null);
 }
 ~~~

### TODO
 - Move accounts subsystem to own handler implementation.
 - Allow plugin sharing - Each plugin can use another plugin's api.
 - Fix conflicts on same commands from different Handler classes. (I think, we should add specify how to identify handler's command, maybe some suffixes/prefixes/groups)

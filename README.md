# TeleAPI
My poor realisation of mvc telegram bot making.

Some old projects here.

I was impressed with MVC pattern when I heard of it and tried on ASP.NET for the first time, and I thougt, damn, why bots aren't doing the same things?
And so, using my great(terrible) knowledge of reflection
It has some cool features, like waiting for user input in controller
```csharp
[OnCommand("/start")]
async Task StartCommand(RequestArgs<CustomUser> args)
{
  this.SendTextMessageAsync(args.SessionUser, "Hi, what's your name?");
  var name = this.ReceiveStringAsync(args.SessionUser);

  this.SendTextMessageAsync(args.SessionUser, $"Hi, {name}!");
}
```

Beside of this it's simple MVC pattern bot library, nothing more. Has controllers, route and etc.

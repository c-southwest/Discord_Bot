using DSharpPlus;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;


var config = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly, true)
    .Build();

// Bot
var client = new DiscordClient(new DiscordConfiguration
{
    Token = Environment.GetEnvironmentVariable("BCI_BOT_TOKEN"),
    TokenType = TokenType.Bot,

});
var channel = await client.GetChannelAsync(895606647290990706);
await client.SendMessageAsync(channel, "Hello, I'm alive now!");
client.MessageCreated += async (client, args) =>
{
    if (args.Message.Content.Contains("get out"))
    {
        await client.SendMessageAsync(args.Channel, "Warning! Be nice, please!");
    }
};
await client.ConnectAsync();

// Web api
var builder = WebApplication.CreateBuilder(args);
var certName = Environment.GetEnvironmentVariable("CERT_NAME");
var cerKey = Environment.GetEnvironmentVariable("CERT_KEY");
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
        options.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certName, cerKey));
});
//var app = WebApplication.Create();
var app = builder.Build();
app.MapGet("/", () => "Hello, post /send with key to send message to your discord bot");
app.MapGet("/send", async () => await client.SendMessageAsync(channel, "You just access the /send endpoint! It works!"));
app.MapPost("/send", async (Message msg) =>
{
    if (msg.key != Environment.GetEnvironmentVariable("BCI_KEY")) return Results.Problem("Wrong key");
    await client.SendMessageAsync(channel, msg.text);
    return Results.Ok("Got it!");
});
app.UseHttpsRedirection();
app.Urls.Add("https://0.0.0.0:443");

app.Run();
record Message(string text, string key);


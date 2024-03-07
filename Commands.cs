using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSPlus.Examples
{
    public class ExampleUngrouppedCommands : BaseCommandModule
    {
        [Command("ping")] // defines this method as a command
        [Description("Example ping command")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("pong")] // alternative names for the command
        public async Task Ping(CommandContext ctx) // this command takes no arguments
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // adds emoji
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

            // respond with ping
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
        }

        [Command("greet"), Description("Says hi to specified user."), Aliases("sayhi", "say_hi")]
        public async Task Greet(CommandContext ctx, [Description("The user to say hi to.")] DiscordMember member) // this command takes a member as an argument; you can pass one by username, nickname, id, or mention
        {
            // triggers a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // adds emoji
            var emoji = DiscordEmoji.FromName(ctx.Client, ":wave:");

            // responds and greets the user.
            await ctx.RespondAsync($"{emoji} Hello, {member.Mention}!");
        }

        [Command("sum"), Description("Sums all given numbers and returns said sum.")]
        public async Task SumOfNumbers(CommandContext ctx, [Description("Integers to sum.")] params int[] args)
        {
            // bot is typing 
            await ctx.TriggerTypingAsync();

            // calculate the sum
            var sum = args.Sum();

            // send
            await ctx.RespondAsync($"The sum of these numbers is {sum.ToString("#,##0")}");
        }

        // this command will uses a custom type which has
        // registered a converter during initialization
        [Command("math"), Description("Does basic math.")]
        public async Task Math(CommandContext ctx, [Description("First operand.")] double num1, MathOperation operation, [Description("Second operand.")] double num2)
        {
            var result = 0.0;
            switch (operation)
            {
                case MathOperation.Add:
                    result = num1 + num2;
                    break;

                case MathOperation.Subtract:
                    result = num1 - num2;
                    break;
                    
                case MathOperation.Multiply:
                    result = num1 * num2;
                    break;

                case MathOperation.Divide:
                    result = num1 / num2;
                    break;

                case MathOperation.Modulo:
                    result = num1 % num2;
                    break;
            }

            var emoji = DiscordEmoji.FromName(ctx.Client, ":1234:");
            await ctx.RespondAsync($"{emoji} The result is {result.ToString("#,##0.00")}");
        }
    }

    [Group("admin")] // marks this class as a command group
    [Description("Administrative commands.")] // gives it a description for help
 
    [RequirePermissions(Permissions.ManageGuild)] // restricts this to users who have appropriate permissions
    public class ExampleGrouppedCommands : BaseCommandModule
    {
        // all the commands will need to be executed as <prefix>admin <command> <arguments>

        // this command will be only executable by the bot's owner
        [Command("sudo"), Description("Executes a command as another user."), Hidden, RequireOwner]
        public async Task Sudo(CommandContext ctx, [Description("Member to execute as.")] DiscordMember member, [RemainingText, Description("Command text to execute.")] string command)
        {

            await ctx.TriggerTypingAsync();

            // get the command service, 
            var cmds = ctx.CommandsNext;

            // retrieve the command and its arguments from the given string
            var cmd = cmds.FindCommand(command, out var customArgs);

            // create a fake CommandContext
            var fakeContext = cmds.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, customArgs);

            // perform the sudo
            await cmds.ExecuteCommandAsync(fakeContext);
        }

        [Command("nick"), Description("Gives someone a new nickname."), RequirePermissions(Permissions.ManageNicknames)]
        public async Task ChangeNickname(CommandContext ctx, [Description("Member to change the nickname for.")] DiscordMember member, [RemainingText, Description("The nickname to give to that user.")] string new_nickname)
        {
            // triggers a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            try
            {
                // changes the nickname, and tell the 
                // audit logs who did it.
                await member.ModifyAsync(x =>
                {
                    x.Nickname = new_nickname;
                    x.AuditLogReason = $"Changed by {ctx.User.Username} ({ctx.User.Id}).";
                });
                
                // make sa simple response.
                var emoji = DiscordEmoji.FromName(ctx.Client, ":+1:");

                // and responds with it.
                await ctx.RespondAsync(emoji);
            }
            catch (Exception)
            {
                // oh no, something failed, let the invoker now
                var emoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
                await ctx.RespondAsync(emoji);
            }
        }
    }

    [Group("memes")]
    [Description("Contains some memes. When invoked without subcommand, returns a random one.")]
    [Aliases("copypasta")]
    public class ExampleExecutableGroup : BaseCommandModule
    {
        // commands in this group need to be executed as 
        // <prefix>memes [command] or <prefix>copypasta [command]

        // this command is a group command. It means it can be invoked by just typing <prefix>memes
        [GroupCommand]
        public async Task RandomMemeAsync(CommandContext ctx)
        {
            // let's give them a random meme
            var rnd = new Random();
            var nxt = rnd.Next(0, 2);

            switch (nxt)
            {
                case 0:
                    await Pepe(ctx);
                    return;

                case 1:
                    await NotFine(ctx);
                    return;

            }
        }

        [Command("pepe"), Aliases("feelsbadman"), Description("Feels bad, man.")]
        public async Task Pepe(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            // wrap it into an embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "Pepe",
                ImageUrl = "http://i.imgur.com/44SoSqS.jpg"
            };
            await ctx.RespondAsync(embed);
        }

        [Command("!fine"), Aliases("notfine"), Description("Perfectly fine")]
        public async Task NotFine(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "NotFine",
                ImageUrl = "https://i.imgur.com/c4jt321.png"
            };
            await ctx.RespondAsync(embed);
        }

        

        // subgroup
        [Group("mememan")]
        public class MemeMan : BaseCommandModule
        {
            [GroupCommand]
            public async Task MemeManAsync(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Meme man",
                    ImageUrl = "http://i.imgur.com/tEmKtNt.png"
                };
                await ctx.RespondAsync(embed);
            }

        
            
            [Command("art"), Description("Art.")]
            public async Task Art(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Art",
                    ImageUrl = "http://i.imgur.com/VkmmmQd.png"
                };
                await ctx.RespondAsync(embed);
            }

            [Command("seeameme"), Description("When you see a meme.")]
            public async Task SeeMeme(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "When you see a meme",
                    ImageUrl = "http://i.imgur.com/8GD0hbZ.jpg"
                };
                await ctx.RespondAsync(embed);
            }

           

            [Command("deepdream"), Description("Deepdream'd meme man.")]
            public async Task DeepDream(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Deep dream",
                    ImageUrl = "http://i.imgur.com/U666J6x.png"
                };
                await ctx.RespondAsync(embed);
            }

            
        }
    }
}
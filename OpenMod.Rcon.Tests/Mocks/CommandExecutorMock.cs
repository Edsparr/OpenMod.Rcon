using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Rcon.Api.Actors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenMod.Rcon.Tests.Mocks
{
    public class CommandExecutorMock : ICommandExecutor
    {
        public const string DemoResponse = "Executed command!";
        public ICollection<(ICommandActor, string[], string)> CommandsExecuted { get; } = new List<(ICommandActor, string[], string)>();

        public async Task<ICommandContext> ExecuteAsync(ICommandActor actor, string[] args, string prefix)
        {
            CommandsExecuted.Add((actor, args, prefix));

            await actor.PrintMessageAsync(DemoResponse);


            return default(CommandContext);
        }
    }
}

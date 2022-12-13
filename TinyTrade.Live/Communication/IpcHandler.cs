using HandierCli.CLI;
using System.IO.Pipes;

namespace TinyTrade.Live.Communication;

/// <summary>
/// Inter process communication handler
/// </summary>
internal class IpcHandler
{
    private readonly IEnumerable<Command> pipeCommands;

    private bool closed = true;

    public IpcHandler(IEnumerable<Command> commands)
    {
        pipeCommands = commands;
    }

    public void Close() => closed = true;

    public void Open()
    {
        if (!closed) return;

        Console.WriteLine($"Opened pipe {Environment.ProcessId}.pipe");
        _ = Task.Run(() => PipeBackgroundTask());
        closed = false;
    }

    private async Task PipeBackgroundTask()
    {
        while (!closed)
        {
            var serverPipe = new NamedPipeServerStream($"{Environment.ProcessId}.pipe", PipeDirection.InOut);
            var pipeReader = new StreamReader(serverPipe);
            try
            {
                await serverPipe.WaitForConnectionAsync();
                while (serverPipe.IsConnected)
                {
                    var line = await pipeReader.ReadLineAsync();
                    await HandlePipeCommand(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error handling pipe: {ex}");
            }
            finally
            {
                pipeReader.Close();
                serverPipe.Close();
            }
        }
    }

    private async Task HandlePipeCommand(string? command)
    {
        if (string.IsNullOrEmpty(command)) return;
        Console.WriteLine($"Pipe command {command} received");
        var args = command.Trim().Split(" ");
        if (string.IsNullOrEmpty(args.First())) return;
        var cmd = pipeCommands.FirstOrDefault(c => c.Key == args.First());
        if (cmd == null) return;
        await cmd.Execute(args.Skip(1));
    }
}
// using UnityEngine.UI;
// using static Viva.console.DevConsole;
//
// namespace Viva.console
// {
//     public class CommandClearConsole : ConsoleCommand
//     {
//         public sealed override string Name { get; protected set; }
//         public sealed override string Command { get; protected set; }
//         public sealed override string Description { get; protected set; }
//         public sealed override string Help { get; protected set; }
//         public sealed override string Example { get; protected set; }
//
//         public CommandClearConsole()
//         {
//             Name = "Clear console terminal";
//             Command = "clear";
//             Description = "Removes all commands data from the window.";
//             Help = "Syntax: clear";
//             Example = "clear";
//
//             AddCommandToConsole();
//         }
//
//         public override void RunCommand(string[] data)
//         {
//             if (data.Length == 1)
//             {
//                 var type = data[0].ToLower();
//
//                 if (type == "clear")
//                 {
//                     DevConsole.Instance._consoleText.text = "Cleared";
//                 }
//                 else
//                 {
//                     AddStaticMessageToConsole(TypeNotSupported);
//                 }
//             }
//             else
//             {
//                 AddStaticMessageToConsole(ParametersAmount);
//             }
//
//         }
//
//         public static CommandClearConsole CreateCommand()
//         {
//             return new CommandClearConsole();
//         }
//     }
// }
// using System;
// using System.Linq;
// using static Viva.console.DevConsole;
//
// namespace Viva.console
// {
//     public class CommandSetPlayerSpeed : ConsoleCommand
//     {
//         public sealed override string Name { get; protected set; }
//         public sealed override string Command { get; protected set; }
//         public sealed override string Description { get; protected set; }
//         public sealed override string Help { get; protected set; }
//         public sealed override string Example { get; protected set; }
//
//         public CommandSetPlayerSpeed()
//         {
//             Name = "Player Walkspeed";
//             Command = "walkspeed";
//             Description = "Sets the Player WalkSpeed, Default WalkSpeed is 0.24";
//             Help = "Syntax: walkspeed <walkspeed> \n" +
//                    $"<color={RequiredColor}><walkspeed></color> is required!";
//             Example = "walkspeed 0.5, walkspeed reset";
//
//             AddCommandToConsole();
//         }
//
//         public override void RunCommand(string[] data)
//         {
//             if (data.Length == 2)
//             {
//                 var commandParameter = data[1];
//                 if (string.IsNullOrWhiteSpace(commandParameter))
//                 {
//                     AddStaticMessageToConsole(ParametersAmount);
//                 }
//                 // when commandParameter is digit, speed was passed
//                 if (float.TryParse(commandParameter, out float speed))
//                 {
//                     speed = Convert.ToSingle(commandParameter);
//                     GameDirector.player.walkSpeed = speed;
//                     AddStaticMessageToConsole("Set Walkspeed to" + " " + speed);
//                 }
//                 else
//                 {
//                     if (commandParameter.Contains("reset"))
//                     {
//                         GameDirector.player.walkSpeed = 0.24f;
//                         AddStaticMessageToConsole("Reset Walkspeed");
//                     }
//                 }
//             }
//             else
//             {
//
//                 AddStaticMessageToConsole(ParametersAmount);
//             }
//         }
//
//         public static CommandSetPlayerSpeed CreateCommand()
//         {
//             return new CommandSetPlayerSpeed();
//         }
//     }
// }
//

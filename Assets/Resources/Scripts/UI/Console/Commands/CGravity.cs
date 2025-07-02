// using static Viva.console.DevConsole;
// using UnityEngine;
// using System.IO;
// using System.Globalization;
//
// namespace Viva.console
// {
//     public class CGravity : ConsoleCommand
//     {
//         public sealed override string Name { get; protected set; }
//         public sealed override string Command { get; protected set; }
//         public sealed override string Description { get; protected set; }
//         public sealed override string Help { get; protected set; }
//         public sealed override string Example { get; protected set; }
//
//         public CGravity()
//         {
//             Name = "Set Gravity";
//             Command = "Gravity";
//             Description = "Sets the world's gravity";
//             Help = "Syntax: Gravity <value> \n" +
//                    $"<color={RequiredColor}><value></color> is required!";
//             Example = "Gravity 0, Gravity 1";
//
//             AddCommandToConsole();
//         }
//
//         public override void RunCommand(string[] data)
//         {
//             if (data.Length == 2)
//             {
//                 var commandParameter = data[1];
//                 
//                 if (string.IsNullOrWhiteSpace(commandParameter))
//                 {
//                     AddStaticMessageToConsole(ParametersAmount);
//                 }
//
//                 var gravity = float.Parse(commandParameter) * -9.81f;
//                 
//                 Physics.gravity = new Vector3(0, gravity, 0);
//             }
//             else
//             {
//                 AddStaticMessageToConsole(ParametersAmount);
//             }
//         }
//
//         public static CommandSpawn CreateCommand()
//         {
//             return new CommandSpawn();
//         }
//     }
// }
//

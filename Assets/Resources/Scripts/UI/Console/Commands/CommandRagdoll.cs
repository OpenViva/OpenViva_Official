// using System;
// using static Viva.console.DevConsole;
//
// namespace Viva.console
// {
//     public class CommandRagdoll : ConsoleCommand
//     {
//         public sealed override string Name { get; protected set; }
//         public sealed override string Command { get; protected set; }
//         public sealed override string Description { get; protected set; }
//         public sealed override string Help { get; protected set; }
//         public sealed override string Example { get; protected set; }
//
//         public CommandRagdoll()
//         {
//             Name = "Ragdoll";
//             Command = "ragdoll";
//             Description = "Ragdoll's Selected Companions";
//             Help = "Syntax: ragdoll <muscle weights> \n" +
//                    $"<color={RequiredColor}><muscle weights></color> are required!";
//             Example = "ragdoll 0.5";
//
//             AddCommandToConsole();
//         }
//
//         public override void RunCommand(string[] args)
//         {
//
//             if (args.Length == 2)
//             {
//                 var commandParameter = args[1];
//                 float weight = Convert.ToSingle(commandParameter);
//                 if (GameDirector.player.objectFingerPointer.selectedCompanions.Count > 0)
//                 {
//                     foreach (var companion in GameDirector.player.objectFingerPointer.selectedCompanions)
//                     {
//                         companion.BeginRagdollMode(weight, Companion.Animation.FALLING_LOOP);
//                     }
//                 }
//                 else
//                 {
//                     AddStaticMessageToConsole("No Companion's Selected");
//                 }
//             }
//             else
//             {
//                 AddStaticMessageToConsole(ParametersAmount);
//             }
//         }
//
//         public static CommandRagdoll CreateCommand()
//         {
//             return new CommandRagdoll();
//         }
//     }
// }
//

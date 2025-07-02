// using UnityEngine.TextCore.Text;
// using static Viva.console.DevConsole;
//
// namespace Viva.console
// {
//     public class CommandSelectCharacter : ConsoleCommand
//     {
//         public sealed override string Name { get; protected set; }
//         public sealed override string Command { get; protected set; }
//         public sealed override string Description { get; protected set; }
//         public sealed override string Help { get; protected set; }
//         public sealed override string Example { get; protected set; }
//
//         public CommandSelectCharacter()
//         {
//             Name = "Select Character";
//             Command = "Selected";
//             Description = "Selects/Deselects Companion's";
//             Help = "Syntax: Selected <all/none> \n" +
//                    $"<color={RequiredColor}><all/none></color> is required!";
//             Example = "Selected all, Selected none";
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
//                 if (commandParameter.Contains("all"))
//                 {
//                     foreach (var character in GameDirector.characters.objects)
//                     {
//                         if (character is Companion companion && !(character is Player))
//                         {
//                             if (!GameDirector.player.objectFingerPointer.selectedCompanions.Contains(companion))
//                             {
//                                 companion.OnSelected();
//                                 GameDirector.player.objectFingerPointer.selectedCompanions.Add(companion);
//                             }
//                         }
//                     }
//                     AddStaticMessageToConsole("Selected All Companions");
//                 }
//                 else if (commandParameter.Contains("none"))
//                 {
//                     foreach (var character in GameDirector.characters.objects)
//                     {
//                         if (character is Companion companion && !(character is Player))
//                         {
//                             companion.OnUnselected();
//                             GameDirector.player.objectFingerPointer.selectedCompanions.Remove(companion);
//                         }
//
//                     }
//                     AddStaticMessageToConsole("Deselected All Companions");
//                 }
//                 else
//                 {
//                     AddStaticMessageToConsole(ArgumentNotRecognized);
//                 }
//             }
//             else
//             {
//                 AddStaticMessageToConsole(ParametersAmount);
//             }
//         }
//
//         public static CommandSelectCharacter CreateCommand()
//         {
//             return new CommandSelectCharacter();
//         }
//     }
// }
//

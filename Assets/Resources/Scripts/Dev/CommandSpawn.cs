// using static Viva.console.DevConsole;
// using UnityEngine;
// using System.IO;
// using System.Globalization;
//
// namespace Viva.console
// {
//     public class CommandSpawn : ConsoleCommand
//     {
//         public sealed override string Name { get; protected set; }
//         public sealed override string Command { get; protected set; }
//         public sealed override string Description { get; protected set; }
//         public sealed override string Help { get; protected set; }
//         public sealed override string Example { get; protected set; }
//
//         public CommandSpawn()
//         {
//             Name = "Spawn Prefab";
//             Command = "spawn";
//             Description = "Creates Any prefab you specify";
//             Help = "Syntax: spawn <prefab name> \n" +
//                    $"<color={RequiredColor}><prefab></color> is required!";
//             Example = "spawn hat, spawn firework";
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
//                 Vector3 spawnPos = GameDirector.player.head.transform.position + GameDirector.player.head.transform.forward * 2;
//                 
//                 foreach (GameObject prefabToSpawn in GameDirector.instance.spawnablePrefabs)
//                 {
//                     if (commandParameter.ToLower() == prefabToSpawn.name.ToLower())
//                     {
//                         GameObject prefab;
//
//                         prefab = Object.Instantiate(prefabToSpawn, spawnPos, GameDirector.player.head.transform.rotation);
//                         AddStaticMessageToConsole("Successfully spawned " + prefab.gameObject.name);
//
//                     }
//                     else
//                     {
//                         //Too Laggy plus doesent work cause im a dumbass - SolidStone
//                         //AddStaticMessageToConsole("Prefab not found.\n Showing List of available prefabs");
//                         //foreach(GameObject prefabList in GameDirector.instance.spawnablePrefabs)
//                         //{
//                         //    AddStaticMessageToConsole(prefabList.gameObject.name + "\n");
//                         //}
//
//                     }
//                 }
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

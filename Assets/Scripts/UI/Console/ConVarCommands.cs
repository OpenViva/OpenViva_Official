using System;
using System.Linq;
using UnityEngine;

namespace Viva.console
{
    public class ConVarCommands
    {
        [ConVar("help", "Provides information about available commands or details of a specific command.", "commandName (optional)")]
        private void HelpCommand(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                //list all commands
                var allCommands = DevConsole.Commands.Keys
                    .Select(cmd => $"{cmd} - {DevConsole.Commands[cmd].Description}")
                    .ToList();

                DevConsole.AddStaticMessageToConsole("Available Commands:\n" + string.Join("\n", allCommands));
            }
            else
            {
                if (DevConsole.Commands.TryGetValue(commandName, out var commandInfo))
                {
                    string commandDetails = $"{commandInfo.Command}:\nDescription: {commandInfo.Description}\n" +
                                            $"Arguments: {string.Join(", ", commandInfo.Arguments)}";
                    DevConsole.AddStaticMessageToConsole(commandDetails);
                }
                else
                {
                    CommandResult.Failure(DevConsole.CommandNotRecognized);
                }
            }
        }
        
        [ConVar("createcompanion", "Creates a companion at a specified location.", "cardName")]
        public static void CreateCharacter(string cardName)
        {
            if (string.IsNullOrWhiteSpace(cardName))
            {
                CommandResult.Failure("Card name is required.");
                return;
            }

            if (GameDirector.player == null || GameDirector.player.head == null)
            {
                CommandResult.Failure("Player or player's head not found, unable to create character.");
                return;
            }

            //get the spawn position
            Vector3 spawnPos = GameDirector.player.head.transform.position + GameDirector.player.head.transform.forward * 2;

            //check if the character card exists
            if (ModelCustomizer.main.characterCardBrowser.CheckIfCardActuallyExists(cardName))
            {
                GameDirector.instance.town.BuildTownLolis(new string[] { cardName }, 1, spawnPos);
                CommandResult.Success($"Successfully created: {cardName}");
            }
            else
            {
                CommandResult.Failure($"Couldn't find: {cardName}. Are you sure it exists?");
            }
        }
        
        [ConVar("walkspeed", "Set Player Walk Speed")]
        public static void SetPlayerSpeed(string wSpeed)
        {
             if (float.TryParse(wSpeed, out float speed))
             {
                 speed = Convert.ToSingle(wSpeed);
                 GameDirector.player.walkSpeed = speed;
                 CommandResult.Success("Set Walkspeed to" + " " + speed);
             }
             else
             {
                 if (wSpeed.Contains("reset"))
                 {
                     GameDirector.player.walkSpeed = 0.24f;
                     CommandResult.Success("Reset Walkspeed");
                 }
             }
        }
        
        [ConVar("testarea", "Teleport to test area")]
        public static void TestArea()
        {
            
            GameDirector.player.transform.position = new Vector3(-559, 143, 1181);
            CommandResult.Success("Teleported to Test Area");
        }
    }
}
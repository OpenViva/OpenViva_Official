using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Viva.console
{
    public class CommandResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        private CommandResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;

            PrintResult();
        }

        public static CommandResult Success(string message) => new CommandResult(true, message);
        public static CommandResult Failure(string message) => new CommandResult(false, message);

        private void PrintResult()
        {
            if (IsSuccess)
            {
                DevConsole.AddStaticMessageToConsole(Message);
            }
            else
            {
                DevConsole.AddStaticMessageToConsole($"<color=#ff0000>Error:</color> {Message}");
            }
        }
    }

public class DevConsole : MonoBehaviour
    {
        public static DevConsole Instance { get; private set; }
        public static Dictionary<string, CommandInfo> Commands { get; private set; } = new Dictionary<string, CommandInfo>();

        [SerializeField] private Canvas _consoleCanvas;
        [SerializeField] private ScrollRect _scrollRect;
        public Text _consoleText;
        public Text _inputText;
        [SerializeField] private Text _autocompleteText;
        public InputField _consoleInput;

        [Tooltip("Define how many commands can be held in the clipboard. If set to 0, clipboard will be off.")]
        public int _clipboardSize;

        private string[] _clipboard;
        private int _clipboardIndexer = 0;
        private int _clipboardCursor = 0;

        [SerializeField]
        [Tooltip("Specify minimum amount of characters for autocomplete key(TAB) to work.")]
        private int _tabMinCharLength = 3;

        private List<string> _suggestions;
        private int _suggestionIndex;

        #region Colors

        public static string RequiredColor = "#FA8072";
        public static string OptionalColor = "#00FF7F";
        public static string WarningColor = "#ffcc00";
        public static string ExecutedColor = "#e600e6";

        #endregion

        #region Typical Console Messages

        public static string CommandNotRecognized = $"Command not <color={WarningColor}>recognized</color>";
        public static string ParametersAmount = $"Wrong <color={WarningColor}>amount of parameters</color>";
        public static string ClipboardCleared = $"\nConsole clipboard <color={OptionalColor}>cleared</color>";

        #endregion

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Commands = new Dictionary<string, CommandInfo>();
            _suggestions = new List<string>();
        }

        private void Start()
        {
            _clipboard = new string[_clipboardSize];
            _consoleCanvas.gameObject.SetActive(false);

            var primary = "#F9F0E6";

            _consoleText.text = "\n\n---------------------------------------------------------------------------------\n" +
                               $"<size=30><color={primary}>OpenViva Developer Console</color></size> \n" +
                               "---------------------------------------------------------------------------------\n\n" +
                               "Type <color=orange>help</color> for list of available commands. \n" +
                               "Type <color=orange>help <command></color> for command details. \n \n \n";

            RegisterAllConVars();
        }

        private void RegisterAllConVars()
        {
            var members = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(assembly => assembly.GetTypes())
                            .SelectMany(type => type.GetMembers(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                            .Where(member => member.IsDefined(typeof(ConVarAttribute), false));

            foreach (var member in members)
            {
                var attribute = member.GetCustomAttribute<ConVarAttribute>();
                string command = attribute.Command;

                Action<string[]> commandAction = args => { };

                // Register fields
                if (member is FieldInfo field)
                {
                    commandAction = args =>
                    {
                        object target = field.IsStatic ? null : Activator.CreateInstance(field.DeclaringType);

                        if (args.Length == 0)
                        {
                            try
                            {
                                AddStaticMessageToConsole($"{command}: {field.GetValue(target)}");
                            }
                            catch (Exception e)
                            {
                                CommandResult.Failure($"Failed to get value of {command}: {e.Message}");
                            }
                        }
                        else
                        {
                            try
                            {
                                object value = Convert.ChangeType(args[0], field.FieldType);
                                field.SetValue(target, value);
                                AddStaticMessageToConsole($"{command} set to: {value}");
                            }
                            catch (Exception e)
                            {
                                CommandResult.Failure($"Failed to set {command}: {e.Message}");
                            }
                        }
                    };
                }
                // Register properties
                else if (member is PropertyInfo property)
                {
                    commandAction = args =>
                    {
                        object target = property.GetMethod.IsStatic ? null : Activator.CreateInstance(property.DeclaringType);

                        if (args.Length == 0)
                        {
                            try
                            {
                                AddStaticMessageToConsole($"{command}: {property.GetValue(target)}");
                            }
                            catch (Exception e)
                            {
                                CommandResult.Failure($"Failed to get value of {command}: {e.Message}");
                            }
                        }
                        else
                        {
                            try
                            {
                                object value = Convert.ChangeType(args[0], property.PropertyType);
                                property.SetValue(target, value);
                                AddStaticMessageToConsole($"{command} set to: {value}");
                            }
                            catch (Exception e)
                            {
                                CommandResult.Failure($"Failed to set {command}: {e.Message}");
                            }
                        }
                    };
                }
                // Register methods
                else if (member is MethodInfo method)
                {
                    commandAction = args =>
                    {
                        var parameters = method.GetParameters();
                        int requiredParamCount = parameters.Count(p => !p.IsOptional);

                        // Ensure provided arguments are at least the required count
                        if (args.Length < requiredParamCount)
                        {
                            CommandResult.Failure($"Command {command} expects at least {requiredParamCount} arguments, but got {args.Length}.");
                            return;
                        }

                        try
                        {
                            object[] invokeArgs;

                            // Check if the method has only one parameter of type string[] (params scenario)
                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
                            {
                                invokeArgs = new object[] { args }; // Pass the entire args array as the single parameter
                            }
                            else
                            {
                                // Prepare arguments including handling optional parameters
                                invokeArgs = new object[parameters.Length];
                                for (int i = 0; i < parameters.Length; i++)
                                {
                                    if (i < args.Length)
                                    {
                                        // Convert provided argument
                                        invokeArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                                    }
                                    else
                                    {
                                        // Handle optional parameter by setting its default value if not provided
                                        invokeArgs[i] = parameters[i].DefaultValue;
                                    }
                                }
                            }

                            // Distinguish between static and instance methods
                            object target = method.IsStatic ? null : Activator.CreateInstance(method.DeclaringType);
                            method.Invoke(target, invokeArgs);
                            CommandResult.Success($"{command} executed successfully.");
                        }
                        catch (Exception e)
                        {
                            CommandResult.Failure($"Failed to execute {command}: {e.Message}");
                        }
                    };
                }

                // Add command to the console commands dictionary
                Commands[command] = new CommandInfo
                {
                    Command = command,
                    Description = attribute.Description,
                    Arguments = attribute.Arguments,
                    Execute = commandAction
                };
            }
        }



        public static void AddCommandsToConsole(string name, CommandInfo command)
        {
            if (!Commands.ContainsKey(name))
            {
                Commands.Add(name, command);
            }
        }

        private void Update()
        {
            // Disable if in VR
            if (GameDirector.player.controls == Player.ControlType.VR)
            {
                _consoleCanvas.gameObject.SetActive(false);
                _consoleInput.DeactivateInputField();
                return;
            }

            // Toggle console visibility with F1
            if (Keyboard.current[Key.F1].wasPressedThisFrame)
            {
                if (!_consoleCanvas.gameObject.activeInHierarchy)
                {
                    GameDirector.instance.SetEnableControls(GameDirector.ControlsAllowed.NONE);
                    _consoleCanvas.gameObject.SetActive(true);
                    _consoleInput.ActivateInputField();
                    _consoleInput.Select();
                }
                else
                {
                    if (!GameDirector.player.pauseMenu.IsPauseMenuOpen)
                    {
                        GameDirector.instance.SetEnableControls(GameDirector.ControlsAllowed.ALL);
                    }
                    _consoleCanvas.gameObject.SetActive(false);
                }
            }

            if (_consoleCanvas.gameObject.activeInHierarchy)
            {
                // Enter to submit command
                if (Keyboard.current[Key.Enter].wasPressedThisFrame)
                {
                    if (!string.IsNullOrEmpty(_inputText.text))
                    {
                        AddMessageToConsole(_inputText.text);
                        ParseInput(_inputText.text);

                        if (_clipboardSize != 0)
                        {
                            StoreCommandInTheClipboard(_inputText.text);
                        }
                    }
                    _consoleInput.text = "";
                    _consoleInput.ActivateInputField();
                    _consoleInput.Select();
                }

                HandleClipboardNavigation();
                HandleAutocomplete();
            }

            if (!_consoleCanvas.gameObject.activeInHierarchy)
            {
                _consoleInput.text = "";
            }
        }
        
        private void ParseInput(string input)
        {
            string[] splitInput = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (splitInput.Length == 0) return;

            string command = splitInput[0].ToLower();
            if (!DevConsole.Commands.TryGetValue(command, out var commandInfo))
            {
                AddMessageToConsole(CommandNotRecognized);
                return;
            }

            //execute the command with the remaining arguments
            string[] arguments = splitInput.Skip(1).ToArray();
            commandInfo.Execute(arguments);
        }

        private void HandleClipboardNavigation()
        {
            if (Keyboard.current[Key.UpArrow].wasPressedThisFrame)
            {
                if (_clipboardSize != 0 && _clipboardIndexer != 0)
                {
                    if (_clipboardCursor == _clipboardIndexer)
                    {
                        _clipboardCursor--;
                        _consoleInput.text = _clipboard[_clipboardCursor];
                    }
                    else if (_clipboardCursor > 0)
                    {
                        _clipboardCursor--;
                        _consoleInput.text = _clipboard[_clipboardCursor];
                    }
                    else
                    {
                        _consoleInput.text = _clipboard[0];
                    }
                    _consoleInput.caretPosition = _consoleInput.text.Length;
                }
            }

            if (Keyboard.current[Key.DownArrow].wasPressedThisFrame)
            {
                if (_clipboardSize != 0 && _clipboardIndexer != 0)
                {
                    if (_clipboardCursor < _clipboardIndexer)
                    {
                        _clipboardCursor++;
                        _consoleInput.text = _clipboard[_clipboardCursor];
                        _consoleInput.caretPosition = _consoleInput.text.Length;
                    }
                }
            }
        }

        private void HandleAutocomplete()
        {
            string currentText = _consoleInput.text;
            string[] splitInput = currentText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            _autocompleteText.text = "";

            if (string.IsNullOrEmpty(currentText))
                return;

            // Handle autocompletion when pressing Tab (for commands only)
            if (Keyboard.current[Key.Tab].wasPressedThisFrame)
            {
                if (splitInput.Length == 1)
                {
                    // Autocomplete for commands
                    string commandPrefix = splitInput[0];
                    var possibleCommands = Commands.Keys
                        .Where(cmd => cmd.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (possibleCommands.Count == 1)
                    {
                        _consoleInput.text = possibleCommands[0] + " ";
                        _consoleInput.caretPosition = _consoleInput.text.Length;
                    }
                    else if (possibleCommands.Count > 1)
                    {
                        _autocompleteText.text = string.Join(", ", possibleCommands);
                    }
                }
            }
            // Handle displaying suggestions without modifying the input
            else
            {
                if (splitInput.Length == 1)
                {
                    // Suggest commands
                    string commandPrefix = splitInput[0];
                    var possibleCommands = Commands.Keys
                        .Where(cmd => cmd.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (possibleCommands.Count > 0)
                    {
                        _autocompleteText.text = string.Join(", ", possibleCommands);
                    }
                }
                else if (splitInput.Length > 1)
                {
                    // Suggest arguments for the command (no Tab completion)
                    string command = splitInput[0];
                    if (Commands.TryGetValue(command, out var commandInfo))
                    {
                        int argIndex = splitInput.Length - 2;
                        if (argIndex >= 0 && argIndex < commandInfo.Arguments.Length)
                        {
                            string currentArg = splitInput[argIndex + 1];
                            string expectedArg = commandInfo.Arguments[argIndex];

                            string commandPart = currentText.Substring(0, currentText.IndexOf(currentArg, StringComparison.OrdinalIgnoreCase));
                            commandPart = commandPart.TrimEnd() + " ";

                            // Display the expected argument suggestion without requiring any input or auto Tab complete
                            _autocompleteText.text = commandPart + expectedArg;
                        }
                    }
                }
            }
        }

        private void StoreCommandInTheClipboard(string command)
        {
            _clipboard[_clipboardIndexer] = command;

            if (_clipboardIndexer < _clipboardSize - 1)
            {
                _clipboardIndexer++;
                _clipboardCursor = _clipboardIndexer;
            }
            else if (_clipboardIndexer == _clipboardSize - 1)
            {
                //clear clipboard & reset
                _clipboardIndexer = 0;
                _clipboardCursor = 0;
                Array.Clear(_clipboard, 0, _clipboardSize);

                AddStaticMessageToConsole(ClipboardCleared);
            }
        }

        private void AddMessageToConsole(string msg)
        {
            _consoleText.text += msg + "\n";
        }

        public static void AddStaticMessageToConsole(string msg)
        {
            Instance._consoleText.text += msg + "\n";
        }
    }


    public class CommandInfo
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string[] Arguments { get; set; }
        public Action<string[]> Execute { get; set; }
    }

}
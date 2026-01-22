using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Commands
{
    public partial class CommandsManager : MonoBehaviour
    {
        private Dictionary<string, (MethodInfo method, CommandAttribute attr)> _commands = new();
        [SerializeField] TMPro.TMP_InputField textField;
        string _input;
        public PauseMenu pauseMenu;
        [SerializeField] TMP_Text outputText;

        private void Awake()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name == "Assembly-CSharp");

            foreach (var assembly in assemblies)
            {
                foreach (MethodInfo methodInfo in assembly.GetTypes().SelectMany(classType => classType.GetMethods()))
                {
                    var attributes = methodInfo.GetCustomAttributes<CommandAttribute>().ToList();
                    if (attributes.Count == 0) continue;

                    foreach (CommandAttribute attribute in attributes)
                    {
                        Debug.Log($"{attribute.CommandName} | {methodInfo.Name}");
                        _commands.Add(attribute.CommandName, (methodInfo, attribute));
                    }

                }
            }

            textField.onSubmit.AddListener(OnSubmit);
        }
        private void OnSubmit(string text)
        {
            _input = text;
            ProcessCommand();
            _input = "";
            textField.text = "";
            pauseMenu.CloseConsole();
        }

        private void ProcessCommand()
        {
            _input = _input.Trim();
            if (string.IsNullOrEmpty(_input)) return;

            string[] tokens = _input.Split(" ");
            string[] parameterTokens = tokens.Skip(1).ToArray();

            if (!_commands.TryGetValue(tokens[0], out var commandEntry))
            {
                Debug.LogError($"Command \"{tokens[0]}\" doesn't exist.");
                return;
            }

            ParameterInfo[] parameters = commandEntry.method.GetParameters();
            if (parameters.Length != parameterTokens.Length)
            {
                Debug.LogError($"Error while handling command \"{tokens[0]}\". Expected {parameters.Length} parameters, got {parameterTokens.Length}");
                return;
            }

            List<object> invocationParams = new List<object>();
            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    invocationParams.Add(Convert.ChangeType(parameterTokens[i], parameters[i].ParameterType));
                }
                catch
                {
                    Debug.LogError($"Failed to convert parameter {parameterTokens[i]} to type {parameters[i].ParameterType}");
                    return;
                }
            }

            
            try
            {
                commandEntry.method.Invoke(this, invocationParams.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while executing command {tokens[0]}: {e.Message}");
            }
        }
    }


    
}



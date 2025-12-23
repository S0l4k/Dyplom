using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Commands
{
    public partial class CommandsManager : MonoBehaviour
    {
        private Dictionary<string, MethodInfo> _commands = new();
        [SerializeField] TMPro.TMP_InputField textField;
        string _input;
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
                        _commands.Add(attribute.CommandName, methodInfo);
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
        }

        private void ProcessCommand()
        {
            Debug.Log("ProcessCommand");
            string[] tokens = _input.Split("");
            string[] parameterTokens = tokens.Skip(1).ToArray();

            if (tokens.Length == 0)
            {
                return;
            }
            if (!_commands.TryGetValue(tokens[0], out var methodInfo))
            {
                Debug.LogError($"Command \"{tokens[0]} \" doesn't exist.");
                return;
            }

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            if (parameterInfos.Length != parameterTokens.Length)
            {
                Debug.LogError($" Error while handling command \"{tokens[0]}\". Excepted {parameterInfos.Length} parameters, got {parameterTokens.Length}");
                return;
            }

            List<object> invocationParams = new List<object>();
            for(int i= 0; i< parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                invocationParams.Add(Convert.ChangeType(parameterTokens[i], parameterInfo.ParameterType));

            }
            methodInfo.Invoke(this, invocationParams.ToArray());
        }
    }

    
}



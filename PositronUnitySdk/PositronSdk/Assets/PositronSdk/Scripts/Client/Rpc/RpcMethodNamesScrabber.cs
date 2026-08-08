#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assembly = System.Reflection.Assembly;
using System.Linq;
using System.Reflection;
using Positron.Client.Settings;
using Positron.Client.Rpc;

namespace Positron.UnsafeEditor
{
    [InitializeOnLoad]
    public static class RpcMethodNamesScrabber
    {
        static RpcMethodNamesScrabber()
        {
            OnDomainReload();
        }

        private static void OnDomainReload()
        {
            if (Application.isPlaying)
            {
                return;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> types = new();

            foreach (Assembly asm in assemblies)
            {
                string name = asm.GetName().Name;
                string[] banlist = new string[]
                {
                "Mono",
                "Microsoft",
                "Interop",
                "Interop+",
                "System",
                "Bee",
                "Unity",
                "UnityEngine",
                "UniTask",
                "I18N",
                "netstandard",
                "PlayerBuildProgramLibrary",
                "ScriptCompilationBuildProgram",
                "NuGetForUnity",
                "JetBrains",
                "K4os",
                "unityplastic",
                "nunit",
                "log4net",
                "MessagePack",
                "NugetForUnity",
                "Newtonsoft",
                "mscorlib",
                "endel",
                "PPv2URPConverters",
                "Domain_Reload"
                };

                bool found = false;

                foreach (string banword in banlist)
                {
                    if (name.StartsWith(banword) || name.StartsWith(banword + "."))
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    continue;
                }

                types.AddRange(asm.GetTypes());
            }

            List<string> methodNames = new();

            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (MethodInfo method in methods)
                {
                    if (method.CustomAttributes.Any((x) => x.AttributeType == typeof(RpcAttribute)))
                    {
                        if (methodNames.Contains(method.Name))
                        {
                            Debug.LogError($"Method of name '{method.Name}' in type '{type.FullName}' is not unique by name, All PRCs must be globally unique by name");
                            continue;
                        }

                        methodNames.Add(method.Name);
                    }
                }
            }

            PositronSettings settings = Resources.Load<PositronSettings>(PositronSettings.RESOURCES_PATH);

            if (settings == null)
            {
                Debug.LogError("Unable to load PositronSettings at standart path");
                return;
            }

            settings.SetRpcMapping(methodNames.ToArray());
        }
    }
}
#else
public sealed class RpcMethodNamesScrabber { }
#endif
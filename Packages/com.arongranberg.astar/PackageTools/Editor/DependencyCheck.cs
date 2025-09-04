// Disable the warning: "Field 'DependencyCheck.Dependency.name' is never assigned to, and will always have its default value null"
#pragma warning disable 649
using UnityEditor;
using System.Linq;
using UnityEngine;

namespace Pathfinding.Util {
	[InitializeOnLoad]
	static class DependencyCheck {
		struct Dependency {
			public string name;
			public string version;
		}

		static DependencyCheck() {
			var missingDependencies = new Dependency[] {
#if !MODULE_BURST
				new Dependency {
					name = "com.unity.burst",
					version = "1.8.3",
				},
#endif
#if !MODULE_MATHEMATICS
				new Dependency {
					name = "com.unity.mathematics",
					version = "1.2.6",
				},
#endif
#if !MODULE_COLLECTIONS
				new Dependency {
					name = "com.unity.collections",
					version = "1.5.1",
				},
#endif
				// #if !MODULE_ENTITIES
				// new Dependency {
				// 	name = "com.unity.entities",
				// 	version = "1.1.0-pre.3",
				// },
				// #endif
			};

			if (missingDependencies.Length > 0) {
				string missing = string.Join(", ", missingDependencies.Select(p => p.name + " (" + p.version + ")"));
				bool res = EditorUtility.DisplayDialog("Missing dependencies", "The packages " + missing + " are required by the A* Pathfinding Project but they are not installed, or the installed versions are too old. Do you want to install the latest versions of the packages?", "Ok", "Cancel");
				if (res) {
					foreach (var dep in missingDependencies) {
						UnityEditor.PackageManager.Client.Add(dep.name);
					}
				}
			}

			// E.g. 2023.3.0b8
			var v = Application.unityVersion.Split('.');
			UnityEngine.Assertions.Assert.IsTrue(v.Length >= 3, "Unity version string is not in the expected format");
			var major = int.Parse(v[0]);
			var minor = int.Parse(v[1]);
			// Filter out non-digits from v[2]
			v[2] = new string(v[2].TakeWhile(char.IsDigit).ToArray());
			var patch = int.Parse(v[2]);
			if (major == 2022 && minor == 3 && patch < 21) {
				Debug.LogError("This version of Unity has a bug which causes components in the A* Pathfinding Project to randomly stop working. Please update to unity 2022.3.21 or later.");
			}
		}
	}
}

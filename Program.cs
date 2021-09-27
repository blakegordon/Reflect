using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reflect2 {
	class Program {
		static void Main(string[] args) {
			if (args.Length >= 1) {
				try {
					string targetDir = Path.GetDirectoryName(args[0]);
					if (!string.IsNullOrEmpty(targetDir)) {
						AppDomain.CurrentDomain.AppendPrivatePath(targetDir);
					}
					Assembly a = Assembly.LoadFrom(args[0]);
					Console.WriteLine();
					foreach (Type t in a.GetTypes().Where(f => f.IsClass && !f.Attributes.HasFlag(TypeAttributes.NestedPrivate)).OrderBy(f => f.Name)) {
						Console.WriteLine(t.Name);
						foreach (MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
							.Union(t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
							.Union(t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
							.Union(t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
							.OrderBy(m => m.Name)) {
							Console.WriteLine("\t" + m.Name + "()");
						}
					}
				}
				catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is BadImageFormatException || ex is ReflectionTypeLoadException) {
					Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
					Console.WriteLine();
					Console.WriteLine("\t" + ex.InnerException?.GetType()?.Name + "\t" + ex.InnerException?.Message);
					Environment.Exit(1);
				}
			}
		}
	}
}
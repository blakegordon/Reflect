using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reflect2 {
	class Program {
		static void Main(string[] args) {
			if (args.Length >= 1) {
				try {
					if (!File.Exists(args[0])) {
						Console.WriteLine("File does not exist.");
						Environment.Exit(1);
					}
					string extension = Path.GetExtension(args[0]).ToLowerInvariant();
					if (extension != ".dll" && extension != ".exe") {
						Console.WriteLine("File must be a .dll or .exe assembly.");
						Environment.Exit(2);
					}
					string targetDir = Path.GetDirectoryName(args[0]);
					AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, e) => {
						string depName = e.Name.Split(',')[0];
						string depPath = Path.Combine(targetDir, depName + ".dll");
						if (File.Exists(depPath)) {
							return Assembly.ReflectionOnlyLoadFrom(depPath);
						} else {
							try {
								return Assembly.ReflectionOnlyLoad(e.Name);
							} catch {
								return null;
							}
						}
					};
					Assembly a = Assembly.ReflectionOnlyLoadFrom(args[0]);
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
				catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is BadImageFormatException || ex is ReflectionTypeLoadException || ex is FileNotFoundException || ex is FileLoadException) {
					Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
					Console.WriteLine();
					Console.WriteLine("\t" + ex.InnerException?.GetType()?.Name + "\t" + ex.InnerException?.Message);
					
					foreach(Exception le in (ex as ReflectionTypeLoadException)?.LoaderExceptions ?? Array.Empty<Exception>()) {
						Console.WriteLine("\t" + le.GetType().Name + "\t" + le.Message);
					}
					Environment.Exit(3);
				}
			}
		}
	}
}

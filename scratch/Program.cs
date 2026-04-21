using System;
using System.Linq;
using System.Reflection;

var assemblies = new[] {
    "GameFinder.StoreHandlers.Steam",
    "GameFinder.StoreHandlers.GOG",
    "GameFinder.StoreHandlers.EGS",
    "GameFinder.StoreHandlers.EADesktop",
    "GameFinder.StoreHandlers.Xbox"
};

foreach (var name in assemblies)
{
    try {
        var asm = Assembly.Load(name);
        foreach (var type in asm.GetTypes().Where(t => t.IsPublic))
        {
            Console.WriteLine($"Type: {type.FullName}");
            foreach (var ctor in type.GetConstructors())
            {
                var p = string.Join(", ", ctor.GetParameters().Select(x => x.ParameterType.Name));
                Console.WriteLine($"  Ctor: ({p})");
            }
            foreach (var prop in type.GetProperties())
            {
                Console.WriteLine($"  Prop: {prop.PropertyType.Name} {prop.Name}");
            }
        }
    } catch { }
}

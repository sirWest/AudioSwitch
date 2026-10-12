using System.Reflection;
using System.Runtime.InteropServices;
using AudioSwitch.Core.CoreAudio;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class NativeImportTests
{
    [Fact]
    public void GeneratedImportsResolveExactNativeExports()
    {
        Assembly[] assemblies =
        [
            typeof(PropVariant).Assembly,
            typeof(Native).Assembly,
            typeof(NativeImportTests).Assembly,
        ];
        foreach (var assembly in assemblies)
        {
            var imports = assembly
                .GetTypes()
                .SelectMany(type =>
                    type.GetMethods(
                        BindingFlags.Static
                            | BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.DeclaredOnly
                    )
                )
                .Select(method =>
                    (Method: method, Import: method.GetCustomAttribute<LibraryImportAttribute>())
                )
                .Where(item => item.Import is not null)
                .ToArray();
            Assert.NotEmpty(imports);
            foreach (var (method, import) in imports)
            {
                var library = NativeLibrary.Load(
                    import!.LibraryName,
                    assembly,
                    DllImportSearchPath.System32
                );
                try
                {
                    var entryPoint = import.EntryPoint ?? method.Name;
                    Assert.True(
                        NativeLibrary.TryGetExport(library, entryPoint, out _),
                        $"{method.DeclaringType!.FullName}.{method.Name}: {import.LibraryName}!{entryPoint} was not found."
                    );
                }
                finally
                {
                    NativeLibrary.Free(library);
                }
            }
        }
    }
}

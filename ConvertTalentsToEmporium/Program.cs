using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ConvertTalentsToEmporium
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton(new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = false
                })
                .AddSingleton(new DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .Build()
                )
                .AddSingleton<SourceConverter>()
                .AddSingleton<EmporiumBuilder>()
                .AddSingleton<DestinationWriter>()
                .AddSingleton<ConversionProcessor>()
                .AddSingleton<PathEnforcer>()
                .AddSingleton<StringBuilder>()
                .AddSingleton<EmporiumTalentBuilder>()
                .BuildServiceProvider()
                ;

            var pathEnforcer = serviceProvider.GetService<PathEnforcer>();
            var conversionProcessor = serviceProvider.GetService<ConversionProcessor>();

            Console.WriteLine("Starting...");
            var (sourcePaths, destinationPath) = pathEnforcer.FindPath(args);
            await conversionProcessor.ConvertFiles(sourcePaths, destinationPath);
            Console.WriteLine("Done!");
        }
    }
}

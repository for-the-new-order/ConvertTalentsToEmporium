using System;
using System.IO;
using System.Threading.Tasks;

namespace ConvertTalentsToEmporium
{
    public class DestinationWriter
    {
        private readonly EmporiumBuilder _emporiumBuilder;

        public DestinationWriter(EmporiumBuilder emporiumBuilder)
        {
            _emporiumBuilder = emporiumBuilder ?? throw new ArgumentNullException(nameof(emporiumBuilder));
        }

        public async Task SaveAsync(string destinationPath)
        {
            var json = _emporiumBuilder.ToJson();
            await File.WriteAllTextAsync(destinationPath, json);
        }
    }
}

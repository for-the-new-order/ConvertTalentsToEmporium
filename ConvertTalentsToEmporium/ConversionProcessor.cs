using System;
using System.Threading.Tasks;

namespace ConvertTalentsToEmporium
{
    public class ConversionProcessor
    {
        private readonly EmporiumBuilder _emporiumBuilder;
        private readonly SourceConverter _sourceConverter;
        private readonly DestinationWriter _destinationWriter;

        public ConversionProcessor(EmporiumBuilder emporiumBuilder, SourceConverter sourceConverter, DestinationWriter destinationWriter)
        {
            _emporiumBuilder = emporiumBuilder ?? throw new ArgumentNullException(nameof(emporiumBuilder));
            _sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
            _destinationWriter = destinationWriter ?? throw new ArgumentNullException(nameof(destinationWriter));
        }

        public async Task ConvertFiles(string[] sourcePaths, string destinationPath)
        {
            _emporiumBuilder.Begin();
            foreach (var source in sourcePaths)
            {
                await _sourceConverter.ConvertAsync(source, _emporiumBuilder);
            }
            _emporiumBuilder.End();
            await _destinationWriter.SaveAsync(destinationPath);
        }
    }
}

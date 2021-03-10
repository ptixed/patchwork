using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Patchwork
{
    public class YamlBoolEventEmitter : ChainedEventEmitter
    {
        public YamlBoolEventEmitter(IEventEmitter next) : base(next)
        {
        }

        public override void Emit(ScalarEventInfo evt, IEmitter emitter)
        {
            if (evt.Source.Type == typeof(string) && Constants.YamlBoolRegex.IsMatch(evt.Source.Value?.ToString()))
                evt.Style = ScalarStyle.DoubleQuoted;
            nextEmitter.Emit(evt, emitter);
        }
    }
}

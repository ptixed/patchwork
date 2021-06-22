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
            if (evt.Source.Type == typeof(string))
            {
                var value = evt.Source.Value?.ToString();
                if (Constants.YamlBoolRegex.IsMatch(value))
                    evt.Style = ScalarStyle.DoubleQuoted;
                else if (double.TryParse(value, out double _))
                    evt.Style = ScalarStyle.DoubleQuoted;
                else if (evt.Source.Value?.ToString().Contains('\n') == true)
                    evt.Style = ScalarStyle.Literal;
            }

            nextEmitter.Emit(evt, emitter);
        }
    }
}

using System.Diagnostics;
using ScpControl.Profiler;

namespace ScpControl.Plugins.Mapper
{
    public class RetroArchProfile : IScpMapperProfile
    {
        private readonly Stopwatch _time = Stopwatch.StartNew();

        public void Process(ScpHidReport report)
        {
            if (!report[Ds3Button.Triangle].IsPressed) return;
            
            if (_time.ElapsedMilliseconds < 100) return;

            report.Set(Ds3Button.Triangle, false);
            _time.Restart();
        }
    }
}

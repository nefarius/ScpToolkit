using System.Diagnostics;
using ScpControl.Profiler;

namespace ScpControl.Plugins.Mapper
{
    public class RetroArchProfile : IScpMapperProfile
    {
        private readonly Stopwatch _time = Stopwatch.StartNew();
        private uint _offCounter = 0;

        public void Process(ScpHidReport report)
        {
            if (!report[Ds3Button.Triangle].IsPressed) return;
            
            if (_time.ElapsedMilliseconds < 100) return;

            report.Set(Ds3Button.Triangle, false);

            if (_offCounter++ != 10) return;

            _time.Restart();
            _offCounter = 0;
        }
    }
}

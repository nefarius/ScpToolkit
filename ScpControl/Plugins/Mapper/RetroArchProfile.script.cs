using System.Diagnostics;
using ScpControl.Profiler;

namespace ScpControl.Plugins.Mapper
{
    public class RetroArchProfile : IScpMapperProfile
    {
        private readonly Stopwatch _time = Stopwatch.StartNew();
        private uint _offCounter;

        public void Process(ScpHidReport report)
        {
            if (!report[Ds3Button.Triangle].IsPressed) return;

            if (_time.ElapsedMilliseconds < 50) return;

            report.Set(Ds3Button.Triangle, false);

            if (_offCounter++ != 5) return;

            _time.Restart();
            _offCounter = 0;
        }

        public string Name
        {
            get { return "RetroArch Turbo-X"; }
        }

        public string Description
        {
            get { return "Demo-Plugin which enables turbo-mode on the X button for playing Castlevania: SotN ;)"; }
        }

        public string Author
        {
            get { return "Nefarius"; }
        }

        public bool IsActive
        {
            get { return true; }
        }
    }
}
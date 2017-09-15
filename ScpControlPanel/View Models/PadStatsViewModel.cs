using System.Collections.Generic;

namespace ScpControlPanel.View_Models
{
    public class PadStatsViewModel
    {
        public static IList<PadStatsViewModel> Pads
        {
            get
            {
                return new List<PadStatsViewModel>
                {
                    new PadStatsViewModel {Id = 1, Type = "DS3"},
                    new PadStatsViewModel {Id = 2, Type = "Ds4"}
                };
            }
        }

        public int Id { get; set; }
        public string Type { get; set; }
    }
}
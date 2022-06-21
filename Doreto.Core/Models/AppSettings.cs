using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doreto.Core.Models;

public class AppSettings
{
    public bool IsTopMost { get; set; }
    public double WindowXCoordinate { get; set; }
    public double WindowYCoordinate { get; set; }
    public bool SwitchOnExchangeDemand { get; set; }
    public bool SwitchOnGameTurnStart { get; set; }
    public bool SwitchOnGroupInvite { get; set; }
}

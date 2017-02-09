using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mediation.Interfaces
{
    interface IMediationEdge
    {
        // Edges have a player action.
        IOperator Action { get; set; }
    }
}

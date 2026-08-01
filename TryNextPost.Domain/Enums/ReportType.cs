using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Enums
{
    public enum ReportType
    {
        CustomReport = 1,           
        ShipmentSummary = 2,
        TopNdrReasons = 3,
        DailySummary = 4,
        StateWiseSummary = 5,
        ProductWiseSummary = 6,
        CourierWiseSummary = 7,
        ChannelWiseSummary = 8,
        ZoneWiseSummary = 9
    }
}

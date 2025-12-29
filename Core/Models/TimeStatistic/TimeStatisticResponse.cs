
namespace Core.Models.TimeStatistic;

public class TimeStatisticResponse
{
    public int TotalSeconds { get; set; }
    public int SessionsCount { get; set; }
    public List<TimeStatisticDTO> Items { get; set; } = [];
}

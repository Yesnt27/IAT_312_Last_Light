// How the player handled the ships: for one night (GameState.Tonight) or the whole game (GameState.Total).
// A ship counts as failed when it wasn't identified correctly: an abnormal (ghost) ship marked Safe and let
// through, a normal ship Reported, or any ship not reported before the time ran out.
[System.Serializable]
public class ShipStats
{
    public int guided;
    public int caught;
    public int missed;
    public int falseAlarms;
    public int notReported;

    public int Correct => guided + caught;
    public int Failed => missed + falseAlarms + notReported;
    public int Ships => Correct + Failed;

    public void Clear()
    {
        guided = 0;
        caught = 0;
        missed = 0;
        falseAlarms = 0;
        notReported = 0;
    }
}

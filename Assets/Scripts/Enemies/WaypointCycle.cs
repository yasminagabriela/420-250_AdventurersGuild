public class WaypointCycle {
    private readonly int _count;
    private readonly bool _pingPong;
    private int _index;
    private int _step = 1;


    public WaypointCycle(int count, bool pingPong) {
        _count = count;
        _pingPong = pingPong;
    }

    public int Next() {
        if (_count <= 1) return 0;

        if (_pingPong)
        {
            if (_index + _step >= _count || _index + _step < 0)
                _step = -_step;
            _index += _step;
        }
        else
        {
            _index = (_index + 1) % _count;
        }
        return _index;
    }
}
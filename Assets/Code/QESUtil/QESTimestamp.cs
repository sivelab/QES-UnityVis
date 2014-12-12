
public class QESTimestamp {
	public QESTimestamp(int y, int m, int d, int h, int min, int s) {
		Year = y;
		Month = m;
		Day = d;
		Hour = h;
		Minute = min;
		Second = s;
	}

	public int Year { get; private set; }
	public int Month { get; private set; }
	public int Day { get; private set; }
	public int Hour { get; private set; }
	public int Minute { get; private set; }
	public int Second { get; private set; }
}

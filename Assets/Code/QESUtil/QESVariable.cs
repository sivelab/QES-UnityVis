
public class QESVariable {
	public QESVariable(string name, string longname, string unit, float min, float max, float mean, float stdev) {
		Name = name;
		Longname = longname;
		Unit = unit;
		Min = min;
		Max = max;
		Mean = mean;
		Stdev = stdev;
	}

	public string Name { get; private set; }
	public string Longname { get; private set;}
	public string Unit { get; private set; }
	public float Min { get; private set; }
	public float Max { get; private set; }
	public float Mean { get; private set; }
	public float Stdev { get; private set; }
}

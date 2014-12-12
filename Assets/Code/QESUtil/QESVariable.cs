
public class QESVariable {
	public QESVariable(string name, string longname, string unit, float min, float max) {
		Name = name;
		Longname = longname;
		Unit = unit;
		Min = min;
		Max = max;
	}

	public string Name { get; private set; }
	public string Longname { get; private set;}
	public string Unit { get; private set; }
	public float Min { get; private set; }
	public float Max { get; private set; }
}

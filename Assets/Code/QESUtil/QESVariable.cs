
public class QESVariable {

	public enum Type { PATCH, AIRCELL }

	public QESVariable(string name, string longname, string unit, float min, float max, Type t) {
		Name = name;
		Longname = longname;
		Unit = unit;
		Min = min;
		Max = max;
		type = t;
	}

	public string Name { get; private set; }
	public string Longname { get; private set;}
	public string Unit { get; private set; }
	public float Min { get; private set; }
	public float Max { get; private set; }
	public Type type { get; private set; }
}

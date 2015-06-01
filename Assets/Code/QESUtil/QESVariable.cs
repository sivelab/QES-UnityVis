/// <summary>
/// Represents a variable in the QES file format
/// </summary>
public class QESVariable {

	/// Whether the variable represents a value defined on a 2D surface or a 3D volume
	public enum Type { PATCH, AIRCELL }

	public QESVariable(string name, string longname, string unit, float min, float max, Type t) {
		Name = name;
		Longname = longname;
		Unit = unit;
		Min = min;
		Max = max;
		type = t;
	}

	// Short name used internally to refer to variable (and used for filenames)
	public string Name { get; private set; }

	// Long, human-readable filename
	public string Longname { get; private set;}

	// Units for the variable.  TODO: define a convention for representing units
	// One option is to have units separated by periods, implicity raised to 1 or
	// whatever integer is specified.  (I think this is the convention used in GRIB)
	// Then, for human display, we can convert into a 'nice' form.
	// For example:
	//
	// Watts per sqaure meter: W.m-2
	// Kilogram-meters per second: kg.m.s-1
	// 
	public string Unit { get; private set; }

	// Minimum value for this variable across all timesteps
	public float Min { get; private set; }

	// Maximum value for this variable across all timesteps
	public float Max { get; private set; }

	// Type of variable (PATCH or AIRCELL)
	public Type type { get; private set; }
}

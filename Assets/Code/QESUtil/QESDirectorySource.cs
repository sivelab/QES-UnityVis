/// <summary>
/// QESDataSource that sources data from a filesystem location
/// </summary>

public class QESDirectorySource : IQESDataSource
{

	public QESDirectorySource (string dir)
	{
		directory = dir;
		if (!System.IO.Directory.Exists (directory)) {
			throw new System.IO.FileNotFoundException ("Directory doesn't exist");
		}
	}

	string IQESDataSource.TextFileContents (string filename)
	{
		string fullpath = System.IO.Path.Combine (directory, filename);
		if (!System.IO.File.Exists (fullpath)) {
			throw new System.IO.FileNotFoundException("File " + filename + " doesn't exist");
		}
		return System.IO.File.ReadAllText (fullpath);
	}

	byte[] IQESDataSource.BinaryFileContents (string filename)
	{
		string fullpath = System.IO.Path.Combine (directory, filename);
		return System.IO.File.ReadAllBytes (fullpath);
	}
	
	string[] IQESDataSource.ListAllFiles ()
	{
		return System.IO.Directory.GetFiles (directory);
		;
	}

	private string directory;
}

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

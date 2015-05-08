
public interface IQESDataSource
{
	/// <summary>
	/// Returns the file contents of the specified file, interpreted as text.
	/// </summary>
	/// <returns>The file contents.</returns>
	/// <param name="filename">Filename.</param>
	string TextFileContents(string filename);

	/// <summary>
	/// Returns bytes representing the given file
	/// </summary>
	/// <returns>The file contents.</returns>
	/// <param name="filename">Filename.</param>
	byte[] BinaryFileContents(string filename);

	/*
	/// <summary>
	/// Determines whether given file exists
	/// </summary>
	/// <returns><c>true</c>, if file exists, <c>false</c> otherwise.</returns>
	/// <param name="filename">Filename.</param>
	bool FileExists(string filename);
*/
	/// <summary>
	/// List all files available for reading
	/// </summary>
	/// <returns>All files</returns>
	string[] ListAllFiles();
}

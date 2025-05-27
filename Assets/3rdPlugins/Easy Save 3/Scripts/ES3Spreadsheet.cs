using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ES3Internal;

#if UNITY_VISUAL_SCRIPTING
[Unity.VisualScripting.IncludeInSettings(true)]
#elif BOLT_VISUAL_SCRIPTING
[Ludiq.IncludeInSettings(true)]
#endif
public class ES3Spreadsheet
{
	private int cols = 0;
	private int rows = 0;
	private Dictionary<Index, string> cells = new Dictionary<Index, string>();

	private const string QUOTE = "\"";
	private const char QUOTE_CHAR = '"';
	private const char COMMA_CHAR = ',';
	private const char NEWLINE_CHAR = '\n';
	private const string ESCAPED_QUOTE = "\"\"";
	private static char[] CHARS_TO_ESCAPE = { ',', '"', '\n'};

	public int ColumnCount
	{
		get{ return cols; }
	}

	public int RowCount
	{
		get{ return rows; }
	}

    public int GetColumnLength(int col)
    {
        if (col >= cols)
            return 0;

        int maxRow = -1;

        foreach(var index in cells.Keys)
            if (index.col == col && index.row > maxRow)
                maxRow = index.row;

        return maxRow+1;
    }

    public int GetRowLength(int row)
    {
        if (row >= rows)
            return 0;

        int maxCol = -1;

        foreach (var index in cells.Keys)
            if (index.row == row && index.col > maxCol)
                maxCol = index.col;

        return maxCol + 1;
    }

    public void SetCell(int col, int row, object value)
    {
		if(value == null)
		{
            SetCellString(col, row, null);
			return;
        }

        var type = value.GetType();

		// If we're writing a string, add it without formatting.
		if (type == typeof(string))
		{
			SetCellString(col, row, (string)value);
			return;
		}

        var settings = new ES3Settings(ES3.Location.File);
        if (ES3Reflection.IsPrimitive(type))
            SetCellString(col, row, value.ToString());
        else
            SetCellString(col, row, settings.encoding.GetString(ES3.Serialize(value, ES3TypeMgr.GetOrCreateES3Type(type))));

        // Expand the spreadsheet if necessary.
        if (col >= cols)
            cols = (col + 1);
        if (row >= rows)
            rows = (row + 1);
    }

    private void SetCellString(int col, int row, string value)
	{
		cells [new Index (col, row)] = value;

		// Expand the spreadsheet if necessary.
		if(col >= cols)
			cols = (col+1);
        if (row >= rows)
            rows = (row + 1);
    }


    // Don't create non-generic version of this. Generic parameter is necessary as no type data is stored in the CSV file.
    public T GetCell<T>(int col, int row)
	{
        var val = GetCell(typeof(T), col, row);

        if (val == null)
            return default(T);

        return (T)val;
	}

    public object GetCell(System.Type type, int col, int row)
    {
        string value;

        if (col >= cols || row >= rows)
            throw new System.IndexOutOfRangeException("Cell (" + col + ", " + row + ") is out of bounds of spreadsheet (" + cols + ", " + rows + ").");

        if (!cells.TryGetValue(new Index(col, row), out value) || value == null)
            return null;

        // If we're loading a string, simply return the string value.
        if (type == typeof(string))
        {
            var str = (object)value;
            return str;
        }

        var settings = new ES3Settings(ES3.Location.File);
        return ES3.Deserialize(ES3TypeMgr.GetOrCreateES3Type(type, true), settings.encoding.GetBytes(value), settings);
    }

    public void Load(string filePath)
	{
		Load(new ES3Settings (filePath));
	}

	public void Load(string filePath, ES3Settings settings)
	{
		Load(new ES3Settings (filePath, settings));
	}

	public void Load(ES3Settings settings)
	{
        // Spreadsheets can't be read from cache, so read from file instead.
        if (settings.location == ES3.Location.Cache)
        {
            settings = (ES3Settings)settings.Clone();
            settings.location = ES3.Location.File;
        }

        Load(ES3Stream.CreateStream(settings, ES3FileMode.Read), settings);
	}

	public void LoadRaw(string str)
	{
		Load(new MemoryStream (((new ES3Settings (ES3.Location.File)).encoding).GetBytes(str)), new ES3Settings(ES3.Location.File));
	}

	public void LoadRaw(string str, ES3Settings settings)
	{
		Load(new MemoryStream ((settings.encoding).GetBytes(str)), settings);
	}

	private void Load(Stream stream, ES3Settings settings)
	{
		using (var reader = new StreamReader(stream))
		{
			int c_int;
			char c;
			string value = null;
			int col = 0;
			int row = 0;

			// Read until the end of the stream.
			while(true)
			{
				c_int = reader.Read();
				c = (char)c_int;
				if(c == QUOTE_CHAR)
				{
					while (true)
					{
						c = (char)reader.Read();

						if(c == QUOTE_CHAR)
						{
							// If this quote isn't escaped by another, it is the last quote, so we should stop parsing this value.
							if(((char)reader.Peek()) != QUOTE_CHAR)
								break;
							else
								c = (char)reader.Read();
						}
						value += c;
					}
				}
				// If this is the end of a column, row, or the stream, add the value to the spreadsheet.
				else if(c == COMMA_CHAR || c == NEWLINE_CHAR || c_int == -1)
				{
					SetCell(col, row, value);
					value = null;
					if(c == COMMA_CHAR)
						col++;
					else if(c == NEWLINE_CHAR)
					{
						col = 0;
						row++;
					}
					else
						break;
				}
				else
					value += c;
			}
		}
    }

	public void Save(string filePath)
	{
		Save(new ES3Settings (filePath), false);
	}

	public void Save(string filePath, ES3Settings settings)
	{
		Save(new ES3Settings (filePath, settings), false);
	}

	public void Save(ES3Settings settings)
	{
		Save(settings, false);
	}

	public void Save(string filePath, bool append)
	{
		Save(new ES3Settings (filePath), append);
	}

	public void Save(string filePath, ES3Settings settings, bool append)
	{
		Save(new ES3Settings (filePath, settings), append);
	}

	public void Save(ES3Settings settings, bool append)
	{
		// Spreadsheets can't be written to cache, so write to file instead.
		if (settings.location == ES3.Location.Cache)
		{
			settings = (ES3Settings)settings.Clone();
			settings.location = ES3.Location.File;
		}

		using (var writer = new StreamWriter(ES3Stream.CreateStream(settings, append ? ES3FileMode.Append : ES3FileMode.Write)))
		{
			// If data already exists and we're appending, we need to prepend a newline.
			if(append && ES3.FileExists(settings))
				writer.Write(NEWLINE_CHAR);

			var array = ToArray();
			for(int row = 0; row < rows; row++)
			{
				if(row != 0)
					writer.Write(NEWLINE_CHAR);

				for(int col = 0; col < cols; col++)
				{
					if(col != 0)
						writer.Write(COMMA_CHAR);

                    writer.Write( Escape(array [col, row]) );
				}
			}
		}
		if(!append)
			ES3IO.CommitBackup(settings);
	}

	private static string Escape(string str, bool isAlreadyWrappedInQuotes=false)
	{
        if (str == "")
            return "\"\"";
		else if(str == null)
			return null;

		// Now escape any other quotes.
		if(str.Contains(QUOTE))
			str = str.Replace(QUOTE, ESCAPED_QUOTE);
		
		// If there's chars to escape, wrap the value in quotes.
		if(str.IndexOfAny(CHARS_TO_ESCAPE) > -1 || StartsOrEndsWithWhitespace(str))
			str = QUOTE + str + QUOTE;
		return str;
	}

	private static string Unescape(string str)
	{
		if(str.StartsWith(QUOTE) && str.EndsWith(QUOTE))
		{
			str = str.Substring(1, str.Length-2);
			if(str.Contains(ESCAPED_QUOTE))
				str = str.Replace(ESCAPED_QUOTE, QUOTE);
		}
		return str;
	}

	private static bool StartsOrEndsWithWhitespace(string str)
	{
		if (string.IsNullOrEmpty(str))
			return false;

		if (char.IsWhiteSpace(str[0]))
			return true;

		if (char.IsWhiteSpace(str[str.Length - 1]))
			return true;

		return false;
	}

	private string[,] ToArray()
	{
		var array = new string[cols, rows];
		foreach (var cell in cells)
			array [cell.Key.col, cell.Key.row] = cell.Value;
		return array;
	}

	protected struct Index
	{
		public int col;
		public int row;

		public Index(int col, int row)
		{
			this.col = col;
			this.row = row;
		}
	}
}
